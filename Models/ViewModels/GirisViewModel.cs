using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.ViewModels
{
    // Giris Yap ekranindaki form verilerini tasir
    public class GirisViewModel
    {
        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        // Sifre kutusu olarak gorunmesi icin Password tipinde isaretlendi
        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;

        // Isaretlenirse tarayici kapansa bile oturum cerezi (cookie) kalici olur
        [Display(Name = "Beni Hatırla")]
        public bool BeniHatirla { get; set; }
    }
}