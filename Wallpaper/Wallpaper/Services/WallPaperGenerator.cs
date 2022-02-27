using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wallpaper.Configs;
using Wallpaper.Helpers;
using Wallpaper.Models.FileControl;

namespace Wallpaper.Services;

internal class WallPaperGenerator
{
    private const string GeneratorFolderName = "Wallpapers";
    private readonly YandexService _service;
    private readonly ILogger<WallPaperGenerator> _logger;
    private readonly string _generatorFolder;
    private readonly WallPaperConfig _config;

    public WallPaperGenerator(
        YandexService service,
        IOptions<WallPaperConfig> options,
        ILogger<WallPaperGenerator> logger)
    {
        _service = service;
        _config = options.Value;
        _logger = logger;

        _generatorFolder = Path.Combine(_config.WallpaperFolder, GeneratorFolderName);

        logger.LogDebug("Path of wallpapers folder: {Folder}", _generatorFolder);
        logger.LogDebug("Path of wallpaper file history: {File}", _config.WallpaperFileHistory);

        if (!Directory.Exists(_generatorFolder))
        {
            Directory.CreateDirectory(_generatorFolder);
        }
    }

    public async Task Create(CancellationToken cancellationToken)
    {
        var imageMax = _config.ImageMax;

        if (imageMax <= 0)
        {
            _logger.LogWarning("ImageMax incorrect {imageMax}", imageMax);
            return;
        }

        var imageControl = new ImageFileControl(_config.WallpaperFileHistory);

        try
        {
            _logger.LogDebug("Load images: {count}", imageControl.CountImages);
            var saveImages = await SaveImages(imageControl, cancellationToken);
            _logger.LogInformation("Successfully saving images: {TotalCount}", saveImages);
        }
        finally
        {
            imageControl.Save();
            _logger.LogDebug("Update images: {count}", imageControl.CountImages);
        }
    }

    private async Task<int> SaveImages(
        ImageFileControl imageControl,
        CancellationToken cancellationToken)
    {
        var imageCounter = 0;
        var imageMax = _config.ImageMax;

        await foreach (var images in _service.GetImages(cancellationToken))
        {
            var listImages = images.ToList();
            var tasks = new List<Task<EntityImage>>(listImages.Count);

            while (listImages.Count > 0)
            {
                tasks.Clear();

                foreach (var imageUrl in images)
                {
                    listImages.Remove(imageUrl);
                    var imageName = GetImageName(imageUrl);

                    if (string.IsNullOrWhiteSpace(imageName) ||
                        !imageControl.IsImage(imageName))
                    {
                        _logger.LogWarning("File: {imageName} is not image (skipped)", imageName);
                        continue;
                    }

                    var imagePath = Path.Combine(_generatorFolder, imageName);

                    if (imageControl.TryCheck(i => i.ImageName == imageName))
                    {
                        _logger.LogWarning("File: {imageName} already saved (skipped)", imageName);
                        continue;
                    }

                    if (tasks.Count + imageCounter >= imageMax)
                    {
                        break;
                    }

                    tasks.Add(Task.Run(() => GetTask(
                            imageName: imageName,
                            imagePath: imagePath,
                            imageUrl: imageUrl,
                            cancellationToken: cancellationToken),
                        cancellationToken));
                }

                await Task.WhenAll(tasks);

                var count = tasks
                    .Select(t => t.Result)
                    .Select(imageControl.AddImage)          // Add image in collection
                    .Count(r => !r.IsError);      // Success images processed

                imageCounter += count;

                if (imageCounter >= imageMax)
                {
                    return imageCounter;
                }
            }
        }

        return 0;
    }

    private async Task<EntityImage> GetTask(
        string imageName,
        string imagePath,
        string imageUrl,
        CancellationToken cancellationToken)
    {
        var entityImage = new EntityImage
        {
            ImageName = imageName,
            ImagePath = imagePath,
            ImageUrl = imageUrl,
        };
        try
        {
            _logger.LogInformation("Downloading file: {imageUrl}", imageUrl);
            var imageBytes = await HttpHelper.DownloadFile(imageUrl, cancellationToken);
            entityImage.SizeBytes = imageBytes.Length;

            _logger.LogInformation("Saving {imagePath} ({Bytes})", imagePath, imageBytes.Length);
            await File.WriteAllBytesAsync(imagePath, imageBytes, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            switch (ex.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    entityImage.Error = $"StatusCode: {ex.StatusCode}";
                    _logger.LogWarning("File: {imageName} not found (skipped)", imageName);
                    break;
                default:
                    _logger.LogWarning(ex, "File: {Image}, status code: {StatusCode}", entityImage.ImageName, ex.StatusCode);
                    entityImage.Error = ex.GetBaseException().Message;
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error: {@imageName}", imageName);
            entityImage.Error = ex.GetBaseException().Message;
        }
        finally
        {
            entityImage.Timestamp = DateTime.Now;
        }

        return entityImage;
    }

    private static string? GetImageName(string imageUrl)
    {
        const int maxImageLength = 60; // Вместе с расширением
        var uri = new Uri(imageUrl);  // https://cdn.suwalls.com/wallpapers/nature/yellowstone-national-park-45253-2560x1600.jpg
        var fileName = uri.Segments[^1]; // Ignore query

        var indexOfDot = fileName.LastIndexOf(".", StringComparison.Ordinal);
        var indexOfDotInc = indexOfDot+1;

        if (indexOfDot == -1)
        {
            return null;
        }

        var fileNameWithoutExt = fileName[..indexOfDot];
        var extLength = fileName.Length - indexOfDotInc;
        var fileExt = fileName.Substring(indexOfDotInc, extLength);

        if (fileNameWithoutExt.Length >= maxImageLength)
        {
            fileNameWithoutExt = fileNameWithoutExt[..maxImageLength];
        }

        return $"{fileNameWithoutExt}.{fileExt}";
    }

    public void ClearFolder()
    {
        var counter = 0;

        foreach (var file in Directory.EnumerateFiles(_generatorFolder))
        {
            counter++;
            File.Delete(file);
        }

        _logger.LogInformation("Deleting files: {count}", counter);
    }
}