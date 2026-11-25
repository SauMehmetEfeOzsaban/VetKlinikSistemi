using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.ViewModels
{
    // Admin yeni veteriner eklerken hem giris hesabini hem profilini tek formda olusturur
    public class VeterinerEkleViewModel
    {
        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta (Giriş için)")]
        public string Eposta { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(3, ErrorMessage = "Şifre en az 3 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Klinik seçimi zorunludur.")]
        [Display(Name = "Görev Yaptığı Klinik")]
        public int KlinikId { get; set; }

        [StringLength(100)]
        [Display(Name = "Unvan")]
        public string? Unvan { get; set; }

        [StringLength(1000)]
        [Display(Name = "Biyografi")]
        public string? Biyografi { get; set; }

        // Formdaki checkboxlardan secilen uzmanlik ve hizmet id'leri buraya gelir
        public List<int> SeciliUzmanlikIdleri { get; set; } = new List<int>();
        public List<int> SeciliHizmetIdleri { get; set; } = new List<int>();
    }
}