using Arkko.MomoTalk.OneBot.Protocol.Events;

namespace Arkko.MomoTalk.OneBot;

public delegate Task ObAsyncEventHandler<in TEvent>(MomoTalk momoTalk, TEvent ev) where TEvent : EventBase;
