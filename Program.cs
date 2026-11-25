using VetKlinikSistemi.Services.Interfaces;
using VetKlinikSistemi.Services.Implementations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Data;
using VetKlinikSistemi.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// MVC servisleri
builder.Services.AddControllersWithViews();

// Ortak servis katmani kayitlari (MVC, REST API ve MCP bunlari kullanacak)
builder.Services.AddScoped<IKlinikServisi, KlinikServisi>();
builder.Services.AddScoped<IHizmetServisi, HizmetServisi>();
builder.Services.AddScoped<IVeterinerServisi, VeterinerServisi>();
// Veritabani baglantisi (User Secrets uzerinden okunur)
builder.Services.AddDbContext<UygulamaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Core Identity ve rol yapilandirmasi
builder.Services.AddIdentity<Kullanici, IdentityRole>(options =>
{
    // Odevdeki "sau" sifresinin kabul edilmesi icin sifre kurallari esnetildi
    options.Password.RequiredLength = 3;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<UygulamaDbContext>()
.AddDefaultTokenProviders();

// Giris yapilmadiginda veya yetki yetmediginde yonlendirilecek sayfalar
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Hesap/GirisYap";
    options.LogoutPath = "/Hesap/CikisYap";
    options.AccessDeniedPath = "/Hesap/YetkisizErisim";
});

var app = builder.Build();

// Uygulama baslarken rolleri, admin hesabini ve temel verileri olustur
await VeriBaslatici.BaslatAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kimlik dogrulama (Authentication) mutlaka yetkilendirmeden (Authorization) once gelmeli
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();