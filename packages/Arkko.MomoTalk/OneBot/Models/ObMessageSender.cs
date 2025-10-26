namespace Arkko.MomoTalk.OneBot.Models;

public class ObMessageSender {
    /// <summary>
    /// QQ 号
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// 昵称
    /// </summary>
    public required string Nickname { get; set; }
    
    /// <summary>
    /// 群名片（群聊）或备注（私聊）
    /// </summary>
    public required string Card { get; set; }
    
    /// <summary>
    /// 性别，若对方未公开性别则返回null
    /// </summary>
    public string? Sex { get; set; }
    
    /// <summary>
    /// 年龄，若对方未公开年龄则返回null
    /// </summary>
    public int? Age { get; set; }
    
    /// <summary>
    /// 地区，若对方未公开地区则返回null
    /// </summary>
    public string? Area { get; set; }
    
    /// <summary>
    /// 成员等级，若不是群聊则返回null
    /// </summary>
    public string? Level { get; set; }
    
    /// <summary>
    /// 群聊角色，若不是群聊则返回null
    /// </summary>
    public string? Role { get; set; }
    
    /// <summary>
    /// 群聊专属头衔，若不是群聊则返回null
    /// </summary>
    public string? Title { get; set; }
}
