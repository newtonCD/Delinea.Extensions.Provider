using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Delinea.Extensions.Provider.Exceptions;

[ExcludeFromCodeCoverage]
public class DelineaServiceException : Exception
{
    public DelineaServiceException()
    {
    }

    public DelineaServiceException(string message)
        : base(message)
    {
    }

    public DelineaServiceException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }

    public DelineaServiceException(
        string message,
        Exception innerException,
        HttpStatusCode statusCode)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public IDictionary<string, string[]> Errors { get; private set; }

    public HttpStatusCode StatusCode { get; }
}