using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Data;
using VetKlinikSistemi.Models.Entities;
using VetKlinikSistemi.Services.Interfaces;

namespace VetKlinikSistemi.Services.Implementations
{
    // Kliniklerle ilgili tum veritabani ve is kurallari burada toplanir
    public class KlinikServisi : IKlinikServisi
    {
        private readonly UygulamaDbContext _db;

        public KlinikServisi(UygulamaDbContext db)
        {
            _db = db;
        }

        // Tum klinikleri isme gore sirali getirir
        public async Task<List<Klinik>> TumKlinikleriGetirAsync()
        {
            return await _db.Klinikler
                .OrderBy(k => k.Ad)
                .ToListAsync();
        }

        // Tek bir klinigi Id ile bulur
        public async Task<Klinik?> IdIleGetirAsync(int id)
        {
            return await _db.Klinikler.FindAsync(id);
        }

        // Yeni klinik eklerken acilis-kapanis saati kuralini kontrol eder
        public async Task<(bool Basarili, string Mesaj)> EkleAsync(Klinik klinik)
        {
            if (klinik.AcilisSaati >= klinik.KapanisSaati)
            {
                return (false, "Açılış saati, kapanış saatinden önce olmalıdır.");
            }

            _db.Klinikler.Add(klinik);
            await _db.SaveChangesAsync();
            return (true, "Klinik başarıyla eklendi.");
        }

        // Mevcut klinigi gunceller
        public async Task<(bool Basarili, string Mesaj)> GuncelleAsync(Klinik klinik)
        {
            if (klinik.AcilisSaati >= klinik.KapanisSaati)
            {
                return (false, "Açılış saati, kapanış saatinden önce olmalıdır.");
            }

            var mevcut = await _db.Klinikler.FindAsync(klinik.Id);
            if (mevcut == null)
            {
                return (false, "Güncellenecek klinik bulunamadı.");
            }

            mevcut.Ad = klinik.Ad;
            mevcut.Adres = klinik.Adres;
            mevcut.Telefon = klinik.Telefon;
            mevcut.CalismaGunleri = klinik.CalismaGunleri;
            mevcut.AcilisSaati = klinik.AcilisSaati;
            mevcut.KapanisSaati = klinik.KapanisSaati;

            await _db.SaveChangesAsync();
            return (true, "Klinik bilgileri güncellendi.");
        }

        // Klinigi silmeden once bagli kayit var mi diye bakar (Restrict hatasini onlemek icin)
        public async Task<(bool Basarili, string Mesaj)> SilAsync(int id)
        {
            var klinik = await _db.Klinikler.FindAsync(id);
            if (klinik == null)
            {
                return (false, "Silinecek klinik bulunamadı.");
            }

            bool hizmetVarMi = await _db.Hizmetler.AnyAsync(h => h.KlinikId == id);
            bool veterinerVarMi = await _db.Veterinerler.AnyAsync(v => v.KlinikId == id);
            bool randevuVarMi = await _db.Randevular.AnyAsync(r => r.KlinikId == id);

            if (hizmetVarMi || veterinerVarMi || randevuVarMi)
            {
                return (false, "Bu kliniğe bağlı hizmet, veteriner veya randevu bulunduğu için silinemez.");
            }

            _db.Klinikler.Remove(klinik);
            await _db.SaveChangesAsync();
            return (true, "Klinik başarıyla silindi.");
        }
    }
}