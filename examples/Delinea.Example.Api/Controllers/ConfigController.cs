using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Delinea.Example.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ConfigController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public ActionResult<IDictionary<string, string>> Get()
    {
        string? azureAdB2EClientId = _configuration["AzureAdB2E:ClientId"];
        string? azureAdB2EClientSecret = _configuration["AzureAdB2E:ClientSecret"];

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
