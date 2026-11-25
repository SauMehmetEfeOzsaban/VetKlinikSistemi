using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Data;
using VetKlinikSistemi.Models.Entities;
using VetKlinikSistemi.Models.ViewModels;
using VetKlinikSistemi.Services.Interfaces;

namespace VetKlinikSistemi.Services.Implementations
{
    // Veteriner hekim, uzmanlik, hizmet eslestirme ve musaitlik kurallari burada yonetilir
    public class VeterinerServisi : IVeterinerServisi
    {
        private readonly UygulamaDbContext _db;
        private readonly UserManager<Kullanici> _userManager;

        public VeterinerServisi(UygulamaDbContext db, UserManager<Kullanici> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Tum veterinerleri klinik, uzmanlik, hizmet ve musaitlik bilgileriyle birlikte getirir
        public async Task<List<Veteriner>> TumVeterinerleriGetirAsync(int? klinikId = null)
        {
            var sorgu = _db.Veterinerler
                .Include(v => v.Klinik)
                .Include(v => v.Kullanici)
                .Include(v => v.VeterinerUzmanliklar)
                    .ThenInclude(vu => vu.UzmanlikAlani)
                .Include(v => v.VeterinerHizmetler)
                    .ThenInclude(vh => vh.Hizmet)
                .Include(v => v.Musaitlikler)
                .AsQueryable();

            if (klinikId.HasValue)
            {
                sorgu = sorgu.Where(v => v.KlinikId == klinikId.Value);
            }

            return await sorgu
                .OrderBy(v => v.AdSoyad)
                .ToListAsync();
        }

        // Tek bir veterineri tum detaylariyla getirir
        public async Task<Veteriner?> IdIleGetirAsync(int id)
        {
            return await _db.Veterinerler
                .Include(v => v.Klinik)
                .Include(v => v.Kullanici)
                .Include(v => v.VeterinerUzmanliklar)
                    .ThenInclude(vu => vu.UzmanlikAlani)
                .Include(v => v.VeterinerHizmetler)
                    .ThenInclude(vh => vh.Hizmet)
                .Include(v => v.Musaitlikler)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        // Veritabanindaki tum uzmanlik alanlarini sirali getirir
        public async Task<List<UzmanlikAlani>> TumUzmanliklariGetirAsync()
        {
            return await _db.UzmanlikAlanlari
                .OrderBy(u => u.Ad)
                .ToListAsync();
        }

        // Yeni veteriner icin once Identity hesabi acar, Veteriner rolu verir, sonra profili kaydeder
        public async Task<(bool Basarili, string Mesaj)> EkleAsync(VeterinerEkleViewModel model)
        {
            bool klinikVarMi = await _db.Klinikler.AnyAsync(k => k.Id == model.KlinikId);
            if (!klinikVarMi)
            {
                return (false, "Seçilen klinik bulunamadı.");
            }

            var mevcutKullanici = await _userManager.FindByEmailAsync(model.Eposta);
            if (mevcutKullanici != null)
            {
                return (false, "Bu e-posta adresiyle kayıtlı bir kullanıcı zaten var.");
            }

            // Veterinerin sisteme giris yapacagi kullanici hesabini olusturuyoruz
            var yeniKullanici = new Kullanici
            {
                UserName = model.Eposta,
                Email = model.Eposta,
                Ad = model.Ad.Trim(),
                Soyad = model.Soyad.Trim(),
                EmailConfirmed = true
            };

            var kullaniciSonucu = await _userManager.CreateAsync(yeniKullanici, model.Sifre);
            if (!kullaniciSonucu.Succeeded)
            {
                string hatalar = string.Join(" ", kullaniciSonucu.Errors.Select(e => e.Description));
                return (false, "Kullanıcı hesabı oluşturulamadı: " + hatalar);
            }

            await _userManager.AddToRoleAsync(yeniKullanici, "Veteriner");

            // Veteriner profil kaydini olusturuyoruz
            var veteriner = new Veteriner
            {
                KullaniciId = yeniKullanici.Id,
                KlinikId = model.KlinikId,
                AdSoyad = $"{model.Ad.Trim()} {model.Soyad.Trim()}",
                Unvan = model.Unvan?.Trim(),
                Biyografi = model.Biyografi?.Trim()
            };

            _db.Veterinerler.Add(veteriner);
            await _db.SaveChangesAsync();

            // Secilen uzmanlik alanlarini ara tabloya ekliyoruz
            if (model.SeciliUzmanlikIdleri.Any())
            {
                foreach (var uzmanlikId in model.SeciliUzmanlikIdleri.Distinct())
                {
                    _db.VeterinerUzmanliklar.Add(new VeterinerUzmanlik
                    {
                        VeterinerId = veteriner.Id,
                        UzmanlikAlaniId = uzmanlikId
                    });
                }
            }

            // Sadece veterinerin gorev yaptigi klinige ait secili hizmetleri ara tabloya ekliyoruz
            if (model.SeciliHizmetIdleri.Any())
            {
                var gecerliHizmetIdleri = await _db.Hizmetler
                    .Where(h => h.KlinikId == model.KlinikId && model.SeciliHizmetIdleri.Contains(h.Id))
                    .Select(h => h.Id)
                    .ToListAsync();

                foreach (var hizmetId in gecerliHizmetIdleri)
                {
                    _db.VeterinerHizmetler.Add(new VeterinerHizmet
                    {
                        VeterinerId = veteriner.Id,
                        HizmetId = hizmetId
                    });
                }
            }

            await _db.SaveChangesAsync();
            return (true, "Veteriner hekim ve giriş hesabı başarıyla oluşturuldu.");
        }

        // Veteriner bilgilerini ve coktan-coga uzmanlik/hizmet eslestirmelerini gunceller
        public async Task<(bool Basarili, string Mesaj)> GuncelleAsync(VeterinerDuzenleViewModel model)
        {
            var veteriner = await _db.Veterinerler
                .Include(v => v.VeterinerUzmanliklar)
                .Include(v => v.VeterinerHizmetler)
                .FirstOrDefaultAsync(v => v.Id == model.Id);

            if (veteriner == null)
            {
                return (false, "Güncellenecek veteriner bulunamadı.");
            }

            bool klinikVarMi = await _db.Klinikler.AnyAsync(k => k.Id == model.KlinikId);
            if (!klinikVarMi)
            {
                return (false, "Seçilen klinik bulunamadı.");
            }

            veteriner.AdSoyad = model.AdSoyad.Trim();
            veteriner.KlinikId = model.KlinikId;
            veteriner.Unvan = model.Unvan?.Trim();
            veteriner.Biyografi = model.Biyografi?.Trim();

            // Eski uzmanlik eslestirmelerini silip yenilerini ekliyoruz
            _db.VeterinerUzmanliklar.RemoveRange(veteriner.VeterinerUzmanliklar);
            foreach (var uzmanlikId in model.SeciliUzmanlikIdleri.Distinct())
            {
                _db.VeterinerUzmanliklar.Add(new VeterinerUzmanlik
                {
                    VeterinerId = veteriner.Id,
                    UzmanlikAlaniId = uzmanlikId
                });
            }

            // Eski hizmet eslestirmelerini silip sadece secili klinige ait olanlari ekliyoruz
            _db.VeterinerHizmetler.RemoveRange(veteriner.VeterinerHizmetler);
            if (model.SeciliHizmetIdleri.Any())
            {
                var gecerliHizmetIdleri = await _db.Hizmetler
                    .Where(h => h.KlinikId == model.KlinikId && model.SeciliHizmetIdleri.Contains(h.Id))
                    .Select(h => h.Id)
                    .ToListAsync();

                foreach (var hizmetId in gecerliHizmetIdleri)
                {
                    _db.VeterinerHizmetler.Add(new VeterinerHizmet
                    {
                        VeterinerId = veteriner.Id,
                        HizmetId = hizmetId
                    });
                }
            }

            await _db.SaveChangesAsync();
            return (true, "Veteriner bilgileri güncellendi.");
        }

        // Veterineri silmeden once randevusu var mi kontrol eder; yoksa ara tablolari ve hesabi temizler
        public async Task<(bool Basarili, string Mesaj)> SilAsync(int id)
        {
            var veteriner = await _db.Veterinerler
                .Include(v => v.VeterinerUzmanliklar)
                .Include(v => v.VeterinerHizmetler)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veteriner == null)
            {
                return (false, "Silinecek veteriner bulunamadı.");
            }

            bool randevuVarMi = await _db.Randevular.AnyAsync(r => r.VeterinerId == id);
            if (randevuVarMi)
            {
                return (false, "Bu veterinere ait randevu kayıtları bulunduğu için silinemez.");
            }

            string kullaniciId = veteriner.KullaniciId;

            // Ara tablolar Restrict oldugu icin once onlari kaldiriyoruz
            _db.VeterinerUzmanliklar.RemoveRange(veteriner.VeterinerUzmanliklar);
            _db.VeterinerHizmetler.RemoveRange(veteriner.VeterinerHizmetler);
            _db.Veterinerler.Remove(veteriner);
            await _db.SaveChangesAsync();

            // Bagli Identity hesabini da temizliyoruz
            var kullanici = await _userManager.FindByIdAsync(kullaniciId);
            if (kullanici != null)
            {
                await _userManager.DeleteAsync(kullanici);
            }

            return (true, "Veteriner hekim başarıyla silindi.");
        }

        // Bir veterinerin haftalik musaitlik saatlerini gun ve saate gore sirali getirir
        public async Task<List<VeterinerMusaitlik>> MusaitlikleriGetirAsync(int veterinerId)
        {
            return await _db.VeterinerMusaitlikler
                .Where(m => m.VeterinerId == veterinerId)
                .OrderBy(m => m.Gun == DayOfWeek.Sunday ? 7 : (int)m.Gun)
                .ThenBy(m => m.BaslangicSaati)
                .ToListAsync();
        }

        // Tek bir musaitlik kaydini Id ile getirir
        public async Task<VeterinerMusaitlik?> MusaitlikIdIleGetirAsync(int id)
        {
            return await _db.VeterinerMusaitlikler
                .Include(m => m.Veteriner)
                    .ThenInclude(v => v!.Klinik)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        // Yeni musaitlik saati ekler (saat sirasi, klinik saatine uyum ve cakisma kontrolu yapar)
        public async Task<(bool Basarili, string Mesaj)> MusaitlikEkleAsync(VeterinerMusaitlik musaitlik)
        {
            var dogrulama = await MusaitlikKurallariniKontrolEtAsync(musaitlik, null);
            if (!dogrulama.Basarili)
            {
                return dogrulama;
            }

            _db.VeterinerMusaitlikler.Add(musaitlik);
            await _db.SaveChangesAsync();
            return (true, "Müsaitlik saati eklendi.");
        }

        // Mevcut musaitlik saatini gunceller
        public async Task<(bool Basarili, string Mesaj)> MusaitlikGuncelleAsync(VeterinerMusaitlik musaitlik)
        {
            var mevcut = await _db.VeterinerMusaitlikler.FindAsync(musaitlik.Id);
            if (mevcut == null)
            {
                return (false, "Güncellenecek müsaitlik kaydı bulunamadı.");
            }

            musaitlik.VeterinerId = mevcut.VeterinerId;
            var dogrulama = await MusaitlikKurallariniKontrolEtAsync(musaitlik, mevcut.Id);
            if (!dogrulama.Basarili)
            {
                return dogrulama;
            }

            mevcut.Gun = musaitlik.Gun;
            mevcut.BaslangicSaati = musaitlik.BaslangicSaati;
            mevcut.BitisSaati = musaitlik.BitisSaati;

            await _db.SaveChangesAsync();
            return (true, "Müsaitlik saati güncellendi.");
        }

        // Musaitlik kaydini siler
        public async Task<(bool Basarili, string Mesaj)> MusaitlikSilAsync(int id)
        {
            var mevcut = await _db.VeterinerMusaitlikler.FindAsync(id);
            if (mevcut == null)
            {
                return (false, "Silinecek müsaitlik kaydı bulunamadı.");
            }

            _db.VeterinerMusaitlikler.Remove(mevcut);
            await _db.SaveChangesAsync();
            return (true, "Müsaitlik saati silindi.");
        }

        // Musaitlik ekleme ve guncelleme icin ortak kural kontrolu
        private async Task<(bool Basarili, string Mesaj)> MusaitlikKurallariniKontrolEtAsync(VeterinerMusaitlik musaitlik, int? haricId)
        {
            if (musaitlik.BaslangicSaati >= musaitlik.BitisSaati)
            {
                return (false, "Başlangıç saati, bitiş saatinden önce olmalıdır.");
            }

            var veteriner = await _db.Veterinerler
                .Include(v => v.Klinik)
                .FirstOrDefaultAsync(v => v.Id == musaitlik.VeterinerId);

            if (veteriner == null || veteriner.Klinik == null)
            {
                return (false, "Veteriner veya bağlı olduğu klinik bulunamadı.");
            }

            // Hekimin calisma saati, klinigin acilis-kapanis saatleri icinde olmali
            if (musaitlik.BaslangicSaati < veteriner.Klinik.AcilisSaati ||
                musaitlik.BitisSaati > veteriner.Klinik.KapanisSaati)
            {
                string acilis = veteriner.Klinik.AcilisSaati.ToString(@"hh\:mm");
                string kapanis = veteriner.Klinik.KapanisSaati.ToString(@"hh\:mm");
                return (false, $"Müsaitlik saatleri kliniğin çalışma saatleri ({acilis} - {kapanis}) içinde olmalıdır.");
            }

            // Ayni gun hekimin baska bir musaitlik araligiyla cakisiyor mu kontrol ediyoruz
            bool cakisiyorMu = await _db.VeterinerMusaitlikler.AnyAsync(m =>
                m.VeterinerId == musaitlik.VeterinerId &&
                m.Gun == musaitlik.Gun &&
                (!haricId.HasValue || m.Id != haricId.Value) &&
                musaitlik.BaslangicSaati < m.BitisSaati &&
                musaitlik.BitisSaati > m.BaslangicSaati);

            if (cakisiyorMu)
            {
                return (false, "Bu gün için girilen saat aralığı mevcut bir müsaitlik ile çakışıyor.");
            }

            return (true, string.Empty);
        }
    }
}