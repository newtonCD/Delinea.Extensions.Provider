using System;
using System.Reflection;
using Delinea.Example.Api.Settings;
using Delinea.Extensions.Provider.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Delinea.Example.Api;

public class Program
{
    protected Program()
    {
    }

    public static void Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(builder.Configuration)
                            .CreateLogger();

            builder.Host.UseSerilog();

            builder
                .Configuration
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();

            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            }

            builder.Configuration.AddDelineaSecretVault(builder.Services);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.UseInlineDefinitionsForEnums();
            });

            builder.Services.Configure<AzureAdB2E>(builder.Configuration.GetSection(nameof(AzureAdB2E)));

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            if (Log.IsEnabled(LogEventLevel.Fatal))
                Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            if (Log.IsEnabled(LogEventLevel.Information))
                Log.Information("Server shutting down...");

            Log.CloseAndFlush();
        }
    }
}
