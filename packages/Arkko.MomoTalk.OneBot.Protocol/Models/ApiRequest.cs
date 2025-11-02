using Arkko.MomoTalk.Foundation.Utils;

namespace Arkko.MomoTalk.OneBot.Protocol.Models;

public class ApiRequest {
    public string Action { get; set; }
    public object? Params { get; set; }
    public string Echo { get; set; }

    public ApiRequest(string action, object? @params) {
        Action = action;
        Params = @params;
        Echo = IdWorker.Instance.NextId().ToString();
    }
}
