using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetKlinikSistemi.Models.Entities;
using VetKlinikSistemi.Models.ViewModels;
using VetKlinikSistemi.Services.Interfaces;

namespace VetKlinikSistemi.Controllers
{
    // Asi islemlerine sadece giris yapmis Uye, Veteriner ve Admin erisebilir
    [Authorize(Roles = "Uye,Veteriner,Admin")]
    public class AsiKayitlariController : Controller
    {
        private readonly IAsiKaydiServisi _asiKaydiServisi;
        private readonly UserManager<Kullanici> _userManager;

        public AsiKayitlariController(
            IAsiKaydiServisi asiKaydiServisi,
            UserManager<Kullanici> userManager)
        {
            _asiKaydiServisi = asiKaydiServisi;
            _userManager = userManager;
        }

        // Oturumdaki kullanicinin Id bilgisini verir
        private string GirisYapanKullaniciId() => _userManager.GetUserId(User) ?? string.Empty;

        // Admin veya Veteriner ise tum kayitlari gorebilir, Uye ise sadece kendi hayvanlarini
        private bool TumunuGorebilirMi() => User.IsInRole("Admin") || User.IsInRole("Veteriner");

        // Asi kayitlarini listeler (istege bagli hayvana gore filtreler)
        public async Task<IActionResult> Index(int? evcilHayvanId)
        {
            var kullaniciId = GirisYapanKullaniciId();
            var tumunuGorebilirMi = TumunuGorebilirMi();

            var kayitlar = await _asiKaydiServisi.ListeleAsync(kullaniciId, tumunuGorebilirMi, evcilHayvanId);
            var yaklasanlar = await _asiKaydiServisi.YaklasanAsilariGetirAsync(kullaniciId, tumunuGorebilirMi, 30);

            ViewBag.YaklasanAsilar = yaklasanlar;
            ViewBag.SeciliHayvanId = evcilHayvanId;
            await HayvanListesiniDoldurAsync(evcilHayvanId);

            return View(kayitlar);
        }

        // Yeni asi kaydi ekleme formunu acar
        [HttpGet]
        public async Task<IActionResult> Ekle(int? evcilHayvanId)
        {
            await HayvanListesiniDoldurAsync(evcilHayvanId);

            var model = new AsiKaydiFormViewModel
            {
                EvcilHayvanId = evcilHayvanId ?? 0,
                YapildigiTarih = DateTime.Today
            };

            return View(model);
        }

        // Yeni asi kaydini kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(AsiKaydiFormViewModel model)
        {
            // Sunucu tarafi tarih kurali kontrolu
            if (model.SonrakiDozTarihi.HasValue && model.SonrakiDozTarihi.Value.Date < model.YapildigiTarih.Date)
            {
                ModelState.AddModelError(nameof(model.SonrakiDozTarihi),
                    "Bir sonraki doz tarihi, aşının yapıldığı tarihten önce olamaz.");
            }

            if (!ModelState.IsValid)
            {
                await HayvanListesiniDoldurAsync(model.EvcilHayvanId);
                return View(model);
            }

            var yeniKayit = new AsiKaydi
            {
                EvcilHayvanId = model.EvcilHayvanId,
                AsiAdi = model.AsiAdi.Trim(),
                YapildigiTarih = model.YapildigiTarih,
                SonrakiDozTarihi = model.SonrakiDozTarihi
            };

            var sonuc = await _asiKaydiServisi.EkleAsync(yeniKayit, GirisYapanKullaniciId(), TumunuGorebilirMi());

            if (sonuc.YetkisizMi)
            {
                return Forbid();
            }

            if (!sonuc.Basarili)
            {
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
                await HayvanListesiniDoldurAsync(model.EvcilHayvanId);
                return View(model);
            }

            TempData["Basari"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Index), new { evcilHayvanId = model.EvcilHayvanId });
        }

        // Asi kaydi duzenleme formunu acar (IDOR korumali)
        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var sonuc = await _asiKaydiServisi.DetayGetirAsync(id, GirisYapanKullaniciId(), TumunuGorebilirMi());

            if (sonuc.YetkisizMi)
            {
                return Forbid();
            }

            if (sonuc.Kayit == null)
            {
                return NotFound();
            }

            var model = new AsiKaydiFormViewModel
            {
                Id = sonuc.Kayit.Id,
                EvcilHayvanId = sonuc.Kayit.EvcilHayvanId,
                AsiAdi = sonuc.Kayit.AsiAdi,
                YapildigiTarih = sonuc.Kayit.YapildigiTarih,
                SonrakiDozTarihi = sonuc.Kayit.SonrakiDozTarihi
            };

            await HayvanListesiniDoldurAsync(model.EvcilHayvanId);
            return View(model);
        }

        // Duzenlenen asi kaydini kaydeder (IDOR korumali)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(AsiKaydiFormViewModel model)
        {
            if (model.SonrakiDozTarihi.HasValue && model.SonrakiDozTarihi.Value.Date < model.YapildigiTarih.Date)
            {
                ModelState.AddModelError(nameof(model.SonrakiDozTarihi),
                    "Bir sonraki doz tarihi, aşının yapıldığı tarihten önce olamaz.");
            }

            if (!ModelState.IsValid)
            {
                await HayvanListesiniDoldurAsync(model.EvcilHayvanId);
                return View(model);
            }

            var guncelKayit = new AsiKaydi
            {
                Id = model.Id,
                EvcilHayvanId = model.EvcilHayvanId,
                AsiAdi = model.AsiAdi.Trim(),
                YapildigiTarih = model.YapildigiTarih,
                SonrakiDozTarihi = model.SonrakiDozTarihi
            };

            var sonuc = await _asiKaydiServisi.GuncelleAsync(guncelKayit, GirisYapanKullaniciId(), TumunuGorebilirMi());

            if (sonuc.YetkisizMi)
            {
                return Forbid();
            }

            if (!sonuc.Basarili)
            {
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
                await HayvanListesiniDoldurAsync(model.EvcilHayvanId);
                return View(model);
            }

            TempData["Basari"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Index));
        }

        // Asi kaydini siler (IDOR korumali)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var sonuc = await _asiKaydiServisi.SilAsync(id, GirisYapanKullaniciId(), TumunuGorebilirMi());

            if (sonuc.YetkisizMi)
            {
                return Forbid();
            }

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

        // Formlardaki evcil hayvan acilir listesini (dropdown) doldurur
        private async Task HayvanListesiniDoldurAsync(int? seciliId = null)
        {
            var hayvanlar = await _asiKaydiServisi.YetkiliHayvanlariGetirAsync(
                GirisYapanKullaniciId(), TumunuGorebilirMi());

            ViewBag.EvcilHayvanlar = new SelectList(hayvanlar, "Id", "Ad", seciliId);
        }
    }
}