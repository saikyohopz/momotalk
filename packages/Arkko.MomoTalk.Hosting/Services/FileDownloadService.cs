using Arkko.MomoTalk.Hosting.Attributes;

namespace Arkko.MomoTalk.Hosting.Services;

[Singleton]
public class FileDownloadService {
    private readonly HttpClient _httpClient = new();

    public byte[] DownloadFile(string url, string? fileName = null) {
        if (fileName == null) {
            return _httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
        }

        string tmp = Path.GetTempPath();
        string path = Path.Combine(tmp, fileName);

        try {
            return File.ReadAllBytes(path);
        } catch {
            byte[] bytes = _httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();

            File.WriteAllBytes(path, bytes);

            return bytes;
        }
    }
}
