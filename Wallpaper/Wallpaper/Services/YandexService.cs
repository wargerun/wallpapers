using System.Runtime.CompilerServices;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wallpaper.Configs;

namespace Wallpaper.Services;

public class YandexService
{
    private static readonly string[] WordsForRemove =
    {
        "img_url=",
        "&amp"
    };

    private readonly ILogger<YandexService> _logger;
    private readonly IOptions<WallPaperConfig> _options;

    private readonly HttpClient _client;

    public YandexService(
        ILogger<YandexService> logger,
        IOptions<WallPaperConfig> options,
        HttpClient client)
    {
        _logger = logger;
        _options = options;
        _client = client;
    }

    public async IAsyncEnumerable<string[]> GetImages(
        [EnumeratorCancellation] CancellationToken token)
    {
        var page = 0;

        do
        {
            var requestUri = string.Format(_options.Value.YandexUrl, page++);

            using var httpResponseMessage = await _client.GetAsync(requestUri, token);
            var htmlContentString = await httpResponseMessage.Content.ReadAsStringAsync(token);

            var document = new HtmlDocument();
            document.LoadHtml(htmlContentString);

            var imagesHref = document.DocumentNode
                .SelectNodes("//div/a[@class='serp-item__link']")
                .Select(n => n.GetAttributeValue("href", null))
                .Where(a => a is not null)
                .Select(href => href.Split(";", StringSplitOptions.RemoveEmptyEntries))
                .Where(attribute => attribute.Length >= 2)
                .ToList();

            _logger.LogDebug("Get images list from yandex: {Url} (Count: {Count})", requestUri, imagesHref.Count);

            if (imagesHref.Count == 0)
            {
                yield break;
            }

            var images = imagesHref.Select(imagesAttributes => ExtractImageUrl(imagesAttributes[2]))
                .Select(DecodeUrlString)
                .ToArray();

            yield return images;
        } while (!token.IsCancellationRequested);
    }

    private static string ExtractImageUrl(string imagesAttribute)
    {
        string result = imagesAttribute;

        foreach (var word in WordsForRemove)
        {
            result = result.Replace(word, string.Empty);
        }

        return result;
    }

    private static string DecodeUrlString(string url) {
        string newUrl;
        while ((newUrl = Uri.UnescapeDataString(url)) != url)
            url = newUrl;
        return newUrl;
    }
}