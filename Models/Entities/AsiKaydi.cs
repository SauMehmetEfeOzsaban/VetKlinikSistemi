using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.Entities
{
    public class AsiKaydi
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Evcil Hayvan")]
        public int EvcilHayvanId { get; set; }
        public EvcilHayvan? EvcilHayvan { get; set; }

        [Required(ErrorMessage = "Aşı adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Aşı Adı")]
        public string AsiAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yapıldığı tarih zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Yapıldığı Tarih")]
        public DateTime YapildigiTarih { get; set; }

        // Yaklasan asi takibi ve MCP asi_takvimi araci bu alani kullanacak
        [DataType(DataType.Date)]
        [Display(Name = "Bir Sonraki Doz Tarihi")]
        public DateTime? SonrakiDozTarihi { get; set; }
    }
}