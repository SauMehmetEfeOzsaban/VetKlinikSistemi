using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.Entities
{
    public class EvcilHayvan
    {
        public int Id { get; set; }

        // Hayvanin sahibi olan uyenin Id bilgisi (IDOR kontrolunde kullanilacak)
        [Required]
        public string SahipId { get; set; } = string.Empty;
        public Kullanici? Sahip { get; set; }

        [Required(ErrorMessage = "Evcil hayvan adı zorunludur.")]
        [StringLength(60)]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tür zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Tür")]
        public string Tur { get; set; } = string.Empty;

        [Required(ErrorMessage = "Irk zorunludur.")]
        [StringLength(60)]
        [Display(Name = "Irk")]
        public string Irk { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime DogumTarihi { get; set; }

        [Required(ErrorMessage = "Kilo zorunludur.")]
        [Range(0.05, 250, ErrorMessage = "Kilo 0.05 ile 250 kg arasında olmalıdır.")]
        [Display(Name = "Kilo (kg)")]
        public decimal Kilo { get; set; }

        // Istege bagli fotograf dosya yolu
        [StringLength(300)]
        [Display(Name = "Fotoğraf")]
        public string? FotografYolu { get; set; }

        public ICollection<AsiKaydi> AsiKayitlari { get; set; } = new List<AsiKaydi>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}