using VetKlinikSistemi.Models.Entities;

namespace VetKlinikSistemi.Services.Interfaces
{
    // Klinik islemlerini MVC, API ve MCP'nin ortak kullanabilmesi icin arayuz tanimliyoruz
    public interface IKlinikServisi
    {
        Task<List<Klinik>> TumKlinikleriGetirAsync();
        Task<Klinik?> IdIleGetirAsync(int id);
        Task<(bool Basarili, string Mesaj)> EkleAsync(Klinik klinik);
        Task<(bool Basarili, string Mesaj)> GuncelleAsync(Klinik klinik);
        Task<(bool Basarili, string Mesaj)> SilAsync(int id);
    }
}