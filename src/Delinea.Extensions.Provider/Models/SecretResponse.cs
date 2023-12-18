using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Delinea.Extensions.Provider.Models;

[ExcludeFromCodeCoverage]
public sealed class SecretResponse
{
    public SecretResponse()
    {
    }

    public SecretResponse(bool success)
    {
        Success = success;
    }

    public SecretResponse(bool success, string message)
        : this(success)
    {
        Message = message;
    }

    public bool Success { get; set; }

    public string Message { get; set; }

    public string Id { get; set; }

    public string Path { get; set; }

    public object Attributes { get; set; }

    public string Description { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
    public Dictionary<string, string> Data { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

    public DateTime Created { get; set; }

    public DateTime LastModified { get; set; }

    public string CreatedBy { get; set; }

    public string LastModifiedBy { get; set; }

    public string Version { get; set; }
}