using Arkko.MomoTalk.OneBot.Protocol.Events;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Arkko.MomoTalk.OneBot;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class OneBotEventHandlerRepository {
    private readonly Dictionary<Type, Action<EventBase>> _eventInvokers = [];

    private readonly ILogger<OneBotEventHandlerRepository> _logger;

    internal OneBotEventHandlerRepository(MomoTalk momoTalk, ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<OneBotEventHandlerRepository>();

        RegisterEvent((EventMessagePrivate ev) => EventMessagePrivate?.Invoke(momoTalk, ev));
        RegisterEvent((EventMessageGroup ev) => EventMessageGroup?.Invoke(momoTalk, ev));
        RegisterEvent((EventConnect ev) => EventConnect?.Invoke(momoTalk, ev));
        RegisterEvent((EventOneBotDisable ev) => EventOneBotDisable?.Invoke(momoTalk, ev));
        RegisterEvent((EventOneBotEnable ev) => EventOneBotEnable?.Invoke(momoTalk, ev));
        RegisterEvent((EventHeartbeat ev) => EventHeartbeat?.Invoke(momoTalk, ev));
        RegisterEvent((EventFriendAdd ev) => EventFriendAdd?.Invoke(momoTalk, ev));
        RegisterEvent((EventFriendRecall ev) => EventFriendRecall?.Invoke(momoTalk, ev));
        RegisterEvent((EventGroupAdmin ev) => EventGroupAdmin?.Invoke(momoTalk, ev));
        RegisterEvent((EventGroupBan ev) => EventGroupBan?.Invoke(momoTalk, ev));
        RegisterEvent((EventGroupMemberDecrease ev) => EventGroupMemberDecrease?.Invoke(momoTalk, ev));
        RegisterEvent((EventGroupHonorChange ev) => EventGroupHonorChange?.Invoke(momoTalk, ev));
        RegisterEvent((EventGroupMemberIncrease ev) => EventGroupMemberIncrease?.Invoke(momoTalk, ev));
        RegisterEvent((EventGroupLuckyKing ev) => EventGroupLuckyKing?.Invoke(momoTalk, ev));
        RegisterEvent((EventPokeGroup ev) => EventPokeGroup?.Invoke(momoTalk, ev));
        RegisterEvent((EventPokeFriend ev) => EventPokeFriend?.Invoke(momoTalk, ev));
        RegisterEvent((EventGroupRecall ev) => EventGroupRecall?.Invoke(momoTalk, ev));
        RegisterEvent((EventGroupUpload ev) => EventGroupUpload?.Invoke(momoTalk, ev));
        RegisterEvent((EventRequestFriend ev) => EventRequestFriend?.Invoke(momoTalk, ev));
        RegisterEvent((EventRequestGroup ev) => EventRequestGroup?.Invoke(momoTalk, ev));
    }

    public event ObAsyncEventHandler<EventMessagePrivate>? EventMessagePrivate;
    public event ObAsyncEventHandler<EventMessageGroup>? EventMessageGroup;
    public event ObAsyncEventHandler<EventConnect>? EventConnect;
    public event ObAsyncEventHandler<EventOneBotDisable>? EventOneBotDisable;
    public event ObAsyncEventHandler<EventOneBotEnable>? EventOneBotEnable;
    public event ObAsyncEventHandler<EventHeartbeat>? EventHeartbeat;
    public event ObAsyncEventHandler<EventFriendAdd>? EventFriendAdd;
    public event ObAsyncEventHandler<EventFriendRecall>? EventFriendRecall;
    public event ObAsyncEventHandler<EventGroupAdmin>? EventGroupAdmin;
    public event ObAsyncEventHandler<EventGroupBan>? EventGroupBan;
    public event ObAsyncEventHandler<EventGroupMemberDecrease>? EventGroupMemberDecrease;
    public event ObAsyncEventHandler<EventGroupHonorChange>? EventGroupHonorChange;
    public event ObAsyncEventHandler<EventGroupMemberIncrease>? EventGroupMemberIncrease;
    public event ObAsyncEventHandler<EventGroupLuckyKing>? EventGroupLuckyKing;
    public event ObAsyncEventHandler<EventPokeGroup>? EventPokeGroup;
    public event ObAsyncEventHandler<EventPokeFriend>? EventPokeFriend;
    public event ObAsyncEventHandler<EventGroupRecall>? EventGroupRecall;
    public event ObAsyncEventHandler<EventGroupUpload>? EventGroupUpload;
    public event ObAsyncEventHandler<EventRequestFriend>? EventRequestFriend;
    public event ObAsyncEventHandler<EventRequestGroup>? EventRequestGroup;

    private void RegisterEvent<TEvent>(Action<TEvent> invoker) where TEvent : EventBase {
        _eventInvokers[typeof(TEvent)] = ev => invoker.Invoke((TEvent)ev);
    }

    internal void PostEvent(EventBase ev) {
        Task.Run(() => {
            try {
                if (_eventInvokers.TryGetValue(ev.GetType(), out Action<EventBase>? invoker)) {
                    invoker.Invoke(ev);
                } else {
                    if (_logger.IsEnabled(LogLevel.Warning)) {
                        _logger.LogWarning("event {} is not registered but pushed to invoker", ev.GetType().Name);
                    }
                }
            } catch (Exception e) {
                if (_logger.IsEnabled(LogLevel.Error)) {
                    _logger.LogError("error occured while handling event\n{}", e);
                }
            }
        });
    }
}
