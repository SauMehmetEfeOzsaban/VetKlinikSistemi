using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.Entities
{
    public class Veteriner
    {
        public int Id { get; set; }

        // Veterinerin sisteme giris yaptigi Identity kullanici hesabi
        [Required]
        public string KullaniciId { get; set; } = string.Empty;
        public Kullanici? Kullanici { get; set; }

        // Veterinerin gorev yaptigi klinik
        [Required]
        [Display(Name = "Klinik")]
        public int KlinikId { get; set; }
        public Klinik? Klinik { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Unvan")]
        public string? Unvan { get; set; }

        [StringLength(1000)]
        [Display(Name = "Biyografi")]
        public string? Biyografi { get; set; }

        // Coktan-coga iliskiler ve haftalik musaitlik saatleri
        public ICollection<VeterinerUzmanlik> VeterinerUzmanliklar { get; set; } = new List<VeterinerUzmanlik>();
        public ICollection<VeterinerHizmet> VeterinerHizmetler { get; set; } = new List<VeterinerHizmet>();
        public ICollection<VeterinerMusaitlik> Musaitlikler { get; set; } = new List<VeterinerMusaitlik>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}