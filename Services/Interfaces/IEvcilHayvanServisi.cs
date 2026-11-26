using VetKlinikSistemi.Models.Entities;

namespace VetKlinikSistemi.Services.Interfaces
{
    // Evcil hayvan islemlerini MVC, REST API ve MCP icin tek yerden yoneten servis arayuzu
    public interface IEvcilHayvanServisi
    {
        // Uye sadece kendi hayvanlarini, Admin tum hayvanlari gorur
        Task<List<EvcilHayvan>> ListeleAsync(string kullaniciId, bool adminMi);

        // Tek hayvan getirir; baskasinin hayvanina erisim denenirse YetkisizMi true doner
        Task<(EvcilHayvan? Hayvan, bool YetkisizMi)> DetayGetirAsync(int id, string kullaniciId, bool adminMi);

        // Yeni evcil hayvan ekler
        Task<(bool Basarili, string Mesaj)> EkleAsync(EvcilHayvan hayvan);

        // Mevcut evcil hayvani gunceller (IDOR ve tarih/kilo kontrollu)
        Task<(bool Basarili, string Mesaj, bool YetkisizMi)> GuncelleAsync(EvcilHayvan guncelHayvan, string kullaniciId, bool adminMi);

        // Evcil hayvani siler (randevusu varsa silmez, varsa eski fotograf yolunu geri doner)
        Task<(bool Basarili, string Mesaj, bool YetkisizMi, string? SilinenFotografYolu)> SilAsync(int id, string kullaniciId, bool adminMi);
    }
}