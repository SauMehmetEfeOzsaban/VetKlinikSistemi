using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Data;
using VetKlinikSistemi.Models.Entities;
using VetKlinikSistemi.Services.Interfaces;

namespace VetKlinikSistemi.Services.Implementations
{
    // Hizmetlerle ilgili veritabani ve is kurallari burada yonetilir
    public class HizmetServisi : IHizmetServisi
    {
        private readonly UygulamaDbContext _db;

        public HizmetServisi(UygulamaDbContext db)
        {
            _db = db;
        }

        // Tum hizmetleri (istenirse sadece belirli bir klinigin hizmetlerini) getirir
        public async Task<List<Hizmet>> TumHizmetleriGetirAsync(int? klinikId = null)
        {
            var sorgu = _db.Hizmetler
                .Include(h => h.Klinik)
                .AsQueryable();

            if (klinikId.HasValue)
            {
                sorgu = sorgu.Where(h => h.KlinikId == klinikId.Value);
            }

            return await sorgu
                .OrderBy(h => h.Klinik!.Ad)
                .ThenBy(h => h.Ad)
                .ToListAsync();
        }

        // Tek bir hizmeti bagli oldugu klinik bilgisiyle birlikte getirir
        public async Task<Hizmet?> IdIleGetirAsync(int id)
        {
            return await _db.Hizmetler
                .Include(h => h.Klinik)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        // Yeni hizmet eklerken klinik, sure ve ucret kurallarini kontrol eder
        public async Task<(bool Basarili, string Mesaj)> EkleAsync(Hizmet hizmet)
        {
            bool klinikVarMi = await _db.Klinikler.AnyAsync(k => k.Id == hizmet.KlinikId);
            if (!klinikVarMi)
            {
                return (false, "Seçilen klinik bulunamadı.");
            }

            if (hizmet.SureDakika < 5 || hizmet.SureDakika > 480)
            {
                return (false, "Hizmet süresi 5 ile 480 dakika arasında olmalıdır.");
            }

            if (hizmet.Ucret <= 0)
            {
                return (false, "Hizmet ücreti 0'dan büyük olmalıdır.");
            }

            _db.Hizmetler.Add(hizmet);
            await _db.SaveChangesAsync();
            return (true, "Hizmet başarıyla eklendi.");
        }

        // Mevcut hizmeti gunceller
        public async Task<(bool Basarili, string Mesaj)> GuncelleAsync(Hizmet hizmet)
        {
            var mevcut = await _db.Hizmetler.FindAsync(hizmet.Id);
            if (mevcut == null)
            {
                return (false, "Güncellenecek hizmet bulunamadı.");
            }

            bool klinikVarMi = await _db.Klinikler.AnyAsync(k => k.Id == hizmet.KlinikId);
            if (!klinikVarMi)
            {
                return (false, "Seçilen klinik bulunamadı.");
            }

            if (hizmet.SureDakika < 5 || hizmet.SureDakika > 480)
            {
                return (false, "Hizmet süresi 5 ile 480 dakika arasında olmalıdır.");
            }

            if (hizmet.Ucret <= 0)
            {
                return (false, "Hizmet ücreti 0'dan büyük olmalıdır.");
            }

            mevcut.KlinikId = hizmet.KlinikId;
            mevcut.Ad = hizmet.Ad;
            mevcut.Aciklama = hizmet.Aciklama;
            mevcut.SureDakika = hizmet.SureDakika;
            mevcut.Ucret = hizmet.Ucret;

            await _db.SaveChangesAsync();
            return (true, "Hizmet bilgileri güncellendi.");
        }

        // Hizmeti silmeden once bagli veteriner veya randevu var mi kontrol eder
        public async Task<(bool Basarili, string Mesaj)> SilAsync(int id)
        {
            var hizmet = await _db.Hizmetler.FindAsync(id);
            if (hizmet == null)
            {
                return (false, "Silinecek hizmet bulunamadı.");
            }

            bool veterinerBagliMi = await _db.VeterinerHizmetler.AnyAsync(vh => vh.HizmetId == id);
            bool randevuVarMi = await _db.Randevular.AnyAsync(r => r.HizmetId == id);

            if (veterinerBagliMi || randevuVarMi)
            {
                return (false, "Bu hizmete bağlı veteriner veya randevu bulunduğu için silinemez.");
            }

            _db.Hizmetler.Remove(hizmet);
            await _db.SaveChangesAsync();
            return (true, "Hizmet başarıyla silindi.");
        }
    }
}