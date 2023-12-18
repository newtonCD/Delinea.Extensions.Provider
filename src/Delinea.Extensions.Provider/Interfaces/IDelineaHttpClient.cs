using System.Threading.Tasks;
using Delinea.Extensions.Provider.Models;
using RestSharp;

namespace Delinea.Extensions.Provider.Interfaces;

public interface IDelineaHttpClient
{
    Task<RestResponse<TokenResponse>> ExecuteTokenRequestAsync(RestRequest request);

    Task<RestResponse<SecretListPathsResponse>> ExecuteSecretListPathsRequestAsync(RestRequest request);

    Task<RestResponse<SecretResponse>> ExecuteSecretRequestAsync(RestRequest request);
}