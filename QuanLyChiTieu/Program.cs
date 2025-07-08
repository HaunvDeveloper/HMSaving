
/*
build EntityFramework


VS Code
dotnet ef dbcontext scaffold "Data Source=168.231.122.98;Initial Catalog=OnlineCodingWeb;Persist Security Info=True;User ID=sa;Password=NguyenH@u100304;Trust Server Certificate=True" Microsoft.EntityFrameworkCore.SqlServer -o Models --force

VS 2022
Scaffold-DbContext "Data Source=168.231.122.98;Initial Catalog=QLChiTieu;User ID=sa;Password=NguyenH@u100304;Trust Server Certificate=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Force



*/


using Microsoft.AspNetCore.Authentication.Cookies;
using QuanLyChiTieu.Models;
using System.Security.Policy;
using Microsoft.AspNetCore.Http.Features;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddDbContext<QlchiTieuContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("QuanLyChiTieu"));
});
//builder.Services.AddTransient<RunProcessService>(); // Or Scoped if request-specific coordination needed
//builder.Services.AddTransient<EmailSendService>(); // Or Singleton if thread-safe and stateless
//builder.Services.AddScoped<MarkingService>();
//builder.Services.AddScoped<UserPointService>();
//builder.Services.AddScoped<UserListService>();
//builder.Services.AddScoped<RankingService>();
//builder.Services.AddSingleton<OnlineUsersService>();
//builder.Services.AddHostedService<UserCleanupService>();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(3);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "HMS.Auth";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;

});
builder.Services.AddMemoryCache();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/";
        options.ExpireTimeSpan = TimeSpan.FromDays(3);
        options.SlidingExpiration = true;
    });



// Config Lowercase
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddSignalR();

builder.WebHost.UseUrls("http://localhost:5001");



var app = builder.Build();



if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();


//app.MapHub<MarkingHub>("/markingHub");
//app.MapHub<OnlineUsersHub>("/onlineUsersHub");
//app.MapHub<MeetingHub>("/meetingHub");
//app.UseMiddleware<UpdateLastActiveMiddleware>();

//app.MapControllerRoute(
//    name: "areas",
//    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Jar}/{action=Index}/{id?}");

app.Run();


