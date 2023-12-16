using System;
using System.Threading.Tasks;
using Delinea.Extensions.Provider.Interfaces;
using Delinea.Extensions.Provider.Models;
using RestSharp;

namespace Delinea.Extensions.Provider.Services;

public sealed class DelineaHttpClient : IDelineaHttpClient, IDisposable
{
    private readonly RestClient _client;
    private bool disposedValue;

    public DelineaHttpClient(
        string baseUrl,
        RestClientOptions clientOptions = null)
    {
        _client = new RestClient(clientOptions ?? new RestClientOptions(baseUrl));
    }

    public DelineaHttpClient(
        System.Uri baseUrl,
        RestClientOptions clientOptions = null)
    {
        _client = new RestClient(clientOptions ?? new RestClientOptions(baseUrl));
    }

    public async Task<RestResponse<TokenResponse>> ExecuteTokenRequestAsync(RestRequest request)
        => await _client.ExecuteAsync<TokenResponse>(request).ConfigureAwait(false);

    public async Task<RestResponse<SecretListPathsResponse>> ExecuteSecretListPathsRequestAsync(RestRequest request)
        => await _client.ExecuteAsync<SecretListPathsResponse>(request).ConfigureAwait(false);

    public async Task<RestResponse<SecretResponse>> ExecuteSecretRequestAsync(RestRequest request)
        => await _client.ExecuteAsync<SecretResponse>(request).ConfigureAwait(false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _client.Dispose();
            }

            disposedValue = true;
        }
    }
}