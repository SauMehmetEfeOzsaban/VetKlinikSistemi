using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetKlinikSistemi.Models.Entities;
using VetKlinikSistemi.Models.ViewModels;
using VetKlinikSistemi.Services.Interfaces;

namespace VetKlinikSistemi.Controllers
{
    // Veteriner ve musaitlik yonetimi sadece Admin yetkisindedir
    [Authorize(Roles = "Admin")]
    public class VeterinerlerController : Controller
    {
        private readonly IVeterinerServisi _veterinerServisi;
        private readonly IKlinikServisi _klinikServisi;
        private readonly IHizmetServisi _hizmetServisi;

        public VeterinerlerController(
            IVeterinerServisi veterinerServisi,
            IKlinikServisi klinikServisi,
            IHizmetServisi hizmetServisi)
        {
            _veterinerServisi = veterinerServisi;
            _klinikServisi = klinikServisi;
            _hizmetServisi = hizmetServisi;
        }

        // Tum veterinerleri listeler (istenirse klinik bazli filtrelenebilir)
        public async Task<IActionResult> Index(int? klinikId)
        {
            var klinikler = await _klinikServisi.TumKlinikleriGetirAsync();
            ViewBag.Klinikler = new SelectList(klinikler, "Id", "Ad", klinikId);

            var veterinerler = await _veterinerServisi.TumVeterinerleriGetirAsync(klinikId);
            return View(veterinerler);
        }

        // Yeni veteriner ekleme formunu acar
        [HttpGet]
        public async Task<IActionResult> Ekle()
        {
            await FormSecenekleriniDoldurAsync();
            return View(new VeterinerEkleViewModel());
        }

        // Yeni veteriner ve giris hesabini kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(VeterinerEkleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await FormSecenekleriniDoldurAsync(model.KlinikId);
                return View(model);
            }

            var sonuc = await _veterinerServisi.EkleAsync(model);
            if (!sonuc.Basarili)
            {
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
                await FormSecenekleriniDoldurAsync(model.KlinikId);
                return View(model);
            }

