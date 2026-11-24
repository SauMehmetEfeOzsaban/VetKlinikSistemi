using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Data;
using VetKlinikSistemi.Models.Entities;
using VetKlinikSistemi.Models.ViewModels;

namespace VetKlinikSistemi.Controllers
{
    // Bu controller'a sadece Admin rolundeki kullanicilar erisebilir
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UygulamaDbContext _db;
        private readonly UserManager<Kullanici> _userManager;

        public AdminController(UygulamaDbContext db, UserManager<Kullanici> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Admin paneli ana sayfasi: sistemdeki genel sayilari gosterir
        public async Task<IActionResult> Index()
        {
            var uyeler = await _userManager.GetUsersInRoleAsync("Uye");

            var model = new AdminPanelViewModel
            {
                ToplamKlinik = await _db.Klinikler.CountAsync(),
                ToplamHizmet = await _db.Hizmetler.CountAsync(),
                ToplamVeteriner = await _db.Veterinerler.CountAsync(),
                ToplamUye = uyeler.Count,
                BekleyenRandevu = await _db.Randevular.CountAsync(r => r.Durum == RandevuDurumu.Bekliyor),
                ToplamRandevu = await _db.Randevular.CountAsync()
            };

            return View(model);
        }
    }
}