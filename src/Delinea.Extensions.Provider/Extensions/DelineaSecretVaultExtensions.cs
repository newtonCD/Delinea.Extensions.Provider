using System;
using System.Diagnostics.CodeAnalysis;
using Delinea.Extensions.Provider.Interfaces;
using Delinea.Extensions.Provider.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Delinea.Extensions.Provider.Extensions;

[ExcludeFromCodeCoverage]
public static class DelineaSecretVaultConfigurationExtensions
{
    public static IConfigurationBuilder AddDelineaSecretVault(
        this IConfigurationBuilder builder,
        IServiceCollection services)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var settingsSection = builder.Build().GetSection(DelineaSecretVaultSettings.ConfigurationSection);
        services.Configure<DelineaSecretVaultSettings>(settingsSection);

        services.AddSingleton(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<DelineaSecretVaultSettings>>();
            var settings = provider.GetRequiredService<IOptions<DelineaSecretVaultSettings>>().Value;
            settings.Validate(logger);
            return settings;
        });

        services.AddSingleton<IDelineaHttpClient>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<DelineaSecretVaultSettings>>().Value;
            return new DelineaHttpClient(settings.BaseAddress);
        });

        services.AddSingleton<IDelineaService, DelineaService>();
        services.AddSingleton<DelineaConfigurationProvider>();
        services.AddHostedService<DelineaConfigurationManager>();

        builder.Add(new DelineaConfigurationSource(services));

        return builder;
    }
}