using Arkko.MomoTalk.Foundation.Utils;
using Arkko.MomoTalk.OneBot.Protocol.Events;
using Arkko.MomoTalk.OneBot.Protocol.Messages;
using Arkko.MomoTalk.OneBot.Protocol.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Arkko.MomoTalk.OneBot;

public class OneBotClientV2 : IOneBotClient, IDisposable, IAsyncDisposable {
    private readonly ConcurrentDictionary<string, ApiTask> _apiTasks;

    private readonly ILogger<OneBotClientV2> _logger;

    private readonly MessagePacker _messagePacker;

    private readonly TimeSpan _retryRest;

    private readonly int _retryTimes;

    private readonly TimeSpan _retryWait;

    private readonly string? _token;

    private readonly Uri _uri;

    private readonly TimeSpan _wsHeartbeat;

    private ClientWebSocket _client;

    private bool _closed = true;

    /// <summary>
    /// 连接Task，关闭连接后该字段将会被复原为未完成的状态
    /// </summary>
    private TaskCompletionSource _connectionTask;

    private CancellationTokenSource? _cts;

    private Task? _runTask;

    public OneBotClientV2(
        string url,
        string token,
        int retryTimes,
        TimeSpan retryWait,
        TimeSpan retryRest,
        TimeSpan wsHeartbeat,
        ILoggerFactory loggerFactory,
        MomoTalk outerBot
    ) {
        Apis = new OneBotApi(this);
        EventHandlers = new OneBotEventHandlerRepository(outerBot, loggerFactory);

        _client = new ClientWebSocket();
        _logger = loggerFactory.CreateLogger<OneBotClientV2>();
        _apiTasks = new ConcurrentDictionary<string, ApiTask>();
        _connectionTask = new TaskCompletionSource();
        _messagePacker = new MessagePacker(loggerFactory);
        _uri = new Uri(url);
        _retryTimes = retryTimes;
        _retryWait = retryWait;
        _retryRest = retryRest;
        _token = token;
        _wsHeartbeat = wsHeartbeat;

        ApplyClientOptions(_client);
    }

    public OneBotEventHandlerRepository EventHandlers { get; }

    public ValueTask DisposeAsync() {
        GC.SuppressFinalize(this);
        _cts?.Dispose();
        _client.Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
        _cts?.Dispose();
        _client.Dispose();
    }

    public bool IsConnected => _client.State == WebSocketState.Open;

    public OneBotApi Apis { get; }

    /// <summary>
    /// 启动连接循环。会阻塞直到首次连接成功；后续断线由内部自动重连。
    /// </summary>
    public async Task ConnectAsync() {
        if (_runTask is { IsCompleted: false }) {
            return;
        }

        _closed = false;
        _cts = new CancellationTokenSource();
        _connectionTask = new TaskCompletionSource();

        _runTask = RunAsync(_cts.Token);

        // 等待首次连接建立
        await _connectionTask.Task;
    }

    public async Task CloseAsync() {
        if (_closed && _runTask == null) {
            return;
        }

        _closed = true;
        _cts?.Cancel();

        if (_logger.IsEnabled(LogLevel.Information)) {
            _logger.LogInformation("websocket connection closed by client");
        }

        if (IsConnected) {
            try {
                await _client.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "closed by client",
                    CancellationToken.None
                );
            } catch {
                // 忽略关闭过程中的异常，由下方等待 runTask 兜底
            }
        }

        if (_runTask != null) {
            try {
                await _runTask;
            } catch {
                // 忽略
            }
        }

