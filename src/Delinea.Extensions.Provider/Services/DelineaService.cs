using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Delinea.Extensions.Provider.Constants;
using Delinea.Extensions.Provider.Exceptions;
using Delinea.Extensions.Provider.Interfaces;
using Delinea.Extensions.Provider.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RestSharp;

namespace Delinea.Extensions.Provider.Services;

public class DelineaService : IDelineaService
{
    private readonly DelineaSecretVaultSettings _settings;
    private readonly IDelineaHttpClient _httpClient;
    private readonly ILogger<DelineaService> _logger;

    public DelineaService(
        DelineaSecretVaultSettings settings,
        IDelineaHttpClient httpClient,
        ILogger<DelineaService> logger)
    {
        _settings = settings;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<TokenResponse> GetTokenAsync()
    {
        var retryPolicy = CreateRetryPolicy();

        return await retryPolicy.ExecuteAsync(async () =>
        {
            var request = new RestRequest(_settings.TokenResource, Method.Post);

            request.AddHeader(DelineaConstants.ContentType, _settings.TokenContentType);
            request.AddParameter(DelineaConstants.GrantType, _settings.GrantType);
            request.AddParameter(DelineaConstants.ClientId, _settings.ClientId);
            request.AddParameter(DelineaConstants.ClientSecret, _settings.ClientSecret);

            var response = await _httpClient.ExecuteTokenRequestAsync(request).ConfigureAwait(false);

            if (response.IsSuccessful)
            {
                if (response.Data != null)
                {
                    var responseBody = response.Data;
                    responseBody.Success = true;

                    return responseBody;
                }
                else
                {
                    throw new DelineaServiceException("There was a problem obtaining the token. 'response.Data' returned null.");
                }
            }

            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(response.ErrorException, "Failed to obtain secret vault access token: {ErrorMessage}", response.ErrorException?.Message);
            }

            throw new DelineaServiceException(response.ErrorMessage, response.ErrorException, response.StatusCode);
        }).ConfigureAwait(false);
    }

    public Task<SecretListPathsResponse> GetSecretListPathsAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException($"The parameter '{nameof(token)}' cannot be null or empty.", nameof(token));
        }

        return GetSecretListPathsInternalAsync(token);
    }

    public Task<SecretResponse> GetSecretAsync(string token, string secretPath)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException($"The parameter '{nameof(token)}' cannot be null or empty.", nameof(token));
        }

        if (string.IsNullOrWhiteSpace(secretPath))
        {
            throw new ArgumentException($"The parameter '{nameof(secretPath)}' cannot be null or empty.", nameof(secretPath));
        }

        return GetSecretInternalAsync(token, secretPath);
    }

    protected virtual async Task<SecretListPathsResponse> GetSecretListPathsInternalAsync(string token)
    {
        var retryPolicy = CreateRetryPolicy();

        return await retryPolicy.ExecuteAsync(async () =>
        {
            string resource = $"{_settings.SecretsResource}/{_settings.SecretsBasePath}{_settings.ListPaths}";
            var request = new RestRequest(resource, Method.Get);
            request.AddOrUpdateHeader(DelineaConstants.Authorization, DelineaConstants.Bearer + token);

            var response = await _httpClient.ExecuteSecretListPathsRequestAsync(request).ConfigureAwait(false);

            if (response.IsSuccessful && response.Data.Data.Length > 0)
            {
                var responseBody = response.Data;
                responseBody.Success = true;

                return responseBody;
            }

            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(response.ErrorException, "Failed to get list of vault secrets: {ErrorMessage}", response.ErrorException?.Message);
            }

            throw new DelineaServiceException(response.ErrorMessage, response.ErrorException, response.StatusCode);
        }).ConfigureAwait(false);
    }

    protected virtual async Task<SecretResponse> GetSecretInternalAsync(string token, string secretPath)
    {
        var retryPolicy = CreateRetryPolicy();

        return await retryPolicy.ExecuteAsync(async () =>
        {
            string resource = $"{_settings.SecretsResource.TrimEnd('/')}/{{{nameof(secretPath)}}}";
            var request = new RestRequest(resource, Method.Get);
            request.AddOrUpdateHeader(DelineaConstants.Authorization, DelineaConstants.Bearer + token);
            request.AddUrlSegment(nameof(secretPath), secretPath);

            var response = await _httpClient.ExecuteSecretRequestAsync(request).ConfigureAwait(false);

            if (response.IsSuccessful)
            {
                var responseBody = response.Data;
                responseBody.Success = true;

                return responseBody;
            }

            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(response.ErrorException, "Failed to obtain secret '{SecretPath}' from vault: {ErrorMessage}", GetLastPartOfSecretPath(secretPath), response.ErrorException?.Message);
            }

            throw new DelineaServiceException(response.ErrorMessage, response.ErrorException, response.StatusCode);
        }).ConfigureAwait(false);
    }

    [ExcludeFromCodeCoverage]
    private static string GetLastPartOfSecretPath(string input)
    {
        int lastColonIndex = input.LastIndexOf(':');

        if (lastColonIndex == -1 || lastColonIndex == input.Length - 1)
        {
            return string.Empty;
        }

        return input[(lastColonIndex + 1)..];
    }

    private AsyncRetryPolicy CreateRetryPolicy()
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: _settings.RetryCount,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(_settings.RetryBase, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, _) =>
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                    {
                        _logger.LogWarning(exception, "The {RetryCount} attempt to connect to the secret vault failed. Delaying {TotalSeconds} seconds before trying again.", retryCount, timeSpan.TotalSeconds);
                    }
                });
    }
}