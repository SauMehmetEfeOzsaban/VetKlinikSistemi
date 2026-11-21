using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VetKlinikSistemi.Models.Entities
{
    // IdentityUser'dan kalitim alarak e-posta, sifre, telefon gibi hazir alanlari kullaniyoruz
    public class Kullanici : IdentityUser
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        // MCP sunucusuna baglanirken kullaniciyi dogrulamak icin ozel anahtar
        [StringLength(128)]
        public string? McpApiToken { get; set; }

        // Bir kullanicinin birden fazla evcil hayvani olabilir
        public ICollection<EvcilHayvan> EvcilHayvanlar { get; set; } = new List<EvcilHayvan>();

        // Eger kullanici veteriner rolundeyse veteriner profiliyle bire-bir baglanir
        public Veteriner? VeterinerProfili { get; set; }
    }
}