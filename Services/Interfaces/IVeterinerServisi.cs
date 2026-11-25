using VetKlinikSistemi.Models.Entities;
using VetKlinikSistemi.Models.ViewModels;

namespace VetKlinikSistemi.Services.Interfaces
{
    // Veteriner ve musaitlik islemlerini MVC, REST API ve MCP ortak kullanacak
    public interface IVeterinerServisi
    {
        Task<List<Veteriner>> TumVeterinerleriGetirAsync(int? klinikId = null);
        Task<Veteriner?> IdIleGetirAsync(int id);
        Task<List<UzmanlikAlani>> TumUzmanliklariGetirAsync();
        Task<(bool Basarili, string Mesaj)> EkleAsync(VeterinerEkleViewModel model);
        Task<(bool Basarili, string Mesaj)> GuncelleAsync(VeterinerDuzenleViewModel model);
        Task<(bool Basarili, string Mesaj)> SilAsync(int id);

        // Musaitlik saatleri CRUD metodlari
        Task<List<VeterinerMusaitlik>> MusaitlikleriGetirAsync(int veterinerId);
        Task<VeterinerMusaitlik?> MusaitlikIdIleGetirAsync(int id);
        Task<(bool Basarili, string Mesaj)> MusaitlikEkleAsync(VeterinerMusaitlik musaitlik);
        Task<(bool Basarili, string Mesaj)> MusaitlikGuncelleAsync(VeterinerMusaitlik musaitlik);
        Task<(bool Basarili, string Mesaj)> MusaitlikSilAsync(int id);
    }
}