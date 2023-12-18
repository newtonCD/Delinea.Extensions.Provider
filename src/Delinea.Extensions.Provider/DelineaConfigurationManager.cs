using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Delinea.Extensions.Provider;

[ExcludeFromCodeCoverage]
public class DelineaConfigurationManager : IHostedService
{
    private readonly DelineaConfigurationProvider _provider;

    public DelineaConfigurationManager(DelineaConfigurationProvider provider)
    {
        _provider = provider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _provider.DisposeAsync().ConfigureAwait(false);
    }
}