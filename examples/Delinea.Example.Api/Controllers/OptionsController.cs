using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Delinea.Example.Api.Settings;
using System.Collections.Generic;

namespace Delinea.Example.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class OptionsController : ControllerBase
{
    private readonly IOptions<AzureAdB2E> _settings;


    public OptionsController(IOptions<AzureAdB2E> settings)
    {
        _settings = settings;
    }

    [HttpGet]
    public ActionResult<IDictionary<string, string>> Get()
    {
        string? azureAdB2EClientId = _settings.Value.ClientId;
        string? azureAdB2EClientSecret = _settings.Value.ClientSecret;

        if (string.IsNullOrEmpty(azureAdB2EClientId) || string.IsNullOrEmpty(azureAdB2EClientSecret))
        {
            return BadRequest("Azure AD B2E settings not found.");
        }

        return new Dictionary<string, string>
        {
            ["AzureAdB2E:ClientId"] = azureAdB2EClientId,
            ["AzureAdB2E:ClientSecret"] = azureAdB2EClientSecret
        };
    }
}
