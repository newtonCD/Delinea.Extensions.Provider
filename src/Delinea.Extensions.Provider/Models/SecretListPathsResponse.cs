using System.Diagnostics.CodeAnalysis;

namespace Delinea.Extensions.Provider.Models;

[ExcludeFromCodeCoverage]
public sealed class SecretListPathsResponse
{
    public SecretListPathsResponse()
    {
    }

    public SecretListPathsResponse(bool success)
    {
        Success = success;
    }

    public SecretListPathsResponse(bool success, string message)
        : this(success)
    {
        Message = message;
    }

    public bool Success { get; set; }

    public string Message { get; set; }

    public string[] Data { get; set; }
}