using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.Serialization;

namespace Delinea.Extensions.Provider.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
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

    public DelineaServiceException(
        string message,
        IDictionary<string, string[]> errors,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : base(message)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
        StatusCode = statusCode;
    }

    protected DelineaServiceException(
        SerializationInfo info,
        StreamingContext context)
        : base(info, context)
    {
    }

    public IDictionary<string, string[]> Errors { get; private set; }

    public HttpStatusCode StatusCode { get; }
}