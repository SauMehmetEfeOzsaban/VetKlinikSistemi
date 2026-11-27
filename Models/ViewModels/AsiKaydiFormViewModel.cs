using System.ComponentModel.DataAnnotations;

namespace VetKlinikSistemi.Models.ViewModels
{
    // Asi kaydi ekleme ve duzenleme formlarinda kullanilan ViewModel
    public class AsiKaydiFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Evcil hayvan seçimi zorunludur.")]
        [Display(Name = "Evcil Hayvan")]
        public int EvcilHayvanId { get; set; }

        [Required(ErrorMessage = "Aşı adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Aşı adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Aşı Adı")]
        public string AsiAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Aşının yapıldığı tarih zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Yapıldığı Tarih")]
        public DateTime YapildigiTarih { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Bir Sonraki Doz Tarihi (Opsiyonel)")]
        public DateTime? SonrakiDozTarihi { get; set; }
    }
}