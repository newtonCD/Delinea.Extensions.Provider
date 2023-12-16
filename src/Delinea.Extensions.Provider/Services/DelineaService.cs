using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Delinea.Extensions.Provider.Constants;
using Delinea.Extensions.Provider.Exceptions;
using Delinea.Extensions.Provider.Interfaces;
using Delinea.Extensions.Provider.Models;
using Microsoft.Extensions.Logging;
using Polly;
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
                    throw new DelineaServiceException("Problemas na obtenção do token. 'response.Data' retornou nulo.");
                }
            }

            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(response.ErrorException, "Falha ao obter o token de acesso ao cofre de segredos: {ErrorMessage}", response.ErrorException?.Message);
            }

            throw new DelineaServiceException(response.ErrorMessage, response.ErrorException, response.StatusCode);
        }).ConfigureAwait(false);
    }

    public Task<SecretListPathsResponse> GetSecretListPathsAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException($"O parâmetro '{nameof(token)}' não pode estar em branco ou nulo.", nameof(token));
        }

        return GetSecretListPathsInternalAsync(token);
    }

    public Task<SecretResponse> GetSecretAsync(string token, string secretPath)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException($"O parâmetro '{nameof(token)}' não pode estar em branco ou nulo.", nameof(token));
        }

        if (string.IsNullOrWhiteSpace(secretPath))
        {
            throw new ArgumentException($"O parâmetro '{nameof(secretPath)}' não pode estar em branco ou nulo.", nameof(secretPath));
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
                _logger.LogError(response.ErrorException, "Falha ao obter a lista de segredos do cofre: {ErrorMessage}", response.ErrorException?.Message);
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
                _logger.LogError(response.ErrorException, "Falha ao obter o segredo '{SecretPath}' do cofre: {ErrorMessage}", GetLastPartOfSecretPath(secretPath), response.ErrorException?.Message);
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

    private Polly.Retry.AsyncRetryPolicy CreateRetryPolicy()
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
                        _logger.LogWarning(exception, "Tentativa {RetryCount} de conectar no cofre de segredos falhou. Atrasando por {TotalSeconds} segundos antes de tentar novamente.", retryCount, timeSpan.TotalSeconds);
                    }
                });
    }
}