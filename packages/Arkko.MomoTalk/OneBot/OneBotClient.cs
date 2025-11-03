using Arkko.MomoTalk.Foundation.Utils;
using Arkko.MomoTalk.Hosting;
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

public class OneBotClient : IDisposable, IAsyncDisposable {
    private ClientWebSocket _client;

    private readonly ILogger<OneBotClient> _logger;

    private readonly Uri _uri;

    private readonly int _retryTimes;

    private readonly TimeSpan _retryWait;

    private readonly TimeSpan _retryRest;

    /// <summary>
    /// 连接Task，关闭连接后该字段将会被复原为未完成的状态
    /// </summary>
    private TaskCompletionSource _connectionTask;

    private readonly MessagePacker _messagePacker;

    private readonly ConcurrentDictionary<string, ApiTask> _apiTasks;

    public bool IsConnected => _client.State == WebSocketState.Open;

    public OneBotApi Apis { get; }

    public OneBotEventHandlerRepository EventHandlers { get; }

    public OneBotClient(
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
        _logger = loggerFactory.CreateLogger<OneBotClient>();
        _apiTasks = new ConcurrentDictionary<string, ApiTask>();
        _connectionTask = new TaskCompletionSource();
        _messagePacker = new MessagePacker(loggerFactory);
        _uri = new Uri(url);
        _retryTimes = retryTimes;
        _retryWait = retryWait;
        _retryRest = retryRest;

        if (!string.IsNullOrWhiteSpace(token)) {
            _client.Options.SetRequestHeader("Authorization", $"Bearer {token}");
        }

        _client.Options.KeepAliveInterval = wsHeartbeat;
    }

    private void ResetClientWebSocket() {
        _client.Dispose();
        _client = new ClientWebSocket();
    }

    public async Task ConnectAsync() {
        for (int i = 1; i <= _retryTimes; i++) {
            try {
                if (_logger.IsEnabled(LogLevel.Information)) {
                    _logger.LogInformation("connecting to websocket: {}", _uri.AbsoluteUri);
                }

                CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));

                await _client.ConnectAsync(_uri, cts.Token);

                break;
            } catch (Exception e) when (e is OperationCanceledException or WebSocketException) {
                ResetClientWebSocket();

                if (i < _retryTimes) {
                    if (_logger.IsEnabled(LogLevel.Warning)) {
                        _logger.LogWarning(
                            "connection failed, retrying after {wait} seconds... ({current}/{max})",
                            _retryWait.TotalSeconds, i, _retryTimes
                        );
                    }
                } else {
                    if (_retryRest == TimeSpan.Zero) {
                        throw new WebSocketException($"connection failed after {_retryTimes} retries");
                    }
                    
                    if (_logger.IsEnabled(LogLevel.Error)) {
                        _logger.LogError(
                            "connection failed after {max} retries, retry scheduled after {rest} seconds",
                            _retryTimes, _retryRest.TotalSeconds
                        );
                    }

                    i -= _retryTimes;

                    await Task.Delay(_retryRest);
                }
            }
        }

        if (_logger.IsEnabled(LogLevel.Information)) {
            _logger.LogInformation("successfully connected to websocket: {}", _uri.AbsoluteUri);
        }

        _connectionTask.TrySetResult();

        _ = LoopHandleMessageEventAsync();
    }

    public async Task CloseAsync() {
        if (!IsConnected) {
            return;
        }
        
        if (_logger.IsEnabled(LogLevel.Information)) {
            _logger.LogInformation("websocket connection closed by client");
        }

        _connectionTask = new TaskCompletionSource();

        await _client.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "closed by client",
            CancellationToken.None
        );
    }

    private async Task LoopHandleMessageEventAsync() {
        for (;;) {
            try {
                await HandleMessageEventAsync();
            } catch (OperationCanceledException) {
                break;
            } catch (Exception ex) {
                if (_logger.IsEnabled(LogLevel.Error)) {
                    _logger.LogError("error handling message event: {ex}", ex);
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
            // schedule new attempts to connect?
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
                string? subType =  json.GetProperty("sub_type").GetString();

                if (subType == "connect") {
                    ev = json.Deserialize<EventConnect>(JsonOptions.SnakeCaseInstance)!;
                }
            } else if (metaEventType == "heartbeat") {
                ev = json.Deserialize<EventHeartbeat>(JsonOptions.SnakeCaseInstance)!;
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

    async internal Task<TResponse> SendApiRequestAsync<TResponse>(ApiRequest request, bool nullable = false) {
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

    public void Dispose() {
        GC.SuppressFinalize(this);
        _client.Dispose();
    }

    public ValueTask DisposeAsync() {
        GC.SuppressFinalize(this);
        _client.Dispose();
        return ValueTask.CompletedTask;
    }
}
