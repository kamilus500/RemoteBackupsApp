using NToastNotify;
using RemoteBackupsApp.Infrastructure;
using RemoteBackupsApp.Infrastructure.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
  .AddNToastNotifyToastr(new ToastrOptions
  {
      ProgressBar = true,
      PositionClass = ToastPositions.TopCenter,
      CloseButton = true,
      TimeOut = 2000
  });

builder.Services.AddInfrastructure();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
});

var app = builder.Build();

app.UseMiddleware<CspNonceMiddleware>();

app.AddApplication();

app.UseNToastNotify();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Index}")
    .WithStaticAssets();

app.Run();
