using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VetKlinikSistemi.Models.ViewModels
{
    // Evcil hayvan ekleme ve duzenleme formlarinda kullanilan model
    public class EvcilHayvanFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Evcil hayvan adı zorunludur.")]
        [StringLength(60, ErrorMessage = "Ad en fazla 60 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tür zorunludur.")]
        [StringLength(50, ErrorMessage = "Tür en fazla 50 karakter olabilir.")]
        [Display(Name = "Tür")]
        public string Tur { get; set; } = string.Empty;

        [Required(ErrorMessage = "Irk zorunludur.")]
        [StringLength(60, ErrorMessage = "Irk en fazla 60 karakter olabilir.")]
        [Display(Name = "Irk")]
        public string Irk { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime DogumTarihi { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Kilo zorunludur.")]
        [Range(0.05, 250, ErrorMessage = "Kilo 0.05 ile 250 kg arasında olmalıdır.")]
        [Display(Name = "Kilo (kg)")]
        public decimal Kilo { get; set; }

        // Kullanicinin formdan sececegi istege bagli fotograf dosyasi
        [Display(Name = "Fotoğraf (İsteğe Bağlı)")]
        public IFormFile? FotografDosyasi { get; set; }

        // Duzenleme ekraninda mevcut fotografi gostermek icin tutulur
        public string? MevcutFotografYolu { get; set; }
    }
}