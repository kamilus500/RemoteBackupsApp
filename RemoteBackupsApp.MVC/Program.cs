using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using RemoteBackupsApp.Infrastructure.Extensions;
using RemoteBackupsApp.Infrastructure.Hubs;
using RemoteBackupsApp.Infrastructure.Initializers;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddNotyf(config => { config.DurationInSeconds = 3; config.IsDismissable = true; config.Position = NotyfPosition.TopRight; });

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("pl-PL"),
        new CultureInfo("en-US")
    };

    options.DefaultRequestCulture = new RequestCulture("pl-PL");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddLocalization(o => { o.ResourcesPath = "Resources"; });

builder.Services.AddControllersWithViews()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();

builder.Services.AddInfrastructure(configuration);
builder.Services.AddFormOptions();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddMemoryCache();

var serviceProvider = builder.Services
    .BuildServiceProvider();

var databaseContext = serviceProvider.GetRequiredService<DatabaseContext>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseDefaultFiles();
app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.UseRequestLocalization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Backup}/{action=Index}");

app.UseNotyf();

var seeder = new Seeder(configuration);
if(seeder.IsDatatabaseExist() == 0)
{
    seeder.CreateDatabase();
}

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<NotyfyCompleteTaskHub>("/notyfyComplete");
});

app.Run();
