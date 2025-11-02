namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public interface IEventQuickOptAware {
    Func<object, Task>? QuickOptInvoker { get; set; }
}
