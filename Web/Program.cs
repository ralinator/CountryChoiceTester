using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Web;
using Web.Data;
using Web.Services;

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
