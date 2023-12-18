using System.Threading.Tasks;
using Delinea.Extensions.Provider.Models;

namespace Delinea.Extensions.Provider.Interfaces;

public interface IDelineaService
{
    Task<TokenResponse> GetTokenAsync();

    Task<SecretListPathsResponse> GetSecretListPathsAsync(string token);

    Task<SecretResponse> GetSecretAsync(string token, string secretPath);
}