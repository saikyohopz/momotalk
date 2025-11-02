namespace Arkko.MomoTalk.OneBot.Protocol.Models;

public class ObGroupMemberInfo {
    public long GroupId { get; set; }

    public long UserId { get; set; }

    public string Nickname { get; set; }

    public string Card { get; set; }

    public string Sex { get; set; }

    public int Age { get; set; }

    public string Area { get; set; }
    
    public int JoinTime { get; set; }
    
    public int LastSentTime { get; set; }
    
    public string Level { get; set; }
    
    public string Role { get; set; }
    
    public bool Unfriendly { get; set; }
    
    public string Title { get; set; }
    
    public int TitleExpireTime { get; set; }
    
    public bool CardChangeable { get; set; }
}
