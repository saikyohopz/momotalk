using Arkko.MomoTalk.OneBot.Protocol.Enums;
using Arkko.MomoTalk.OneBot.Protocol.Events;
using Arkko.MomoTalk.OneBot.Protocol.Messages;
using Arkko.MomoTalk.OneBot.Protocol.Models;

namespace Arkko.MomoTalk.OneBot;

public class OneBotApi(OneBotClient client) {
    internal OneBotClient Client => client;

    public async Task<ObMessageId?> SendPrivateMessage(long userId, MessageChain messageChain) {
        return await client.SendApiRequestAsync<ObMessageId?>(
            new ApiRequest("send_private_msg", new {
                UserId = userId,
                Message = MessagePacker.PackArrayMessages(messageChain),
            }), true
        );
    }

    public async Task<ObMessageId?> SendGroupMessage(long groupId, MessageChain messageChain) {
        return await client.SendApiRequestAsync<ObMessageId?>(
            new ApiRequest("send_group_msg", new {
                GroupId = groupId,
                Message = MessagePacker.PackArrayMessages(messageChain),
            }), true
        );
    }

    public async Task<ObMessageId?> SendMessage(
        MessageType messageType, long? userId, long? groupId, MessageChain messageChain, bool autoEscape = false
    ) {
        return await client.SendApiRequestAsync<ObMessageId?>(
            new ApiRequest("send_msg", new {
                MessageType = messageType.ToString().ToLower(),
                UserId = userId,
                GroupId = groupId,
                Message = MessagePacker.PackArrayMessages(messageChain),
                AutoEscape = autoEscape,
            }), true
        );
    }

    public async Task DeleteMessage() { }

    public async Task GetMessage() { }

    public async Task GetForwardMessage() { }

    public async Task SendLike() { }

    public async Task SetGroupKick() { }

    public async Task SetGroupBan() { }

    public async Task SetGroupAnonymousBan() { }

    public async Task SetGroupWholeBan() { }

    public async Task SetGroupAdmin() { }

    public async Task SetGroupAnonymous() { }

    public async Task SetGroupCard() { }

    public async Task SetGroupName() { }

    public async Task SetGroupLeave() { }

    public async Task SetGroupSpecialTitle() { }

    public async Task SetFriendAddRequest() { }

    public async Task SetGroupAddRequest() { }

    public async Task<ObLoginInfo> GetLoginInfo() {
        return await client.SendApiRequestAsync<ObLoginInfo>(
            new ApiRequest("get_login_info", null)
        );
    }

    public async Task GetStrangerInfo() { }

    public async Task GetFriendList() { }

    public async Task GetGroupInfo() { }

    public async Task GetGroupList() { }

    public async Task<ObGroupMemberInfo> GetGroupMemberInfo(long groupId, long userId, bool noCache = false) {
        return await client.SendApiRequestAsync<ObGroupMemberInfo>(
            new ApiRequest("get_group_member_info", new {
                GroupId = groupId,
                UserId = userId,
                NoCache = noCache,
            })
        );
    }

    public async Task GetGroupMemberList() { }

    public async Task GetGroupHonorInfo() { }

    public async Task GetCookies() { }

    public async Task GetCsrfToken() { }

    public async Task GetCredentials() { }

    public async Task GetRecord() { }

    public async Task GetImage() { }

    public async Task CanSendImage() { }

    public async Task CanSendRecord() { }

    public async Task<T> GetStatus<T>() where T : ObConnectionStatus {
        return await client.SendApiRequestAsync<T>(
            new ApiRequest("get_status", null)
        );
    }

    public async Task<T> GetVersionInfo<T>() where T : ObVersionInfo {
        return await client.SendApiRequestAsync<T>(
            new ApiRequest("get_version_info", null)
        );
    }

    public async Task SetRestart() { }

    public async Task CleanCache() { }

    public async Task HandleQuickOperation<TEvent>(TEvent context, object operation)
    where TEvent : EventBase {
        await client.SendApiRequestAsync<object>(
            new ApiRequest(".handle_quick_operation", new {
                Context = context,
                Operation = operation,
            }), true
        );
    }
}
