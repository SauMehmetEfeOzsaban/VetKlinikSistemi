using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VetKlinikSistemi.Models.Entities;
using VetKlinikSistemi.Models.ViewModels;

namespace VetKlinikSistemi.Controllers
{
    // Kullanici kayit, giris, cikis, profil ve yetkisiz erisim islemlerini yonetir
    public class HesapController : Controller
    {
        private readonly UserManager<Kullanici> _userManager;
        private readonly SignInManager<Kullanici> _signInManager;

        public HesapController(UserManager<Kullanici> userManager, SignInManager<Kullanici> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Hesap/KayitOl
        [HttpGet]
        public IActionResult KayitOl()
        {
            // Kullanici zaten giris yaptiysa ana sayfaya yonlendir
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Hesap/KayitOl
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KayitOl(KayitViewModel model)
        {
            // Sunucu tarafi dogrulama (Server-side validation)
            if (!ModelState.IsValid)
                return View(model);

            var yeniKullanici = new Kullanici
            {
                UserName = model.Email,
                Email = model.Email,
                Ad = model.Ad.Trim(),
                Soyad = model.Soyad.Trim(),
                PhoneNumber = model.Telefon.Trim()
            };

            // Kullaniciyi sifreyle birlikte veritabanina kaydet
            var sonuc = await _userManager.CreateAsync(yeniKullanici, model.Sifre);

            if (sonuc.Succeeded)
            {
                // Kayit olan her yeni kullaniciya otomatik olarak "Uye" (hayvan sahibi) rolu verilir
                await _userManager.AddToRoleAsync(yeniKullanici, "Uye");

                // Kayit basarili olunca kullaniciyi otomatik giris yaptir
                await _signInManager.SignInAsync(yeniKullanici, isPersistent: false);

                TempData["Basari"] = "Kaydınız başarıyla oluşturuldu. Hoş geldiniz!";
                return RedirectToAction("Index", "Home");
            }

            // Identity tarafindan donen hatalari (ornek: e-posta zaten kayitli) forma ekle
            foreach (var hata in sonuc.Errors)
            {
                ModelState.AddModelError(string.Empty, hata.Description);
            }

            return View(model);
        }

        // GET: /Hesap/GirisYap
        [HttpGet]
        public IActionResult GirisYap(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Hesap/GirisYap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GirisYap(GirisViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            // E-posta ve sifre ile giris denemesi yap
            var sonuc = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Sifre,
                model.BeniHatirla,
                lockoutOnFailure: false);

            if (sonuc.Succeeded)
            {
                // Guvenlik: Sadece kendi sitemizdeki yerel adreslere yonlendir
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                TempData["Basari"] = "Başarıyla giriş yaptınız.";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            return View(model);
        }

        // POST: /Hesap/CikisYap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CikisYap()
        {
            await _signInManager.SignOutAsync();
            TempData["Basari"] = "Çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Hesap/YetkisizErisim
        [HttpGet]
        public IActionResult YetkisizErisim()
        {
            return View();
        }

        // GET: /Hesap/Profil
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profil()
        {
            var kullanici = await _userManager.GetUserAsync(User);
            if (kullanici == null)
                return RedirectToAction("GirisYap");

            var roller = await _userManager.GetRolesAsync(kullanici);

            var model = new ProfilViewModel
            {
                Ad = kullanici.Ad,
                Soyad = kullanici.Soyad,
                Email = kullanici.Email ?? string.Empty,
                Telefon = kullanici.PhoneNumber,
                Roller = roller.ToList(),
                McpApiToken = kullanici.McpApiToken
            };

            return View(model);
        }

        // POST: /Hesap/Profil
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profil(ProfilViewModel model)
        {
            var kullanici = await _userManager.GetUserAsync(User);
            if (kullanici == null)
                return RedirectToAction("GirisYap");

            if (!ModelState.IsValid)
            {
                model.Email = kullanici.Email ?? string.Empty;
                model.Roller = (await _userManager.GetRolesAsync(kullanici)).ToList();
                model.McpApiToken = kullanici.McpApiToken;
                return View(model);
            }

            kullanici.Ad = model.Ad.Trim();
            kullanici.Soyad = model.Soyad.Trim();
            kullanici.PhoneNumber = model.Telefon?.Trim();

            var sonuc = await _userManager.UpdateAsync(kullanici);
            if (sonuc.Succeeded)
            {
                TempData["Basari"] = "Profil bilgileriniz güncellendi.";
                return RedirectToAction("Profil");
            }

            foreach (var hata in sonuc.Errors)
            {
                ModelState.AddModelError(string.Empty, hata.Description);
            }

            model.Email = kullanici.Email ?? string.Empty;
            model.Roller = (await _userManager.GetRolesAsync(kullanici)).ToList();
            model.McpApiToken = kullanici.McpApiToken;
            return View(model);
        }

        // POST: /Hesap/McpTokenOlustur
        // Kullanicinin MCP sunucusuna baglanmasi icin guvenli rastgele anahtar uretir
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> McpTokenOlustur()
        {
            var kullanici = await _userManager.GetUserAsync(User);
            if (kullanici == null)
                return RedirectToAction("GirisYap");

            // 32 byte'lik kriptografik olarak guvenli rastgele hex anahtar uretiyoruz
            var rastgeleBaytlar = RandomNumberGenerator.GetBytes(32);
            kullanici.McpApiToken = Convert.ToHexString(rastgeleBaytlar).ToLowerInvariant();

            await _userManager.UpdateAsync(kullanici);

            TempData["Basari"] = "Yeni MCP API anahtarınız oluşturuldu.";
            return RedirectToAction("Profil");
        }
    }
}