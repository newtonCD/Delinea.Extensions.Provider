using Delinea.Extensions.Provider.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Delinea.Extensions.Provider;

public class DelineaConfigurationSource : IConfigurationSource
{
    private readonly IServiceCollection _services;

    public DelineaConfigurationSource(IServiceCollection services)
    {
        _services = services;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var serviceProvider = _services.BuildServiceProvider();
        var settings = serviceProvider.GetRequiredService<IOptions<DelineaSecretVaultSettings>>().Value;
        var logger = serviceProvider.GetRequiredService<ILogger<DelineaConfigurationProvider>>();
        var delineService = serviceProvider.GetRequiredService<IDelineaService>();

        return new DelineaConfigurationProvider(delineService, settings, logger);
    }
}