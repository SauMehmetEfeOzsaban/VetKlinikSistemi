using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinikSistemi.Models.Entities;
using VetKlinikSistemi.Services.Interfaces;

namespace VetKlinikSistemi.Controllers
{
    // Klinik ekleme, guncelleme ve silme islemlerini sadece Admin yapabilir
    [Authorize(Roles = "Admin")]
    public class KliniklerController : Controller
    {
        private readonly IKlinikServisi _klinikServisi;

        public KliniklerController(IKlinikServisi klinikServisi)
        {
            _klinikServisi = klinikServisi;
        }

        // Tum klinikleri listeler
        public async Task<IActionResult> Index()
        {
            var klinikler = await _klinikServisi.TumKlinikleriGetirAsync();
            return View(klinikler);
        }

        // Yeni klinik ekleme formunu acar
        [HttpGet]
        public IActionResult Ekle()
        {
            return View(new Klinik());
        }

        // Formdan gelen yeni klinigi dogrulayip veritabanina kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Klinik klinik)
        {
            if (!ModelState.IsValid)
            {
                return View(klinik);
            }

            var sonuc = await _klinikServisi.EkleAsync(klinik);
            if (!sonuc.Basarili)
            {
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
                return View(klinik);
            }

            TempData["Basari"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Index));
        }

        // Secilen klinigin duzenleme formunu acar
        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var klinik = await _klinikServisi.IdIleGetirAsync(id);
            if (klinik == null)
            {
                TempData["Hata"] = "Klinik bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            return View(klinik);
        }

        // Guncellenen klinik bilgilerini kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(Klinik klinik)
        {
            if (!ModelState.IsValid)
            {
                return View(klinik);
            }

            var sonuc = await _klinikServisi.GuncelleAsync(klinik);
            if (!sonuc.Basarili)
            {
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
                return View(klinik);
            }

            TempData["Basari"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Index));
        }

        // Klinigi siler (bagli kayit varsa servis uyari mesaji doner)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var sonuc = await _klinikServisi.SilAsync(id);
            if (sonuc.Basarili)
            {
                TempData["Basari"] = sonuc.Mesaj;
            }
            else
            {
                TempData["Hata"] = sonuc.Mesaj;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}