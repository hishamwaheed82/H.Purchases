using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Volo.Abp;

namespace H.Purchases;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt", rollingInterval: RollingInterval.Day))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            Log.Information("Starting H.Purchases API host.");

            var builder = WebApplication.CreateBuilder(args);
            builder.Host
                .AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();

            await builder.AddApplicationAsync<PurchasesHttpApiHostModule>();

            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "H.Purchases API host terminated unexpectedly.");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
