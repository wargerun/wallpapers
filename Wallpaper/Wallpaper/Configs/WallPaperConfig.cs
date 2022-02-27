using Microsoft.Extensions.Options;

namespace Wallpaper.Configs;

public class WallPaperConfig : IValidateOptions<WallPaperConfig>
{
    public const string SectionName = nameof(WallPaperConfig);

    public int ImageMax { get; set; }

    public string YandexUrl { get; set; } = "https://yandex.ru/images/search?text=wallpapers&p={0}";

    public string WallpaperFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures);

    public string WallpaperFileHistory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures), "image_list.json");

    public ValidateOptionsResult Validate(string name, WallPaperConfig options)
    {
        if (options.ImageMax < 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.ImageMax)}: less than 0.");
        }

        if (string.IsNullOrWhiteSpace(options.WallpaperFolder))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.WallpaperFolder)}: is null or white space.");
        }

        if (string.IsNullOrWhiteSpace(options.WallpaperFileHistory))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.WallpaperFileHistory)}: is null or white space.");
        }

        if (string.IsNullOrWhiteSpace(options.YandexUrl))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.YandexUrl)}: is null or white space.");
        }

        return ValidateOptionsResult.Success;
    }
}