using VetKlinikSistemi.Models.Entities;

namespace VetKlinikSistemi.Models.ViewModels
{
    // Ana sayfada ziyaretcilere klinikleri, hizmetleri ve veterinerleri gostermek icin kullanilir
    public class VitrinViewModel
    {
        public List<Klinik> Klinikler { get; set; } = new List<Klinik>();
        public List<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
        public List<Veteriner> Veterinerler { get; set; } = new List<Veteriner>();
    }
}