using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Models.Entities;

namespace VetKlinikSistemi.Data
{
    // Uygulama ilk calistiginda zorunlu rolleri, admin hesabini ve temel verileri ekler
    public static class VeriBaslatici
    {
        public static async Task BaslatAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UygulamaDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Bekleyen migration varsa otomatik uygular
            await context.Database.MigrateAsync();

            // 1. Zorunlu 3 rolu olustur
            string[] roller = { "Admin", "Veteriner", "Uye" };
            foreach (var rol in roller)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // 2. Odevde istenen zorunlu Admin hesabini olustur
            string adminEmail = "b241210058@sakarya.edu.tr";
            var adminKullanici = await userManager.FindByEmailAsync(adminEmail);

            if (adminKullanici == null)
            {
                adminKullanici = new Kullanici
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Ad = "Mehmet Efe",
                    Soyad = "Özsaban",
                    PhoneNumber = "05000000000"
                };

                var sonuc = await userManager.CreateAsync(adminKullanici, "sau");
                if (sonuc.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminKullanici, "Admin");
                }
            }

            // 3. Varsayilan uzmanlik alanlarini ekle (yoksa)
            if (!await context.UzmanlikAlanlari.AnyAsync())
            {
                var uzmanliklar = new List<UzmanlikAlani>
                {
                    new UzmanlikAlani { Ad = "Küçük Hayvan" },
                    new UzmanlikAlani { Ad = "Egzotik Hayvan" },
                    new UzmanlikAlani { Ad = "Cerrahi" },
                    new UzmanlikAlani { Ad = "Dermatoloji" },
                    new UzmanlikAlani { Ad = "İç Hastalıkları" },
                    new UzmanlikAlani { Ad = "Diş Hekimliği" }
                };

                await context.UzmanlikAlanlari.AddRangeAsync(uzmanliklar);
                await context.SaveChangesAsync();
            }
        }
    }
}