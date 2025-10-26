using System.Text.Json.Serialization;

namespace Arkko.MomoTalk.OneBot.Models;

public class ObLoginInfo {
    public long UserId { get; set; }
    
    public required string Nickname { get; set; }
}
