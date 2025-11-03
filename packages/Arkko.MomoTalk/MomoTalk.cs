using Arkko.MomoTalk.Common;
using Arkko.MomoTalk.Hosting;
using Arkko.MomoTalk.OneBot;
using Arkko.MomoTalk.OneBot.Protocol.Enums;
using Arkko.MomoTalk.OneBot.Protocol.Messages;
using Arkko.MomoTalk.OneBot.Protocol.Models;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;

namespace Arkko.MomoTalk;

public class MomoTalk {
    internal OneBotClient OneBot { get; }
    
    private readonly ILogger<MomoTalk> _logger;

    public long BotId {
        get => !OneBot.IsConnected ? throw new WebSocketException("bot not connected") : field;
        private set;
    }

    public MomoTalk(MomoTalkConfig config, ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<MomoTalk>();
        
        OneBot = new OneBotClient(
            config.WebSocketUrl, config.Token, config.RetryTimes, config.RetryWait,
            config.RetryRest, config.WebSocketHeartbeat, loggerFactory, this
        );
    }

    public async Task<ObMessageId?> SendPrivateMessage(long userId, MessageChain messageChain) {
        return await OneBot.Apis.SendPrivateMessage(userId, messageChain);
    }

    public async Task<ObMessageId?> SendGroupMessage(long groupId, MessageChain messageChain) {
        return await OneBot.Apis.SendGroupMessage(groupId, messageChain);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="userId">用户ID，不传时视为发送群消息</param>
    /// <param name="groupId">群ID，不传时视为发送私聊消息</param>
    /// <param name="messageChain">消息链</param>
    /// <returns>消息ID，部分实现可能不会返回消息ID</returns>
    public async Task<ObMessageId?> SendMessage(long? userId, long? groupId, MessageChain messageChain) {
        if (userId == null && groupId == null) {
            throw new ArgumentException("both userId and groupId cannot be null");
        }
        
        return await OneBot.Apis.SendMessage(
            groupId == null ? MessageType.Private : MessageType.Group,
            userId,
            groupId,
            messageChain
        );
    }

    public async Task ConnectAsync() {
        await OneBot.ConnectAsync();

        ObLoginInfo loginInfo = await OneBot.Apis.GetLoginInfo();

        BotId = loginInfo.UserId;

        if (_logger.IsEnabled(LogLevel.Information)) {
            _logger.LogInformation("connected to bot {name}({id})", loginInfo.Nickname, loginInfo.UserId);
        }
    }

    public async Task CloseAsync() {
        await OneBot.CloseAsync();
    }

    public bool IsConnected => OneBot.IsConnected;
}