        _cts?.Dispose();
        _cts = null;
        _runTask = null;
    }

    public async Task<TResponse> SendApiRequestAsync<TResponse>(ApiRequest request, bool nullable = false) {
        string req = request.SerializeJson(JsonOptions.SnakeCaseInstance);

        ApiTask apiTask = new(typeof(TResponse), nullable);

        _apiTasks.TryAdd(request.Echo, apiTask);

        await _client.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(req)),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );

        return await apiTask.GetDeserializedResultAsync<TResponse>();
    }

    private void ApplyClientOptions(ClientWebSocket client) {
        if (!string.IsNullOrWhiteSpace(_token)) {
            client.Options.SetRequestHeader("Authorization", $"Bearer {_token}");
        }

        client.Options.KeepAliveInterval = _wsHeartbeat;
    }

    private void ResetClientWebSocket() {
        _client.Dispose();
        _client = new ClientWebSocket();
        ApplyClientOptions(_client);
    }

    /// <summary>
    /// 主循环：连接 -> 收消息 -> 断线重连，直到 CloseAsync 被调用。
    /// </summary>
    private async Task RunAsync(CancellationToken ct) {
        while (!_closed && !ct.IsCancellationRequested) {
            bool connected = await TryConnectAsync(ct);

            if (!connected) {
                // 被取消或主动关闭，退出循环
                return;
            }

            // 连接成功，通知等待方
            if (!_connectionTask.Task.IsCompleted) {
                _connectionTask.TrySetResult();
            }

            if (_logger.IsEnabled(LogLevel.Information)) {
                _logger.LogInformation("successfully connected to websocket: {}", _uri.AbsoluteUri);
            }

            // 运行接收循环，直到连接断开或被取消
            await LoopHandleMessageEventAsync(ct);

            // 收到断线，准备重连
            if (!_closed && !ct.IsCancellationRequested) {
                if (_logger.IsEnabled(LogLevel.Warning)) {
                    _logger.LogWarning("websocket disconnected, reconnecting...");
                }

                _connectionTask = new TaskCompletionSource();
                ResetClientWebSocket();

                try {
                    await Task.Delay(_retryWait, ct);
                } catch (OperationCanceledException) {
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 带重试的连接尝试。成功返回 true，被取消返回 false。
    /// </summary>
    private async Task<bool> TryConnectAsync(CancellationToken cancellationToken) {
        int attemptInBatch = 0;

        while (!_closed && !cancellationToken.IsCancellationRequested) {
            attemptInBatch++;

            try {
                if (_logger.IsEnabled(LogLevel.Information)) {
                    _logger.LogInformation("connecting to websocket: {}", _uri.AbsoluteUri);
                }

                using CancellationTokenSource timeoutCts
                    = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

                await _client.ConnectAsync(_uri, timeoutCts.Token);
                return true;
            } catch (Exception e) when (e is OperationCanceledException or WebSocketException) {
                if (cancellationToken.IsCancellationRequested) {
                    return false;
                }

                ResetClientWebSocket();

                if (attemptInBatch < _retryTimes) {
                    if (_logger.IsEnabled(LogLevel.Warning)) {
                        _logger.LogWarning(
                            "connection failed, retrying after {wait} seconds... ({current}/{max})",
                            _retryWait.TotalSeconds, attemptInBatch, _retryTimes
                        );
                    }

                    try {
                        await Task.Delay(_retryWait, cancellationToken);
                    } catch (OperationCanceledException) {
                        return false;
                    }
                } else {
                    attemptInBatch = 0;

                    TimeSpan restDelay = _retryRest == TimeSpan.Zero ? _retryWait : _retryRest;

                    if (_logger.IsEnabled(LogLevel.Error)) {
                        _logger.LogError(
                            "connection failed after {max} retries, retry scheduled after {rest} seconds",
                            _retryTimes, restDelay.TotalSeconds
                        );
                    }

                    try {
                        await Task.Delay(restDelay, cancellationToken);
                    } catch (OperationCanceledException) {
                        return false;
                    }
                }
            }
        }

        return false;
    }

    private async Task LoopHandleMessageEventAsync(CancellationToken ct) {
        while (!_closed && !ct.IsCancellationRequested) {
            try {
                await HandleMessageEventAsync();
            } catch (Exception ex) when (ex is OperationCanceledException or WebSocketException) {
                if (!_closed && !ct.IsCancellationRequested && _logger.IsEnabled(LogLevel.Error)) {
                    _logger.LogError(ex, "websocket error or operation cancelled");
                }

                break;
            } catch (Exception ex) {
                if (_logger.IsEnabled(LogLevel.Error)) {
                    _logger.LogError(ex, "error handling message event");
                }
            }
        }
    }

    private async Task HandleMessageEventAsync() {
        ArraySegment<byte> buffer = new(new byte[1024 * 4]);

        WebSocketReceiveResult result = await _client.ReceiveAsync(buffer, CancellationToken.None);

        Stopwatch watch = new();
        watch.Start();

        if (result.MessageType == WebSocketMessageType.Close) {
            if (_logger.IsEnabled(LogLevel.Warning)) {
                _logger.LogWarning("websocket connection closed");
            }

            _connectionTask = new TaskCompletionSource();
        } else if (result.MessageType == WebSocketMessageType.Text) {
            string message = Encoding.UTF8.GetString(
                buffer.Array ?? [],
                buffer.Offset,
                result.Count
            );

            if (_logger.IsEnabled(LogLevel.Trace)) {
                _logger.LogTrace("received websocket message: {}", message);
            }

            JsonElement json = JsonDocument.Parse(message).RootElement;

            if (json.TryGetProperty("post_type", out JsonElement _)) {
                HandleEvent(in json);
            } else {
                HandleApiRequest(json);
            }
        }

        watch.Stop();

        if (_logger.IsEnabled(LogLevel.Trace)) {
            _logger.LogTrace("message handling elapsed: {} sec", watch.Elapsed.TotalSeconds);
        }
    }

    private void HandleEvent(in JsonElement json) {
        EventBase? ev = null;

        string? postType = json.GetProperty("post_type").GetString();

        if (postType == "message") {
            string? messageType = json.GetProperty("message_type").GetString();

            if (messageType == "private") {
                EventMessagePrivate emp = json.Deserialize<EventMessagePrivate>(JsonOptions.SnakeCaseInstance)!;

                emp.Message = _messagePacker.UnpackArrayMessages(emp, json.GetProperty("message"));

                emp.QuickOptInvoker = async o => await Apis.HandleQuickOperation(emp, o);

                ev = emp;
            } else if (messageType == "group") {
                EventMessageGroup emg = json.Deserialize<EventMessageGroup>(JsonOptions.SnakeCaseInstance)!;

                emg.Message = _messagePacker.UnpackArrayMessages(emg, json.GetProperty("message"));

                emg.QuickOptInvoker = async o => await Apis.HandleQuickOperation(emg, o);

                ev = emg;
            }
        } else if (postType == "meta_event") {
            string? metaEventType = json.GetProperty("meta_event_type").GetString();

            if (metaEventType == "lifecycle") {
                string? subType = json.GetProperty("sub_type").GetString();

                if (subType == "connect") {
                    ev = json.Deserialize<EventConnect>(JsonOptions.SnakeCaseInstance)!;
                }
            } else if (metaEventType == "heartbeat") {
                ev = json.Deserialize<EventHeartbeat>(JsonOptions.SnakeCaseInstance)!;
            }
        } else if (postType == "notice") {
            string? noticeType = json.GetProperty("notice_type").GetString();

            if (noticeType == "notify") {
                string? subType = json.GetProperty("sub_type").GetString();

                if (subType == "poke") {
                    if (json.TryGetProperty("group_id", out JsonElement _)) {
                        ev = json.Deserialize<EventPokeGroup>(JsonOptions.SnakeCaseInstance)!;
                    } else {
                        ev = json.Deserialize<EventPokeFriend>(JsonOptions.SnakeCaseInstance)!;
                    }
                }
            } else if (noticeType == "group_increase") {
                ev = json.Deserialize<EventGroupMemberIncrease>(JsonOptions.SnakeCaseInstance)!;
            } else if (noticeType == "group_decrease") {
                ev = json.Deserialize<EventGroupMemberDecrease>(JsonOptions.SnakeCaseInstance)!;
            }
        }

        if (ev != null) {
            EventHandlers.PostEvent(ev);
        }
    }

    private void HandleApiRequest(JsonElement json) {
        int retcode = json.GetProperty("retcode").GetInt32();

        if (retcode is 1401 or 1403 && _logger.IsEnabled(LogLevel.Error)) {
            _logger.LogError("incorrect token, connection died!");
        }

        string? echo = json.GetProperty("echo").GetString();

        if (echo != null && _apiTasks.TryGetValue(echo, out ApiTask? apiTask)) {
            apiTask.SetResult(json.GetProperty("data"));
        }
    }
}
