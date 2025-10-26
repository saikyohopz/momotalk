using Arkko.MomoTalk.OneBot;
using Arkko.MomoTalk.OneBot.Models;

namespace Arkko.MomoTalk.NapCat;

public static class OneBotApiExtensions {
    // these apis are maybe provided only by napcat
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
    }
}
