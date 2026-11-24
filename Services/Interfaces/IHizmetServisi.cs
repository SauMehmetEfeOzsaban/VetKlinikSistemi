using VetKlinikSistemi.Models.Entities;

namespace VetKlinikSistemi.Services.Interfaces
{
    // Hizmet islemleri icin ortak servis arayuzu
    public interface IHizmetServisi
    {
        Task<List<Hizmet>> TumHizmetleriGetirAsync(int? klinikId = null);
        Task<Hizmet?> IdIleGetirAsync(int id);
        Task<(bool Basarili, string Mesaj)> EkleAsync(Hizmet hizmet);
        Task<(bool Basarili, string Mesaj)> GuncelleAsync(Hizmet hizmet);
        Task<(bool Basarili, string Mesaj)> SilAsync(int id);
    }
}