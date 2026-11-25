using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.ViewModels
{
    // Mevcut veteriner bilgilerini, uzmanliklarini ve verdigi hizmetleri guncellemek icin kullanilir
    public class VeterinerDuzenleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Klinik seçimi zorunludur.")]
        [Display(Name = "Görev Yaptığı Klinik")]
        public int KlinikId { get; set; }

        [StringLength(100)]
        [Display(Name = "Unvan")]
        public string? Unvan { get; set; }

        [StringLength(1000)]
        [Display(Name = "Biyografi")]
        public string? Biyografi { get; set; }

        public List<int> SeciliUzmanlikIdleri { get; set; } = new List<int>();
        public List<int> SeciliHizmetIdleri { get; set; } = new List<int>();
    }
}