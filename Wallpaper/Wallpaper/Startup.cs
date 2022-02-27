using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wallpaper.Services;

namespace Wallpaper;

public class Startup
{
    private readonly IServiceScopeFactory _factory;
    private readonly ILogger<Startup> _logger;

    public Startup(
        IServiceScopeFactory factory,
        ILogger<Startup> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        using var scope = _factory.CreateScope();

        var generator = scope.ServiceProvider.GetRequiredService<WallPaperGenerator>();

        _logger.LogInformation("Clear the wallpaper folder");
        generator.ClearFolder();

        _logger.LogInformation("Generate the wallpaper folder");
        await generator.Create(cancellationToken);
    }
}