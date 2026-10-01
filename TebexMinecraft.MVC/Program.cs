using Microsoft.EntityFrameworkCore;
using TebexMinecraft.API.Data;
using TebexMinecraft.Consumer;
using TebexMinecraft.Modelos;
using TebexMinecraft.MVC.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TebexMinecraftAPIContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");
app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<AdminAuthMiddleware>();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Ranks}/{action=Index}/{id?}");

app.Run();