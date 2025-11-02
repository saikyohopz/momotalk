namespace Arkko.MomoTalk.OneBot.Protocol.Models;

public class ApiResponse {
    public required string Status { get; set; }
    public int Retcode { get; set; }
    public object? Data { get; set; }
    public string? Echo { get; set; }
}
