using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Data;
using VetKlinikSistemi.Models.Entities;
using VetKlinikSistemi.Services.Interfaces;

namespace VetKlinikSistemi.Services.Implementations
{
    // Asi kayitlariyla ilgili veritabani ve yetki islemlerini yapan servis sinifi
    public class AsiKaydiServisi : IAsiKaydiServisi
    {
        private readonly UygulamaDbContext _context;

        public AsiKaydiServisi(UygulamaDbContext context)
        {
            _context = context;
        }

        // Asi kayitlarini listeler; Uye sadece kendi hayvanlarinin asilarini gorur
        public async Task<List<AsiKaydi>> ListeleAsync(string kullaniciId, bool tumunuGorebilirMi, int? evcilHayvanId = null)
        {
            var sorgu = _context.AsiKayitlari
                .Include(a => a.EvcilHayvan)
                .ThenInclude(e => e!.Sahip)
                .AsQueryable();

            // Admin veya Veteriner degilse sadece kendi hayvanlarinin kayitlarini getir
            if (!tumunuGorebilirMi)
            {
                sorgu = sorgu.Where(a => a.EvcilHayvan != null && a.EvcilHayvan.SahipId == kullaniciId);
            }

            // Belirli bir hayvan secildiyse sadece onun asilarini filtrele
            if (evcilHayvanId.HasValue)
            {
                sorgu = sorgu.Where(a => a.EvcilHayvanId == evcilHayvanId.Value);
            }

            return await sorgu
                .OrderByDescending(a => a.YapildigiTarih)
                .ToListAsync();
        }

        // Tek bir asi kaydini getirir; baskasinin hayvanina aitse yetkisiz doner
        public async Task<(AsiKaydi? Kayit, bool YetkisizMi)> DetayGetirAsync(
            int id, string kullaniciId, bool tumunuGorebilirMi)
        {
            var kayit = await _context.AsiKayitlari
                .Include(a => a.EvcilHayvan)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (kayit == null)
            {
                return (null, false);
            }

            // IDOR kontrolu: Uye baskasinin hayvaninin asisina erisemez
            if (!tumunuGorebilirMi && kayit.EvcilHayvan?.SahipId != kullaniciId)
            {
                return (null, true);
            }

            return (kayit, false);
        }

        // Bir hayvanin tum asilarini getirir (Saglik Karti ve REST API kullanacak)
        public async Task<(List<AsiKaydi> Asilar, bool HayvanBulunamadi, bool YetkisizMi)> HayvaninAsilariniGetirAsync(
            int evcilHayvanId, string kullaniciId, bool tumunuGorebilirMi)
        {
            var hayvan = await _context.EvcilHayvanlar.FindAsync(evcilHayvanId);
            if (hayvan == null)
            {
                return (new List<AsiKaydi>(), true, false);
            }

            // IDOR kontrolu
            if (!tumunuGorebilirMi && hayvan.SahipId != kullaniciId)
            {
                return (new List<AsiKaydi>(), false, true);
            }

            var asilar = await _context.AsiKayitlari
                .Where(a => a.EvcilHayvanId == evcilHayvanId)
                .OrderByDescending(a => a.YapildigiTarih)
                .ToListAsync();

            return (asilar, false, false);
        }

        // Hayvan adina gore sadece oturumdaki uyenin hayvanini bulup asi takvimini getirir (MCP icin)
        public async Task<(EvcilHayvan? Hayvan, List<AsiKaydi> Asilar)> HayvanAdinaGoreAsiTakvimiGetirAsync(
            string kullaniciId, string hayvanAdi)
        {
            var arananAd = hayvanAdi.Trim().ToLower();

            var hayvan = await _context.EvcilHayvanlar
                .FirstOrDefaultAsync(e => e.SahipId == kullaniciId && e.Ad.ToLower() == arananAd);

            if (hayvan == null)
            {
                return (null, new List<AsiKaydi>());
            }

            var asilar = await _context.AsiKayitlari
                .Where(a => a.EvcilHayvanId == hayvan.Id)
                .OrderByDescending(a => a.YapildigiTarih)
                .ToListAsync();

            return (hayvan, asilar);
        }

        // Bugunden itibaren belirli gun icinde (varsayilan 30 gun) yapilmasi gereken asilari getirir
        public async Task<List<AsiKaydi>> YaklasanAsilariGetirAsync(
            string kullaniciId, bool tumunuGorebilirMi, int gunSayisi = 30)
        {
            var bugun = DateTime.Today;
            var sinirTarih = bugun.AddDays(gunSayisi);

            var sorgu = _context.AsiKayitlari
                .Include(a => a.EvcilHayvan)
                .Where(a => a.SonrakiDozTarihi.HasValue
                            && a.SonrakiDozTarihi.Value.Date >= bugun
                            && a.SonrakiDozTarihi.Value.Date <= sinirTarih);

            if (!tumunuGorebilirMi)
            {
                sorgu = sorgu.Where(a => a.EvcilHayvan != null && a.EvcilHayvan.SahipId == kullaniciId);
            }

            return await sorgu
                .OrderBy(a => a.SonrakiDozTarihi)
                .ToListAsync();
        }

