using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using SakaryaERP.BackgroundJobs;
using SakaryaERP.Data;
using SakaryaERP.Data.Repositories;
using SakaryaERP.HealthChecks;
using SakaryaERP.Middleware;
using SakaryaERP.Models;
using SakaryaERP.Services;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

QuestPDF.Settings.License = LicenseType.Community;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(new CompactJsonFormatter(), "logs/sakaryaerp-.json",
        rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// Varsayılan 30 dakika yerine 5 dakika: bir kullanıcı pasif yapıldığında (security stamp
// döndürülünce) mevcut oturumunun düşmesi için beklenen azami süre.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(5);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICariRepository, CariRepository>();
builder.Services.AddScoped<ICariService, CariService>();
builder.Services.AddScoped<IBankaHesabiService, BankaHesabiService>();
builder.Services.AddScoped<IKasaHesabiService, KasaHesabiService>();
builder.Services.AddScoped<ISubeService, SubeService>();
builder.Services.AddScoped<ICariFisiService, CariFisiService>();
builder.Services.AddScoped<ICekSenetService, CekSenetService>();
builder.Services.AddScoped<IMalzemeKategoriService, MalzemeKategoriService>();
builder.Services.AddScoped<IMalzemeService, MalzemeService>();
builder.Services.AddScoped<IMalzemeHareketFisiService, MalzemeHareketFisiService>();
builder.Services.AddScoped<IAlisSiparisiService, AlisSiparisiService>();
builder.Services.AddScoped<IAlisIrsaliyesiService, AlisIrsaliyesiService>();
builder.Services.AddScoped<IAlisFaturasiService, AlisFaturasiService>();
builder.Services.AddScoped<IMusteriTalebiService, MusteriTalebiService>();
builder.Services.AddScoped<ISatisTeklifiService, SatisTeklifiService>();
builder.Services.AddScoped<ISatisSiparisiService, SatisSiparisiService>();
builder.Services.AddScoped<ISevkIrsaliyesiService, SevkIrsaliyesiService>();
builder.Services.AddScoped<ISatisFaturasiService, SatisFaturasiService>();
builder.Services.AddScoped<IHesapPlaniService, HesapPlaniService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IOnayYetkisiService, OnayYetkisiService>();
builder.Services.AddScoped<IAdminEmailProvider, IdentityAdminEmailProvider>();
builder.Services.AddScoped<IBildirimService, BildirimService>();
builder.Services.AddHostedService<GunlukBildirimHostedService>();

builder.Services.AddHealthChecks()
    .AddCheck<VeritabaniHealthCheck>("veritabani");

builder.Services.AddControllersWithViews(options =>
{
    // Tüm controller/action'lar varsayılan olarak girişli kullanıcı gerektirir;
    // anonim erişim gereken (Login, ForgotPassword vb.) yerler [AllowAnonymous] ile işaretlenir.
    var girisliKullaniciPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(girisliKullaniciPolicy));
});

var app = builder.Build();

// Roller ve hesap planı her ortamda gerekli; Development/Production ayrımı yapılmaz.
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedFoundationAsync(scope.ServiceProvider);
}

// `dotnet SakaryaERP.dll --seed-demo` ile elle tetiklenir (Gün 29: Oracle Cloud'a ilk
// deploy sonrası tek seferlik demo veri yüklemesi). Web sunucusunu başlatmadan seed edip çıkar.
if (args.Contains("--seed-demo"))
{
    using var scope = app.Services.CreateScope();
    await DemoSeeder.SeedAsync(scope.ServiceProvider);
    return;
}

// nginx SSL/domain kurulana kadar sadece HTTP üzerinden proxy yapıyor; zorla https
// yönlendirmesi bu ortamda sonsuz redirect'e yol açar. Domain + Let's Encrypt
// eklendiğinde appsettings'te EnableHttpsRedirection true yapılıp nginx 443 dinlemeli.
var httpsYonlendirmeAktif = builder.Configuration.GetValue<bool>("EnableHttpsRedirection");

if (!app.Environment.IsDevelopment() && httpsYonlendirmeAktif)
{
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseStatusCodePagesWithReExecute("/Home/DurumKodu/{0}");

if (httpsYonlendirmeAktif)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        var sonuc = new
        {
            durum = report.Status.ToString(),
            kontroller = report.Entries.Select(e => new { ad = e.Key, durum = e.Value.Status.ToString(), aciklama = e.Value.Description })
        };
        await context.Response.WriteAsJsonAsync(sonuc);
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await DbSeeder.SeedDevKolayligiAsync(scope.ServiceProvider);
}

app.Run();