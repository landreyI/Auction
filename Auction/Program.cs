using Auction.Data;
using Auction.Service;
using Auction.Service.DBServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.LoginPath = "/Home/NoAuthorize"; // Путь для перенаправления неавторизованных пользователей
        options.SlidingExpiration = true;
    });

builder.Services.AddControllersWithViews();

builder.Services.AddSignalR();

builder.Services.AddSingleton<ConnectionMapping>();
builder.Services.AddTransient<Notification>();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddSingleton<CloudinaryService>();

builder.Services.AddScoped<DBUserService>();
builder.Services.AddScoped<DBAuctionService>();
builder.Services.AddScoped<DBPurchaseService>();
builder.Services.AddScoped<DBChatService>();

builder.Services.AddScoped<DBService>();

builder.Services.AddHostedService<DBCheckService>();

builder.Services.AddSingleton<AutoBidStorage>();
builder.Services.AddHostedService<CheckAutoBidService>();

var connectionStringBD = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDBContext>(options => {
    options.LogTo(Console.WriteLine);
    options.UseSqlServer(connectionStringBD);
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Home}/{id?}");

app.MapHub<AuctionHub>("/chatHub");

app.Run();
