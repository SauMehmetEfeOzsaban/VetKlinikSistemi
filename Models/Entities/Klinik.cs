using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.Entities
{
    public class Klinik
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Klinik adı zorunludur.")]
        [StringLength(120)]
        [Display(Name = "Klinik Adı")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adres zorunludur.")]
        [StringLength(250)]
        [Display(Name = "Adres")]
        public string Adres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(20)]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Çalışma günleri zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Çalışma Günleri")]
        public string CalismaGunleri { get; set; } = "Pazartesi - Cumartesi";

        [Required]
        [Display(Name = "Açılış Saati")]
        public TimeSpan AcilisSaati { get; set; } = new TimeSpan(9, 0, 0);

        [Required]
        [Display(Name = "Kapanış Saati")]
        public TimeSpan KapanisSaati { get; set; } = new TimeSpan(18, 0, 0);

        // Klinigin sundugu hizmetler, calisan veterinerler ve alinan randevular
        public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
        public ICollection<Veteriner> Veterinerler { get; set; } = new List<Veteriner>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}