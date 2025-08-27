using NToastNotify;
using RemoteBackupsApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
  .AddNToastNotifyToastr(new ToastrOptions
  {
      ProgressBar = true,
      PositionClass = ToastPositions.TopCenter,
      CloseButton = true,
      TimeOut = 5000
  });

builder.Services.AddInfrastructure();

var app = builder.Build();

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
