using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.ViewModels
{
    // Giris yapmis kullanicinin profil sayfasinda bilgilerini gormesi ve guncellemesi icin kullanilir
    public class ProfilViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        // E-posta sadece goruntuleme amacli gosterilir, formdan degistirilmez
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        // Telefon bilgisi istege bagli olarak guncellenebilir
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        // Kullanicinin sistemdeki rollerini (Admin, Veteriner, Uye) ekranda rozet olarak gostermek icin
        public List<string> Roller { get; set; } = new List<string>();

        // Yapay zeka (MCP) istemcisinde kimlik dogrulamasi icin kullanilacak kisisel anahtar
        [Display(Name = "MCP API Anahtarı")]
        public string? McpApiToken { get; set; }
    }
}