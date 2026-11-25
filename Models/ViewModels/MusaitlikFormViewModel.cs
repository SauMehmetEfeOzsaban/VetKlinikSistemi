using System.ComponentModel.DataAnnotations;
using VetKlinikSistemi.Models.Entities;

namespace VetKlinikSistemi.Models.ViewModels
{
    // Veteriner musaitlik ekleme ve duzenleme formu icin kullanilir
    public class MusaitlikFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public int VeterinerId { get; set; }

        public string VeterinerAdSoyad { get; set; } = string.Empty;
        public string KlinikAdi { get; set; } = string.Empty;
        public TimeSpan KlinikAcilis { get; set; }
        public TimeSpan KlinikKapanis { get; set; }

        [Required(ErrorMessage = "Gün seçimi zorunludur.")]
        [Display(Name = "Gün")]
        public DayOfWeek Gun { get; set; } = DayOfWeek.Monday;

        [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan BaslangicSaati { get; set; } = new TimeSpan(9, 0, 0);

        [Required(ErrorMessage = "Bitiş saati zorunludur.")]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan BitisSaati { get; set; } = new TimeSpan(17, 0, 0);

        // Mevcut musaitlik listesini ayni sayfada gostermek icin
        public List<VeterinerMusaitlik> MevcutMusaitlikler { get; set; } = new List<VeterinerMusaitlik>();
    }
}