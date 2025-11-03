using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arkko.MomoTalk;

public class MomoTalkConfig {
    public string Token = string.Empty;

    public int RetryTimes = 5;

    public TimeSpan RetryWait = TimeSpan.FromSeconds(5);

    public TimeSpan RetryRest = TimeSpan.FromSeconds(30);

    public TimeSpan WebSocketHeartbeat = TimeSpan.FromSeconds(60);
    
    public string WebSocketUrl { get; set; }

    public MomoTalkConfig(string webSocketUrl) {
        WebSocketUrl = webSocketUrl;
    }
}
