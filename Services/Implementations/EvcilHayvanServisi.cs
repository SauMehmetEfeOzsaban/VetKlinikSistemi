using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Data;
using VetKlinikSistemi.Models.Entities;
using VetKlinikSistemi.Services.Interfaces;

namespace VetKlinikSistemi.Services.Implementations
{
    // Evcil hayvan CRUD, dogrulama ve IDOR guvenlik kurallarinin yazildigi servis
    public class EvcilHayvanServisi : IEvcilHayvanServisi
    {
        private readonly UygulamaDbContext _db;

        public EvcilHayvanServisi(UygulamaDbContext db)
        {
            _db = db;
        }

        // Kullanicinin yetkisine gore evcil hayvanlari listeler
        public async Task<List<EvcilHayvan>> ListeleAsync(string kullaniciId, bool adminMi)
        {
            var sorgu = _db.EvcilHayvanlar
                .Include(e => e.Sahip)
                .AsQueryable();

            // Admin degilse sadece kendi sahip oldugu hayvanlari getir
            if (!adminMi)
            {
                sorgu = sorgu.Where(e => e.SahipId == kullaniciId);
            }

            return await sorgu
                .OrderBy(e => e.Ad)
                .ToListAsync();
        }

        // Id'ye gore hayvan detayini getirir, sahip kontrolu yapar (IDOR korumasi)
        public async Task<(EvcilHayvan? Hayvan, bool YetkisizMi)> DetayGetirAsync(int id, string kullaniciId, bool adminMi)
        {
            var hayvan = await _db.EvcilHayvanlar
     .Include(e => e.Sahip)
     .Include(e => e.AsiKayitlari)
     .Include(e => e.Randevular)
         .ThenInclude(r => r.Hizmet)
     .FirstOrDefaultAsync(e => e.Id == id);

            if (hayvan == null)
            {
                return (null, false);
            }

            // Admin degilse ve hayvan bu kullaniciya ait degilse erisimi engelle
            if (!adminMi && hayvan.SahipId != kullaniciId)
            {
                return (null, true);
            }

            return (hayvan, false);
        }

        // Yeni evcil hayvan ekler
        public async Task<(bool Basarili, string Mesaj)> EkleAsync(EvcilHayvan hayvan)
        {
            // Dogum tarihi bugunden ileri bir tarih olamaz
            if (hayvan.DogumTarihi.Date > DateTime.Today)
            {
                return (false, "Doğum tarihi gelecekte bir tarih olamaz.");
            }

            if (hayvan.Kilo <= 0)
            {
                return (false, "Kilo sıfırdan büyük olmalıdır.");
            }

            _db.EvcilHayvanlar.Add(hayvan);
            await _db.SaveChangesAsync();

            return (true, "Evcil hayvan başarıyla eklendi.");
        }

        // Mevcut evcil hayvani gunceller
        public async Task<(bool Basarili, string Mesaj, bool YetkisizMi)> GuncelleAsync(EvcilHayvan guncelHayvan, string kullaniciId, bool adminMi)
        {
            var mevcut = await _db.EvcilHayvanlar.FindAsync(guncelHayvan.Id);
            if (mevcut == null)
            {
                return (false, "Evcil hayvan bulunamadı.", false);
            }

            // Baskasinin hayvanini guncellemeye calisiyorsa engelle
            if (!adminMi && mevcut.SahipId != kullaniciId)
            {
                return (false, "Bu kaydı düzenleme yetkiniz yok.", true);
            }

            if (guncelHayvan.DogumTarihi.Date > DateTime.Today)
            {
                return (false, "Doğum tarihi gelecekte bir tarih olamaz.", false);
            }

            if (guncelHayvan.Kilo <= 0)
            {
                return (false, "Kilo sıfırdan büyük olmalıdır.", false);
            }

            // Sadece guncellenebilir alanlari degistir (SahipId asla disaridan degistirilmez)
            mevcut.Ad = guncelHayvan.Ad;
            mevcut.Tur = guncelHayvan.Tur;
            mevcut.Irk = guncelHayvan.Irk;
            mevcut.DogumTarihi = guncelHayvan.DogumTarihi;
            mevcut.Kilo = guncelHayvan.Kilo;
            mevcut.FotografYolu = guncelHayvan.FotografYolu;

            await _db.SaveChangesAsync();
            return (true, "Evcil hayvan bilgileri güncellendi.", false);
        }

        // Evcil hayvani siler
        public async Task<(bool Basarili, string Mesaj, bool YetkisizMi, string? SilinenFotografYolu)> SilAsync(int id, string kullaniciId, bool adminMi)
        {
            var mevcut = await _db.EvcilHayvanlar.FindAsync(id);
            if (mevcut == null)
            {
                return (false, "Evcil hayvan bulunamadı.", false, null);
            }

            // Baskasinin hayvanini silmeye calisiyorsa engelle
            if (!adminMi && mevcut.SahipId != kullaniciId)
            {
                return (false, "Bu kaydı silme yetkiniz yok.", true, null);
            }

            // Randevusu olan hayvan Restrict kurali geregi silinemez
            var randevusuVarMi = await _db.Randevular.AnyAsync(r => r.EvcilHayvanId == id);
            if (randevusuVarMi)
            {
                return (false, "Bu evcil hayvana ait randevu kayıtları olduğu için silinemez.", false, null);
            }

            var fotografYolu = mevcut.FotografYolu;
            _db.EvcilHayvanlar.Remove(mevcut);
            await _db.SaveChangesAsync();

            return (true, "Evcil hayvan kaydı silindi.", false, fotografYolu);
        }
    }
}