using System.Diagnostics.CodeAnalysis;

namespace Delinea.Extensions.Provider.Constants;

[ExcludeFromCodeCoverage]
public static class ValidationMessage
{
    public const string RequiredConfigurationItem = "This configuration item cannot be empty.";
    public const string RequiredField = "Required field.";
}