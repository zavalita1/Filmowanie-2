using System.Threading.Tasks;
using Filmowanie;
using Filmowanie.Constants;
using Filmowanie.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Environment = Filmowanie.Enums.Environment;

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


builder.Services.AddAuthorization(o =>
    o.AddPolicy(SchemesNamesConsts.Admin, policy => policy.AddRequirements(new AdminAccessRequirement())));

var dataProtectionBuilder = builder.Services.AddDataProtection().SetApplicationName("filmowanie");

if (environment != Environment.Development)
{
    // TODO dataProtectionBuilder.PersistKeysToAzureBlobStorage()
}

RegisterServices.RegisterCustomServices(builder.Services, builder.Configuration, environment);

// TODO keyvault integration

// TODO service bus integration
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

app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";

    if (environment != Environment.Production)
    {
        spa.UseReactDevelopmentServer(npmScript: "start");
    }
});

// TODO configure signalr hubs

UsersCache.HydrateCache();

app.Run();
