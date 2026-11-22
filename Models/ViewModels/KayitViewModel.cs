using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.ViewModels
{
    // Kayit Ol ekranindaki formdan gelen verileri tasimak ve dogrulamak icin kullanilir
    public class KayitViewModel
    {
        // Bos gecilemez ve en fazla 50 karakter olabilir
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        // EmailAddress attribute'u girilen metnin gecerli e-posta formatinda olup olmadigini kontrol eder
        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        // Phone attribute'u telefon numarasi formatini kontrol eder
        [Required(ErrorMessage = "Telefon alanı zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; } = string.Empty;

        // DataType.Password sayesinde HTML input'u otomatik gizli (noktali) sifre kutusu olur
        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [MinLength(3, ErrorMessage = "Şifre en az 3 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;

        // Compare attribute'u bu alanin yukaridaki Sifre alaniyla birebir ayni olmasini zorunlu kilar
        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare("Sifre", ErrorMessage = "Şifreler birbiriyle uyuşmuyor.")]
        [Display(Name = "Şifre Tekrar")]
        public string SifreTekrar { get; set; } = string.Empty;
    }
}