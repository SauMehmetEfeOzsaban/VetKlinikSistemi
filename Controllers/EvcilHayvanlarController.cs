using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VetKlinikSistemi.Models.Entities;
using VetKlinikSistemi.Models.ViewModels;
using VetKlinikSistemi.Services.Interfaces;

namespace VetKlinikSistemi.Controllers
{
    // Evcil hayvan islemlerini sadece giris yapmis Uye ve Admin kullanabilir
    [Authorize(Roles = "Uye,Admin")]
    public class EvcilHayvanlarController : Controller
    {
        private readonly IEvcilHayvanServisi _evcilHayvanServisi;
        private readonly UserManager<Kullanici> _userManager;
        private readonly IWebHostEnvironment _env;

        // Izin verilen fotograf uzantilari ve maksimum dosya boyutu (2 MB)
        private static readonly string[] IzinliUzantilar = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaksimumDosyaBoyutu = 2 * 1024 * 1024;

        public EvcilHayvanlarController(
            IEvcilHayvanServisi evcilHayvanServisi,
            UserManager<Kullanici> userManager,
            IWebHostEnvironment env)
        {
            _evcilHayvanServisi = evcilHayvanServisi;
            _userManager = userManager;
            _env = env;
        }

        // Giris yapan kullanicinin Id bilgisini verir
        private string GirisYapanKullaniciId() => _userManager.GetUserId(User) ?? string.Empty;

        // Giris yapan kullanici Admin mi kontrol eder
        private bool AdminMi() => User.IsInRole("Admin");

        // Evcil hayvanlari listeler
        public async Task<IActionResult> Index()
        {
            var hayvanlar = await _evcilHayvanServisi.ListeleAsync(GirisYapanKullaniciId(), AdminMi());
            return View(hayvanlar);
        }

        // Secilen evcil hayvanin detay sayfasini acar (IDOR korumali)
        public async Task<IActionResult> Detay(int id)
        {
            var sonuc = await _evcilHayvanServisi.DetayGetirAsync(id, GirisYapanKullaniciId(), AdminMi());

            if (sonuc.YetkisizMi)
            {
                return Forbid();
            }

            if (sonuc.Hayvan == null)
            {
                return NotFound();
            }

            return View(sonuc.Hayvan);
        }

        // Yeni evcil hayvan ekleme formunu acar
        [HttpGet]
        public IActionResult Ekle()
        {
            return View(new EvcilHayvanFormViewModel());
        }

        // Yeni evcil hayvani kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(EvcilHayvanFormViewModel model)
        {
            if (model.DogumTarihi.Date > DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.DogumTarihi), "Doğum tarihi gelecekte bir tarih olamaz.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Fotograf yuklendiyse kontrol et ve kaydet
            string? fotografYolu = null;
            if (model.FotografDosyasi != null && model.FotografDosyasi.Length > 0)
            {
                var yukleme = await FotografKaydetAsync(model.FotografDosyasi);
                if (!yukleme.Basarili)
                {
                    ModelState.AddModelError(nameof(model.FotografDosyasi), yukleme.HataMesaji!);
                    return View(model);
                }
                fotografYolu = yukleme.DosyaYolu;
            }

            // Hayvanin sahibi her zaman oturumdaki kullanici olarak atanir
            var yeniHayvan = new EvcilHayvan
            {
                SahipId = GirisYapanKullaniciId(),
                Ad = model.Ad.Trim(),
                Tur = model.Tur.Trim(),
                Irk = model.Irk.Trim(),
                DogumTarihi = model.DogumTarihi,
                Kilo = model.Kilo,
                FotografYolu = fotografYolu
            };

            var sonuc = await _evcilHayvanServisi.EkleAsync(yeniHayvan);
            if (!sonuc.Basarili)
            {
                FotografSil(fotografYolu);
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
                return View(model);
            }

            TempData["Basari"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Index));
        }

        // Duzenleme formunu acar (IDOR korumali)
        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var sonuc = await _evcilHayvanServisi.DetayGetirAsync(id, GirisYapanKullaniciId(), AdminMi());

            if (sonuc.YetkisizMi)
            {
                return Forbid();
            }

            if (sonuc.Hayvan == null)
            {
                return NotFound();
            }

            var model = new EvcilHayvanFormViewModel
            {
                Id = sonuc.Hayvan.Id,
                Ad = sonuc.Hayvan.Ad,
                Tur = sonuc.Hayvan.Tur,
                Irk = sonuc.Hayvan.Irk,
                DogumTarihi = sonuc.Hayvan.DogumTarihi,
                Kilo = sonuc.Hayvan.Kilo,
                MevcutFotografYolu = sonuc.Hayvan.FotografYolu
            };

