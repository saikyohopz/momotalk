namespace Arkko.MomoTalk.OneBot.Protocol.Messages;

public class ObAtAll : ObAt {
    private readonly static Lazy<ObAtAll> SomeAtAll = new(() => new ObAtAll());
    
    public static ObAtAll Instance => SomeAtAll.Value;
    
    public ObAtAll() : base(-1) { }
}
