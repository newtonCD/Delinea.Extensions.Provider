#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Delinea.Extensions.Provider.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Delinea.Extensions.Provider;

public class DelineaConfigurationProvider : ConfigurationProvider, IDelineaConfigurationProvider, IAsyncDisposable
{
    private readonly IDelineaService _delineaService;
    private readonly CancellationTokenSource _cts = new();
    private readonly TimeSpan _pollingInterval;
    private readonly ILogger<DelineaConfigurationProvider> _logger;
    private readonly bool _enablePolling;
    private Task? _reloadTask;
    private bool _disposedValue;

    public DelineaConfigurationProvider(
        IDelineaService delineService,
        DelineaSecretVaultSettings settings,
        ILogger<DelineaConfigurationProvider> logger,
        bool enablePolling = true)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        _delineaService = delineService;
        _pollingInterval = TimeSpan.FromSeconds(settings.PollingIntervalSeconds);
        _logger = logger;
        _enablePolling = enablePolling;
    }

    public override void Load()
    {
        LoadAsync().GetAwaiter().GetResult();
        StartPollingForChanges();
    }

    [ExcludeFromCodeCoverage]

    public async ValueTask DisposeAsync()
    {
        if (_disposedValue)
        {
            return;
        }

        _disposedValue = true;

#if NET8_0_OR_GREATER
        await _cts.CancelAsync().ConfigureAwait(false);
#else
        _cts.Cancel();
#endif
        if (_reloadTask?.IsCompleted == false)
        {
            try
            {
                await _reloadTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while waiting for the polling task to complete.");
                throw;
            }
        }

        _cts.Dispose();
        GC.SuppressFinalize(this);
    }

    internal void Load(IDictionary secretVariables)
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        var e = secretVariables.GetEnumerator();
        try
        {
            while (e.MoveNext())
            {
                data[Normalize((string)e.Entry.Key)] = (string?)e.Entry.Value;
            }
        }
        finally
        {
            (e as IDisposable)?.Dispose();
        }

        Data = data;
    }

    internal async Task LoadAsync()
    {
        var secretVariables = await GetSecretsVariablesAsync().ConfigureAwait(false);
        Load(secretVariables);
        OnReload();
    }

    internal async Task<IDictionary> GetSecretsVariablesAsync()
    {
        var token = await _delineaService.GetTokenAsync().ConfigureAwait(false);

        if (!token.Success)
        {
            throw new InvalidOperationException("Some error occurred while obtaining the token.");
        }

        var secretsList =
            await _delineaService.GetSecretListPathsAsync(token.AccessToken).ConfigureAwait(false) ??
                  throw new InvalidOperationException("Some error occurred while loading the list of secrets.");

        var secrets = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (string? secretPath in secretsList.Data)
        {
            var secretResponse =
                await _delineaService.GetSecretAsync(token.AccessToken, secretPath).ConfigureAwait(false);

            if (secretResponse?.Success == true && secretResponse.Data is Dictionary<string, string> secretData)
            {
                foreach (var kvp in secretData)
                {
                    secrets[kvp.Key] = kvp.Value;
                }
            }
        }

        return secrets;
    }

    [ExcludeFromCodeCoverage]
    private static string Normalize(string key) => key.Replace("__", ConfigurationPath.KeyDelimiter);

    [ExcludeFromCodeCoverage]
    private void StartPollingForChanges()
    {
        if (_enablePolling && (_reloadTask?.IsCompleted != false))
        {
            _reloadTask = Task.Delay(_pollingInterval)
                              .ContinueWith(
                                  async _ => await PollForChangesAsync(_cts.Token).ConfigureAwait(false),
                                  CancellationToken.None,
                                  TaskContinuationOptions.None,
                                  TaskScheduler.Current);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task PollForChangesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await LoadAsync().ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                break;
            }
#pragma warning disable CA1031 // Catch a more specific allowed exception type
            catch (Exception ex)
            {
                _logger.LogError(ex, "An internal error occurred while consuming the Delinea API.");
            }
#pragma warning restore CA1031 // Catch a more specific allowed exception type

            await Task.Delay(_pollingInterval, cancellationToken).ConfigureAwait(false);
        }
    }
}