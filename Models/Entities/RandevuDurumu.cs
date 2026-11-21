namespace VetKlinikSistemi.Models.Entities
{
    // Randevu surecindeki olasi durumlar
    public enum RandevuDurumu
    {
        Bekliyor = 0,
        Onaylandi = 1,
        Reddedildi = 2,
        Tamamlandi = 3,
        IptalEdildi = 4
    }
}