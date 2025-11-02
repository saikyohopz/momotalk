using Arkko.MomoTalk.OneBot.Protocol.Messages;
using Arkko.MomoTalk.OneBot.Protocol.Models;

namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public class EventMessageGroup : EventMessage {
    public long GroupId { get; set; }
    public ObAnonymous? Anonymous { get; set; }

    /// <summary>
    /// 回复消息
    /// </summary>
    /// <param name="messageChain">消息链</param>
    /// <param name="atSender">是否@发送该消息的成员</param>
    public async Task ReplyAsync(MessageChain messageChain, bool atSender = true) {
        if (QuickOptInvoker != null) {
            await QuickOptInvoker.Invoke(new {
                Reply = MessagePacker.PackArrayMessages(messageChain),
                AtSender = atSender,
            });
        }
    }

    /// <summary>
    /// 撤回该消息
    /// </summary>
    public async Task RecallAsync() {
        if (QuickOptInvoker != null) {
            await QuickOptInvoker.Invoke(new {
                Delete = true,
            });
        }
    }

    /// <summary>
    /// 踢出发送该消息的成员
    /// </summary>
    public async Task KickAsync() {
        if (QuickOptInvoker != null) {
            await QuickOptInvoker.Invoke(new {
                Kick = true,
            });
        }
    }

    /// <summary>
    /// 禁言发送该消息的成员
    /// </summary>
    /// <param name="span">时长</param>
    public async Task MuteAsync(TimeSpan span) {
        if (QuickOptInvoker != null) {
            await QuickOptInvoker.Invoke(new {
                Ban = true,
                BanDuration = Convert.ToInt32(Math.Round(span.TotalMinutes)),
            });
        }
    }
}
