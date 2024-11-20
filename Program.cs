using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Filmowanie;
using Filmowanie.Account.Constants;
using Filmowanie.Controllers;
using Filmowanie.Database.Contexts;
using Filmowanie.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Environment = Filmowanie.Enums.Environment;
using IdentityDbContext = Filmowanie.Database.Contexts.IdentityDbContext;

var builder = WebApplication
    .CreateBuilder(args);

var environment = builder.Environment.IsDevelopment() ? Environment.Development : Environment.Production;

builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddSpaStaticFiles(so => so.RootPath = "ClientApp/build");
// TODO builder.Services.AddSignalR();

builder.Services.AddControllers(o =>
{
    o.Filters.Add<LoggingActionFilter>();
}).AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true);


builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultScheme = SchemesNamesConsts.Cookie;
    })
    .AddCookie(SchemesNamesConsts.Cookie, o =>
    {
        o.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });

if (environment == Environment.Production)
{
    var keyVaultName = builder.Configuration["KeyVaultName"];
    var kvUri = $"https://{keyVaultName}.vault.azure.net";

    var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
    var keyVaultConfigurationProvider = new ConcurrentDictionary<string, string>();
    var keys = client.GetPropertiesOfSecrets().AsPages().ToArray().SelectMany(x => x.Values);
    await Parallel.ForEachAsync(keys, async (k, cancel) =>
    {
        var secret = await client.GetSecretAsync(k.Name, cancellationToken: cancel);
        keyVaultConfigurationProvider.AddOrUpdate(k.Name, secret.Value.Value, (_, _) => throw new NotSupportedException("Secret names must be unique!"));
    });
    builder.Configuration.AddInMemoryCollection(keyVaultConfigurationProvider);
}


var dbConnectionString = builder.Configuration["dbConnectionString"]!;
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseCosmos(connectionString: dbConnectionString, databaseName: "db-filmowanie2"));

builder.Services.AddAuthorization(o =>
    o.AddPolicy(SchemesNamesConsts.Admin, policy => policy.AddRequirements(new AdminAccessRequirement())));

var dataProtectionBuilder = builder.Services.AddDataProtection().SetApplicationName("filmowanie2");

if (environment != Environment.Development)
    dataProtectionBuilder.PersistKeysToDbContext<IdentityDbContext>();


if (environment != Environment.Development)
{
}

RegisterServices.RegisterCustomServices(builder.Services, builder.Configuration, environment);
builder.Services.AddControllers();

// TODO database integration
var app = builder.Build();
if (environment != Environment.Production)
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSpaStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action=Index}/{id?}");
});


app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";

    if (environment != Environment.Production)
    {
        spa.UseReactDevelopmentServer(npmScript: "start");
    }
});


//// TODO configure signalr hubs
//app.MapGet("getUser", () => "user");

// UsersCache.HydrateCache(); TODO
 
app.Run();