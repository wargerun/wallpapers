namespace Wallpaper.Helpers;

public static class HttpHelper
{
    public static async Task<byte[]> DownloadFile(string url, CancellationToken token)
    {
        var httpClient = new HttpClient();
        var bytes = await httpClient.GetByteArrayAsync(url, token);
        return bytes;
    }
}