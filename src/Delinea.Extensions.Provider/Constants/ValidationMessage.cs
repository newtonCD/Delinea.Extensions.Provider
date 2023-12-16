using System.Diagnostics.CodeAnalysis;

namespace Delinea.Extensions.Provider.Constants;

[ExcludeFromCodeCoverage]
public static class ValidationMessage
{
    public const string RequiredConfigurationItem = "Este item de configuração não pode estar em branco.";
    public const string RequiredField = "O campo não pode estar em branco.";
}