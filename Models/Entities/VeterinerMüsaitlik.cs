using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.Entities
{
    // Veterinerin haftanin hangi gunu hangi saatler arasinda calistigini tutar
    public class VeterinerMusaitlik
    {
        public int Id { get; set; }

        [Required]
        public int VeterinerId { get; set; }
        public Veteriner? Veteriner { get; set; }

        [Required]
        [Display(Name = "Gün")]
        public DayOfWeek Gun { get; set; }

        [Required]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan BaslangicSaati { get; set; }

        [Required]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan BitisSaati { get; set; }
    }
}