            return View(model);
        }

        // Duzenlenen evcil hayvani kaydeder (IDOR korumali)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(EvcilHayvanFormViewModel model)
        {
            if (model.DogumTarihi.Date > DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.DogumTarihi), "Doğum tarihi gelecekte bir tarih olamaz.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Once kaydin varligini ve yetkiyi kontrol et
            var mevcutKontrol = await _evcilHayvanServisi.DetayGetirAsync(model.Id, GirisYapanKullaniciId(), AdminMi());
            if (mevcutKontrol.YetkisizMi)
            {
                return Forbid();
            }
            if (mevcutKontrol.Hayvan == null)
            {
                return NotFound();
            }

            var eskiFotografYolu = mevcutKontrol.Hayvan.FotografYolu;
            var guncelFotografYolu = eskiFotografYolu;

            // Yeni fotograf secildiyse kaydet
            if (model.FotografDosyasi != null && model.FotografDosyasi.Length > 0)
            {
                var yukleme = await FotografKaydetAsync(model.FotografDosyasi);
                if (!yukleme.Basarili)
                {
                    model.MevcutFotografYolu = eskiFotografYolu;
                    ModelState.AddModelError(nameof(model.FotografDosyasi), yukleme.HataMesaji!);
                    return View(model);
                }
                guncelFotografYolu = yukleme.DosyaYolu;
            }

            var guncelHayvan = new EvcilHayvan
            {
                Id = model.Id,
                Ad = model.Ad.Trim(),
                Tur = model.Tur.Trim(),
                Irk = model.Irk.Trim(),
                DogumTarihi = model.DogumTarihi,
                Kilo = model.Kilo,
                FotografYolu = guncelFotografYolu
            };

            var sonuc = await _evcilHayvanServisi.GuncelleAsync(guncelHayvan, GirisYapanKullaniciId(), AdminMi());
            if (sonuc.YetkisizMi)
            {
                return Forbid();
            }

            if (!sonuc.Basarili)
            {
                // Guncelleme basarisizsa yeni yuklenen dosyayi temizle
                if (guncelFotografYolu != eskiFotografYolu)
                {
                    FotografSil(guncelFotografYolu);
                }
                model.MevcutFotografYolu = eskiFotografYolu;
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
                return View(model);
            }

            // Yeni fotograf yuklendiyse eski fotografi diskten temizle
            if (guncelFotografYolu != eskiFotografYolu)
            {
                FotografSil(eskiFotografYolu);
            }

            TempData["Basari"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Index));
        }

        // Evcil hayvani siler (IDOR korumali)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var sonuc = await _evcilHayvanServisi.SilAsync(id, GirisYapanKullaniciId(), AdminMi());

            if (sonuc.YetkisizMi)
            {
                return Forbid();
            }

            if (sonuc.Basarili)
            {
                FotografSil(sonuc.SilinenFotografYolu);
                TempData["Basari"] = sonuc.Mesaj;
            }
            else
            {
                TempData["Hata"] = sonuc.Mesaj;
            }

            return RedirectToAction(nameof(Index));
        }

        // Fotografi uzanti ve boyut kontrolu yaparak wwwroot/uploads/hayvanlar klasorune kaydeder
        private async Task<(bool Basarili, string? DosyaYolu, string? HataMesaji)> FotografKaydetAsync(IFormFile dosya)
        {
            var uzanti = Path.GetExtension(dosya.FileName).ToLowerInvariant();
            if (!IzinliUzantilar.Contains(uzanti))
            {
                return (false, null, "Sadece .jpg, .jpeg, .png ve .webp uzantılı fotoğraflar yüklenebilir.");
            }

            if (dosya.Length > MaksimumDosyaBoyutu)
            {
                return (false, null, "Fotoğraf boyutu en fazla 2 MB olabilir.");
            }

            var klasorYolu = Path.Combine(_env.WebRootPath, "uploads", "hayvanlar");
            if (!Directory.Exists(klasorYolu))
            {
                Directory.CreateDirectory(klasorYolu);
            }

            var yeniDosyaAdi = $"{Guid.NewGuid()}{uzanti}";
            var tamYol = Path.Combine(klasorYolu, yeniDosyaAdi);

            using (var akis = new FileStream(tamYol, FileMode.Create))
            {
                await dosya.CopyToAsync(akis);
            }

            return (true, $"/uploads/hayvanlar/{yeniDosyaAdi}", null);
        }

        // Diskteki eski fotografi guvenli sekilde siler
        private void FotografSil(string? goreceliYol)
        {
            if (string.IsNullOrWhiteSpace(goreceliYol))
            {
                return;
            }

            var tamYol = Path.Combine(_env.WebRootPath, goreceliYol.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(tamYol))
            {
                System.IO.File.Delete(tamYol);
            }
        }
    }
}