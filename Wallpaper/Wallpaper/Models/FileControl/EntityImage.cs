using Newtonsoft.Json;

namespace Wallpaper.Models.FileControl;

public class EntityImage
{
    [JsonProperty("name")]
    public string ImageName { get; set; } = null!;

    [JsonProperty("path")]
    public string ImagePath { get; set; } = null!;

    [JsonProperty("url")]
    public string ImageUrl { get; set; } = null!;

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("error")]
    public string? Error { get; set; }

    [JsonProperty("size_bytes")]
    public int SizeBytes { get; set; }

    [JsonIgnore]
    public bool IsError => Error is not null;
}