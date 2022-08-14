namespace Web;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using System.Diagnostics.CodeAnalysis;
using Web;
using Web.Data;
using Web.Services;

public static class Program
{
    /// <summary>
    /// FIXME: This is required for EF Core 6.0 as it is not compatible with trimming.
    ///
    /// For more information:
    ///   [.NET 6] Migrate API - Could not find method 'AddYears' on type 'System.DateOnly'
    ///   https://github.com/dotnet/efcore/issues/26860
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private static Type _keepDateOnly = typeof(DateOnly);
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddMudServices();

        builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services.AddSingleton<IGameDataService, GameDataService>();

        // Sets up EF Core with Sqlite
        builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
          options
            .UseSqlite($"Filename={DbConstants.SqliteDbFilename}")
            .EnableSensitiveDataLogging());

        await builder.Build().RunAsync();

    }
}

