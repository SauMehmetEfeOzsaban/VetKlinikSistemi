using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.Entities
{
    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Evcil Hayvan")]
        public int EvcilHayvanId { get; set; }
        public EvcilHayvan? EvcilHayvan { get; set; }

        [Required]
        [Display(Name = "Veteriner Hekim")]
        public int VeterinerId { get; set; }
        public Veteriner? Veteriner { get; set; }

        [Required]
        [Display(Name = "Hizmet")]
        public int HizmetId { get; set; }
        public Hizmet? Hizmet { get; set; }

        [Required]
        [Display(Name = "Klinik")]
        public int KlinikId { get; set; }
        public Klinik? Klinik { get; set; }

        [Required]
        [Display(Name = "Başlangıç Tarih/Saati")]
        public DateTime BaslangicTarihSaati { get; set; }

        // Baslangic + Hizmet suresi olarak hesaplanip kaydedilir
        [Required]
        [Display(Name = "Bitiş Tarih/Saati")]
        public DateTime BitisTarihSaati { get; set; }

        // Hizmet fiyati veya suresi sonradan degisse bile randevu anindaki deger korunur
        [Required]
        [Display(Name = "Hizmet Süresi (dk)")]
        public int SureDakika { get; set; }

        [Required]
        [Display(Name = "Ücret (TL)")]
        public decimal Ucret { get; set; }

        [Required]
        [Display(Name = "Durum")]
        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Bekliyor;

        // Tamamlanan randevular icin veteriner hekim tarafindan doldurulur
        [StringLength(2000)]
        [Display(Name = "Muayene Notu")]
        public string? MuayeneNotu { get; set; }

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
    }
}