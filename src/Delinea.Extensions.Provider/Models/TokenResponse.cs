using System.Diagnostics.CodeAnalysis;

namespace Delinea.Extensions.Provider.Models;

[ExcludeFromCodeCoverage]
public sealed class TokenResponse
{
    public TokenResponse()
    {
    }

    public TokenResponse(bool success)
    {
        Success = success;
    }

    public TokenResponse(bool success, string message)
        : this(success)
    {
        Message = message;
    }

    public bool Success { get; set; }

    public string Message { get; set; }

    public string AccessToken { get; set; }

    public string TokenType { get; set; }

    public int ExpiresIn { get; set; }
}