        // Yeni asi kaydi ekler
        public async Task<(bool Basarili, bool YetkisizMi, string Mesaj)> EkleAsync(
            AsiKaydi asiKaydi, string kullaniciId, bool tumunuGorebilirMi)
        {
            var hayvan = await _context.EvcilHayvanlar.FindAsync(asiKaydi.EvcilHayvanId);
            if (hayvan == null)
            {
                return (false, false, "Seçilen evcil hayvan bulunamadı.");
            }

            // IDOR kontrolu: Uye sadece kendi hayvanina asi ekleyebilir
            if (!tumunuGorebilirMi && hayvan.SahipId != kullaniciId)
            {
                return (false, true, "Başka bir üyeye ait hayvana aşı kaydı ekleyemezsiniz.");
            }

            // Tarih kurali: Sonraki doz tarihi, yapildigi tarihten once olamaz
            if (asiKaydi.SonrakiDozTarihi.HasValue && asiKaydi.SonrakiDozTarihi.Value.Date < asiKaydi.YapildigiTarih.Date)
            {
                return (false, false, "Bir sonraki doz tarihi, aşının yapıldığı tarihten önce olamaz.");
            }

            _context.AsiKayitlari.Add(asiKaydi);
            await _context.SaveChangesAsync();

            return (true, false, "Aşı kaydı başarıyla eklendi.");
        }

        // Mevcut asi kaydini gunceller
        public async Task<(bool Basarili, bool YetkisizMi, string Mesaj)> GuncelleAsync(
            AsiKaydi asiKaydi, string kullaniciId, bool tumunuGorebilirMi)
        {
            var mevcut = await _context.AsiKayitlari
                .Include(a => a.EvcilHayvan)
                .FirstOrDefaultAsync(a => a.Id == asiKaydi.Id);

            if (mevcut == null)
            {
                return (false, false, "Güncellenecek aşı kaydı bulunamadı.");
            }

            // Mevcut kaydin sahibini kontrol et (IDOR)
            if (!tumunuGorebilirMi && mevcut.EvcilHayvan?.SahipId != kullaniciId)
            {
                return (false, true, "Bu aşı kaydını güncelleme yetkiniz yok.");
            }

            // Secilen yeni hayvanin varligini ve sahibini kontrol et (IDOR)
            var hedefHayvan = await _context.EvcilHayvanlar.FindAsync(asiKaydi.EvcilHayvanId);
            if (hedefHayvan == null)
            {
                return (false, false, "Seçilen evcil hayvan bulunamadı.");
            }

            if (!tumunuGorebilirMi && hedefHayvan.SahipId != kullaniciId)
            {
                return (false, true, "Başka bir üyeye ait hayvana kayıt taşıyamazsınız.");
            }

            // Tarih kurali kontrolu
            if (asiKaydi.SonrakiDozTarihi.HasValue && asiKaydi.SonrakiDozTarihi.Value.Date < asiKaydi.YapildigiTarih.Date)
            {
                return (false, false, "Bir sonraki doz tarihi, aşının yapıldığı tarihten önce olamaz.");
            }

            mevcut.EvcilHayvanId = asiKaydi.EvcilHayvanId;
            mevcut.AsiAdi = asiKaydi.AsiAdi.Trim();
            mevcut.YapildigiTarih = asiKaydi.YapildigiTarih;
            mevcut.SonrakiDozTarihi = asiKaydi.SonrakiDozTarihi;

            await _context.SaveChangesAsync();

            return (true, false, "Aşı kaydı başarıyla güncellendi.");
        }

        // Asi kaydini siler
        public async Task<(bool Basarili, bool YetkisizMi, string Mesaj)> SilAsync(
            int id, string kullaniciId, bool tumunuGorebilirMi)
        {
            var mevcut = await _context.AsiKayitlari
                .Include(a => a.EvcilHayvan)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (mevcut == null)
            {
                return (false, false, "Silinecek aşı kaydı bulunamadı.");
            }

            // IDOR kontrolu
            if (!tumunuGorebilirMi && mevcut.EvcilHayvan?.SahipId != kullaniciId)
            {
                return (false, true, "Bu aşı kaydını silme yetkiniz yok.");
            }

            _context.AsiKayitlari.Remove(mevcut);
            await _context.SaveChangesAsync();

            return (true, false, "Aşı kaydı başarıyla silindi.");
        }

        // Formlardaki hayvan secim listesi icin yetkili olunan hayvanlari getirir
        public async Task<List<EvcilHayvan>> YetkiliHayvanlariGetirAsync(string kullaniciId, bool tumunuGorebilirMi)
        {
            var sorgu = _context.EvcilHayvanlar.AsQueryable();

            if (!tumunuGorebilirMi)
            {
                sorgu = sorgu.Where(e => e.SahipId == kullaniciId);
            }

            return await sorgu
                .OrderBy(e => e.Ad)
                .ToListAsync();
        }
    }
}