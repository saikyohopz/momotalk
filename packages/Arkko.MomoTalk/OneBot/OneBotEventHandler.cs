using Arkko.MomoTalk.OneBot.Events;

namespace Arkko.MomoTalk.OneBot;

public delegate void OneBotEventHandler<in TEvent>(IMomoTalk momoTalk, TEvent ev) where TEvent : Event;
