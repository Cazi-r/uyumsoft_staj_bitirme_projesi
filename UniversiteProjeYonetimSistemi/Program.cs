using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.IIS;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Services;
using UniversiteProjeYonetimSistemi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptions => 
        sqlServerOptions.EnableRetryOnFailure()));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Kimlik doğrulama servisi ve çerez kimlik doğrulama ekliyoruz
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

// HttpContext erişimi için
builder.Services.AddHttpContextAccessor();

// Repository Pattern için servis kayıtları
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Özel servislerimizi kaydediyoruz
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IProjeService, ProjeService>();
builder.Services.AddScoped<IOgrenciService, OgrenciService>();
builder.Services.AddScoped<IAkademisyenService, AkademisyenService>();
builder.Services.AddScoped<IBildirimService, BildirimService>();
builder.Services.AddScoped<ZamanDurumuService>();

// Dosya yükleme boyut sınırını ayarla (50MB)
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 52428800; // 50MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 52428800; // 50MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ZamanDurumu güncelleyen middleware'i ekleyelim
app.UseZamanDurumuUpdate();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