            TempData["Basari"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Index));
        }

        // Secilen veterinerin duzenleme formunu acar
        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var veteriner = await _veterinerServisi.IdIleGetirAsync(id);
            if (veteriner == null)
            {
                TempData["Hata"] = "Veteriner bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var model = new VeterinerDuzenleViewModel
            {
                Id = veteriner.Id,
                AdSoyad = veteriner.AdSoyad,
                KlinikId = veteriner.KlinikId,
                Unvan = veteriner.Unvan,
                Biyografi = veteriner.Biyografi,
                SeciliUzmanlikIdleri = veteriner.VeterinerUzmanliklar.Select(vu => vu.UzmanlikAlaniId).ToList(),
                SeciliHizmetIdleri = veteriner.VeterinerHizmetler.Select(vh => vh.HizmetId).ToList()
            };

            await FormSecenekleriniDoldurAsync(model.KlinikId);
            return View(model);
        }

        // Guncellenen veteriner bilgilerini kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(VeterinerDuzenleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await FormSecenekleriniDoldurAsync(model.KlinikId);
                return View(model);
            }

            var sonuc = await _veterinerServisi.GuncelleAsync(model);
            if (!sonuc.Basarili)
            {
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
                await FormSecenekleriniDoldurAsync(model.KlinikId);
                return View(model);
            }

            TempData["Basari"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Index));
        }

        // Veterineri siler (randevusu varsa servis engeller)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var sonuc = await _veterinerServisi.SilAsync(id);
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

        // Secilen veterinerin musaitlik saatlerini listeler ve yeni saat ekleme formunu gosterir
        [HttpGet]
        public async Task<IActionResult> Musaitlikler(int id)
        {
            var veteriner = await _veterinerServisi.IdIleGetirAsync(id);
            if (veteriner == null)
            {
                TempData["Hata"] = "Veteriner bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var model = new MusaitlikFormViewModel
            {
                VeterinerId = veteriner.Id,
                VeterinerAdSoyad = veteriner.AdSoyad,
                KlinikAdi = veteriner.Klinik?.Ad ?? "-",
                KlinikAcilis = veteriner.Klinik?.AcilisSaati ?? new TimeSpan(9, 0, 0),
                KlinikKapanis = veteriner.Klinik?.KapanisSaati ?? new TimeSpan(18, 0, 0),
                BaslangicSaati = veteriner.Klinik?.AcilisSaati ?? new TimeSpan(9, 0, 0),
                BitisSaati = veteriner.Klinik?.KapanisSaati ?? new TimeSpan(18, 0, 0),
                MevcutMusaitlikler = await _veterinerServisi.MusaitlikleriGetirAsync(veteriner.Id)
            };

            return View(model);
        }

        // Veteriner icin yeni musaitlik saati ekler
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MusaitlikEkle(MusaitlikFormViewModel model)
        {
            var musaitlik = new VeterinerMusaitlik
            {
                VeterinerId = model.VeterinerId,
                Gun = model.Gun,
                BaslangicSaati = model.BaslangicSaati,
                BitisSaati = model.BitisSaati
            };

            var sonuc = await _veterinerServisi.MusaitlikEkleAsync(musaitlik);
            if (sonuc.Basarili)
            {
                TempData["Basari"] = sonuc.Mesaj;
            }
            else
            {
                TempData["Hata"] = sonuc.Mesaj;
            }

            return RedirectToAction(nameof(Musaitlikler), new { id = model.VeterinerId });
        }

        // Mevcut musaitlik saatini duzenleme formunu acar
        [HttpGet]
        public async Task<IActionResult> MusaitlikDuzenle(int id)
        {
            var musaitlik = await _veterinerServisi.MusaitlikIdIleGetirAsync(id);
            if (musaitlik == null || musaitlik.Veteriner == null)
            {
                TempData["Hata"] = "Müsaitlik kaydı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var model = new MusaitlikFormViewModel
            {
                Id = musaitlik.Id,
                VeterinerId = musaitlik.VeterinerId,
                VeterinerAdSoyad = musaitlik.Veteriner.AdSoyad,
                KlinikAdi = musaitlik.Veteriner.Klinik?.Ad ?? "-",
                KlinikAcilis = musaitlik.Veteriner.Klinik?.AcilisSaati ?? new TimeSpan(9, 0, 0),
                KlinikKapanis = musaitlik.Veteriner.Klinik?.KapanisSaati ?? new TimeSpan(18, 0, 0),
                Gun = musaitlik.Gun,
                BaslangicSaati = musaitlik.BaslangicSaati,
                BitisSaati = musaitlik.BitisSaati
            };

            return View(model);
        }

        // Guncellenen musaitlik saatini kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MusaitlikDuzenle(MusaitlikFormViewModel model)
        {
            var musaitlik = new VeterinerMusaitlik
            {
                Id = model.Id,
                VeterinerId = model.VeterinerId,
                Gun = model.Gun,
                BaslangicSaati = model.BaslangicSaati,
                BitisSaati = model.BitisSaati
            };

            var sonuc = await _veterinerServisi.MusaitlikGuncelleAsync(musaitlik);
            if (!sonuc.Basarili)
            {
                TempData["Hata"] = sonuc.Mesaj;
                return RedirectToAction(nameof(MusaitlikDuzenle), new { id = model.Id });
            }

            TempData["Basari"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Musaitlikler), new { id = model.VeterinerId });
        }

        // Musaitlik kaydini siler
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MusaitlikSil(int id, int veterinerId)
        {
            var sonuc = await _veterinerServisi.MusaitlikSilAsync(id);
            if (sonuc.Basarili)
            {
                TempData["Basari"] = sonuc.Mesaj;
            }
            else
            {
                TempData["Hata"] = sonuc.Mesaj;
            }

            return RedirectToAction(nameof(Musaitlikler), new { id = veterinerId });
        }

        // Ekle ve Duzenle formlarindaki klinik, uzmanlik ve hizmet listelerini doldurur
        private async Task FormSecenekleriniDoldurAsync(int? seciliKlinikId = null)
        {
            var klinikler = await _klinikServisi.TumKlinikleriGetirAsync();
            ViewBag.Klinikler = new SelectList(klinikler, "Id", "Ad", seciliKlinikId);
            ViewBag.Uzmanliklar = await _veterinerServisi.TumUzmanliklariGetirAsync();
            ViewBag.Hizmetler = await _hizmetServisi.TumHizmetleriGetirAsync();
        }
    }
}