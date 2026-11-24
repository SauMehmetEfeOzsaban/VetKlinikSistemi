namespace VetKlinikSistemi.Models.ViewModels
{
    // Admin ana sayfasindaki ozet sayilari ekrana tasimak icin kullanilir
    public class AdminPanelViewModel
    {
        public int ToplamKlinik { get; set; }
        public int ToplamHizmet { get; set; }
        public int ToplamVeteriner { get; set; }
        public int ToplamUye { get; set; }
        public int BekleyenRandevu { get; set; }
        public int ToplamRandevu { get; set; }
    }
}