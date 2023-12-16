#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Delinea.Extensions.Provider;

[ExcludeFromCodeCoverage]
public sealed class DelineaSecretVaultSettings
{
    public const string ConfigurationSection = nameof(DelineaSecretVaultSettings);

    public string? BaseAddress { get; set; }

    public string? GrantType { get; set; } = "client_credentials";

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string? TokenResource { get; set; } = "v1/token";

    public string? ListPaths { get; set; } = "::listpaths";

    public string? TokenContentType { get; set; } = "application/x-www-form-urlencoded";

    public string? SecretsBasePath { get; set; } = "apps:group:app01:sit";

    public string? SecretsResource { get; set; } = "v1/secrets";

    public int RetryCount { get; set; } = 3;

    public double RetryBase { get; set; } = 2.0;

    public int PollingIntervalSeconds { get; set; } = 300;

    public void Validate(ILogger logger)
    {
        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        var fields = new List<string>();

        if (string.IsNullOrWhiteSpace(BaseAddress))
        {
            fields.Add(nameof(BaseAddress));
        }
        else if (!Uri.TryCreate(BaseAddress, UriKind.Absolute, out _))
        {
            fields.Add($"{nameof(BaseAddress)} não é uma URL válida.");
        }

#pragma warning disable SA1503 // Braces should not be omitted
        if (string.IsNullOrWhiteSpace(ClientId)) fields.Add(nameof(ClientId));
        if (string.IsNullOrWhiteSpace(ClientSecret)) fields.Add(nameof(ClientSecret));
        if (string.IsNullOrWhiteSpace(GrantType)) fields.Add(nameof(GrantType));
        if (string.IsNullOrWhiteSpace(ListPaths)) fields.Add(nameof(ListPaths));
        if (string.IsNullOrWhiteSpace(SecretsBasePath)) fields.Add(nameof(SecretsBasePath));
        if (string.IsNullOrWhiteSpace(SecretsResource)) fields.Add(nameof(SecretsResource));
        if (string.IsNullOrWhiteSpace(TokenResource)) fields.Add(nameof(TokenResource));
        if (string.IsNullOrWhiteSpace(TokenContentType)) fields.Add(nameof(TokenContentType));
#pragma warning restore SA1503 // Braces should not be omitted

        if (fields.Any())
        {
            string errorMessage = $"[{ConfigurationSection}] - Os seguintes itens de configuração não foram configurados corretamente: {string.Join(", ", fields)}";
            if (logger.IsEnabled(LogLevel.Error))
            {
#pragma warning disable CA2254 // Template should be a static expression
                logger.LogError(errorMessage);
#pragma warning restore CA2254 // Template should be a static expression
            }

            throw new InvalidOperationException(errorMessage);
        }
    }
}