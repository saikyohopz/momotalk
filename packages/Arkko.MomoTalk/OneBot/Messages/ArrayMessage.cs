namespace Arkko.MomoTalk.OneBot.Messages;

public class ArrayMessage {
    public string Type { get; set; }
    public object Data { get; set; }

    public ArrayMessage(string type, object data) {
        Type = type;
        Data = data;
    }
}
