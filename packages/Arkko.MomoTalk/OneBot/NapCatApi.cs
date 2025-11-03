using Arkko.MomoTalk.OneBot.Protocol.Models;

namespace Arkko.MomoTalk.OneBot;

public static class NapCatApi {
    extension(OneBotApi api) {
        public async Task FriendPoke(long userId) {
            await api.Client.SendApiRequestAsync<object>(new ApiRequest("friend_poke", new {
                UserId = userId,
            }), true);
        }

        public async Task GroupPoke(long groupId, long userId) {
            await api.Client.SendApiRequestAsync<object>(new ApiRequest("friend_poke", new {
                GroupId = groupId,
                UserId = userId,
            }), true);
        }

        public async Task SendPoke(long userId, long? groupId) {
            await api.Client.SendApiRequestAsync<object>(new ApiRequest("send_poke", new {
                UserId = userId,
                GroupId = groupId,
            }), true);
        }
    }
}
