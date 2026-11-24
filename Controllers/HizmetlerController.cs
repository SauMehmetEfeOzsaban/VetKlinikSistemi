using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetKlinikSistemi.Models.Entities;
using VetKlinikSistemi.Services.Interfaces;

namespace VetKlinikSistemi.Controllers
{
    // Hizmet ekleme, guncelleme ve silme islemlerini sadece Admin yapabilir
    [Authorize(Roles = "Admin")]
    public class HizmetlerController : Controller
    {
        private readonly IHizmetServisi _hizmetServisi;
        private readonly IKlinikServisi _klinikServisi;

        public HizmetlerController(IHizmetServisi hizmetServisi, IKlinikServisi klinikServisi)
        {
            _hizmetServisi = hizmetServisi;
            _klinikServisi = klinikServisi;
        }

        // Tum hizmetleri listeler (istenirse klinik bazli filtrelenebilir)
        public async Task<IActionResult> Index(int? klinikId)
        {
            await KlinikListesiniDoldurAsync(klinikId);
            var hizmetler = await _hizmetServisi.TumHizmetleriGetirAsync(klinikId);
            return View(hizmetler);
        }

        // Yeni hizmet ekleme formunu acar
        [HttpGet]
        public async Task<IActionResult> Ekle()
        {
            await KlinikListesiniDoldurAsync();
            return View(new Hizmet { SureDakika = 30 });
        }

        // Formdan gelen yeni hizmeti dogrulayip veritabanina kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Hizmet hizmet)
        {
            if (!ModelState.IsValid)
            {
                await KlinikListesiniDoldurAsync(hizmet.KlinikId);
                return View(hizmet);
            }

            var sonuc = await _hizmetServisi.EkleAsync(hizmet);
            if (!sonuc.Basarili)
            {
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
                await KlinikListesiniDoldurAsync(hizmet.KlinikId);
                return View(hizmet);
            }

            TempData["Basari"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Index));
        }

        // Secilen hizmetin duzenleme formunu acar
        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var hizmet = await _hizmetServisi.IdIleGetirAsync(id);
            if (hizmet == null)
            {
                TempData["Hata"] = "Hizmet bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            await KlinikListesiniDoldurAsync(hizmet.KlinikId);
            return View(hizmet);
        }

        // Guncellenen hizmet bilgilerini kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(Hizmet hizmet)
        {
            if (!ModelState.IsValid)
            {
                await KlinikListesiniDoldurAsync(hizmet.KlinikId);
                return View(hizmet);
            }

            var sonuc = await _hizmetServisi.GuncelleAsync(hizmet);
            if (!sonuc.Basarili)
            {
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
                await KlinikListesiniDoldurAsync(hizmet.KlinikId);
                return View(hizmet);
            }

            TempData["Basari"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Index));
        }

        // Hizmeti siler (bagli veteriner veya randevu varsa uyari doner)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var sonuc = await _hizmetServisi.SilAsync(id);
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

        // Dropdown (acilir liste) icin klinikleri ViewBag'e dolduran yardimci metot
        private async Task KlinikListesiniDoldurAsync(int? seciliKlinikId = null)
        {
            var klinikler = await _klinikServisi.TumKlinikleriGetirAsync();
            ViewBag.Klinikler = new SelectList(klinikler, "Id", "Ad", seciliKlinikId);
        }
    }
}