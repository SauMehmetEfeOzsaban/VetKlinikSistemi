using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.Entities
{
    public class Hizmet
    {
        public int Id { get; set; }

        // Hizmetin hangi klinige ait oldugunu tutar
        [Required]
        [Display(Name = "Klinik")]
        public int KlinikId { get; set; }
        public Klinik? Klinik { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Hizmet Adı")]
        public string Ad { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        // Randevu cakisma hesabinda bu sure kullanilacak
        [Required(ErrorMessage = "Hizmet süresi zorunludur.")]
        [Range(5, 480, ErrorMessage = "Süre 5 ile 480 dakika arasında olmalıdır.")]
        [Display(Name = "Süre (Dakika)")]
        public int SureDakika { get; set; }

        [Required(ErrorMessage = "Ücret zorunludur.")]
        [Range(0.01, 100000, ErrorMessage = "Ücret 0'dan büyük olmalıdır.")]
        [Display(Name = "Ücret (TL)")]
        public decimal Ucret { get; set; }

        public ICollection<VeterinerHizmet> VeterinerHizmetler { get; set; } = new List<VeterinerHizmet>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}