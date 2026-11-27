using VetKlinikSistemi.Models.Entities;

namespace VetKlinikSistemi.Services.Interfaces
{
    // Asi kayitlarinin tum is kurallarini tek yerde toplayan servis arayuzu
    // MVC, REST API ve MCP araclari ortak olarak bunu kullanacak
    public interface IAsiKaydiServisi
    {
        // Kullanicinin yetkisine gore asi kayitlarini listeler (istege bagli tek bir hayvana gore filtreler)
        Task<List<AsiKaydi>> ListeleAsync(string kullaniciId, bool tumunuGorebilirMi, int? evcilHayvanId = null);

        // Tek bir asi kaydini IDOR kontrolu yaparak getirir
        Task<(AsiKaydi? Kayit, bool YetkisizMi)> DetayGetirAsync(int id, string kullaniciId, bool tumunuGorebilirMi);

        // Belirli bir evcil hayvanin tum asilarini IDOR kontroluyle getirir (Saglik Karti ve REST API icin)
        Task<(List<AsiKaydi> Asilar, bool HayvanBulunamadi, bool YetkisizMi)> HayvaninAsilariniGetirAsync(
            int evcilHayvanId, string kullaniciId, bool tumunuGorebilirMi);

        // Hayvan adina gore uyenin kendi hayvaninin asi takvimini getirir (MCP asi_takvimi araci icin)
        Task<(EvcilHayvan? Hayvan, List<AsiKaydi> Asilar)> HayvanAdinaGoreAsiTakvimiGetirAsync(
            string kullaniciId, string hayvanAdi);

        // Belirtilen gun sayisi icinde (varsayilan 30 gun) zamani gelecek asilari listeler
        Task<List<AsiKaydi>> YaklasanAsilariGetirAsync(string kullaniciId, bool tumunuGorebilirMi, int gunSayisi = 30);

        // Yeni asi kaydi ekler (tarih ve yetki kontrolu yapar)
        Task<(bool Basarili, bool YetkisizMi, string Mesaj)> EkleAsync(
            AsiKaydi asiKaydi, string kullaniciId, bool tumunuGorebilirMi);

        // Mevcut asi kaydini gunceller (tarih ve yetki kontrolu yapar)
        Task<(bool Basarili, bool YetkisizMi, string Mesaj)> GuncelleAsync(
            AsiKaydi asiKaydi, string kullaniciId, bool tumunuGorebilirMi);

        // Asi kaydini siler (yetki kontrolu yapar)
        Task<(bool Basarili, bool YetkisizMi, string Mesaj)> SilAsync(
            int id, string kullaniciId, bool tumunuGorebilirMi);

        // Formlardaki acilir liste (dropdown) icin kullanicinin erisebilecegi hayvanlari getirir
        Task<List<EvcilHayvan>> YetkiliHayvanlariGetirAsync(string kullaniciId, bool tumunuGorebilirMi);
    }
}