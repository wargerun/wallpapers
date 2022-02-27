using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wallpaper.Configs;

namespace Wallpaper.Models.FileControl;

public class ImageFileControl
{
    private readonly string _pathOfImage;
    private readonly Encoding _encoding = Encoding.UTF8;
    private readonly string[] _imageExtensions = {"jpg", "png"};

    private List<EntityImage> Images { get; }
    public int CountImages => Images.Count;

    public ImageFileControl(
        string wallpaperFileHistory)
    {
        _pathOfImage = wallpaperFileHistory;

        Images ??= LoadImage();
    }

    public bool TryCheck(
        Func<EntityImage, bool> predicate)
    {
        var exist = Images.Any(predicate);
        return exist;
    }

    public EntityImage AddImage(EntityImage image)
    {
        Images.Add(image);
        return image;
    }

    public void Save()
    {
        var jsonSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
        };
        var entityImages = Images.OrderBy(i => i.Timestamp).ToList();
        var serializeObject = JsonConvert.SerializeObject(entityImages, jsonSerializerSettings);
        File.WriteAllText(_pathOfImage, serializeObject, _encoding);
    }

    public bool IsImage(string imageName)
    {
        return _imageExtensions.Any(imageName.EndsWith);
    }

    private List<EntityImage> LoadImage()
    {
        if (!File.Exists(_pathOfImage))
        {
            return new List<EntityImage>();
        }
        var readAllText = File.ReadAllText(_pathOfImage, _encoding);
        var images = JsonConvert.DeserializeObject<List<EntityImage>>(readAllText);

        return images ?? new List<EntityImage>();
    }
}