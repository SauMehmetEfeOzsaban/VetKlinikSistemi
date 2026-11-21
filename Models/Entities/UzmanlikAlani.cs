using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.Entities
{
    // Kucuk hayvan, cerrahi, dermatoloji gibi uzmanlik alanlari
    public class UzmanlikAlani
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Uzmanlık alanı adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Uzmanlık Alanı")]
        public string Ad { get; set; } = string.Empty;

        public ICollection<VeterinerUzmanlik> VeterinerUzmanliklar { get; set; } = new List<VeterinerUzmanlik>();
    }
}