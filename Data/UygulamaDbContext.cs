using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetKlinikSistemi.Models.Entities;

namespace VetKlinikSistemi.Data
{
    // Identity tablolarini ve kendi tablolarimizi tek DbContext uzerinde yonetiyoruz
    public class UygulamaDbContext : IdentityDbContext<Kullanici>
    {
        public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Klinik> Klinikler { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<UzmanlikAlani> UzmanlikAlanlari { get; set; }
        public DbSet<Veteriner> Veterinerler { get; set; }
        public DbSet<VeterinerUzmanlik> VeterinerUzmanliklar { get; set; }
        public DbSet<VeterinerHizmet> VeterinerHizmetler { get; set; }
        public DbSet<VeterinerMusaitlik> VeterinerMusaitlikler { get; set; }
        public DbSet<EvcilHayvan> EvcilHayvanlar { get; set; }
        public DbSet<AsiKaydi> AsiKayitlari { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Decimal alanlar icin SQL Server hassasiyet ayarlari
            builder.Entity<Hizmet>()
                .Property(h => h.Ucret)
                .HasPrecision(10, 2);

            builder.Entity<Randevu>()
                .Property(r => r.Ucret)
                .HasPrecision(10, 2);

            builder.Entity<EvcilHayvan>()
                .Property(e => e.Kilo)
                .HasPrecision(6, 2);

            // Coktan-coga ara tablo: VeterinerUzmanlik bilesik anahtari
            builder.Entity<VeterinerUzmanlik>()
                .HasKey(vu => new { vu.VeterinerId, vu.UzmanlikAlaniId });

            builder.Entity<VeterinerUzmanlik>()
                .HasOne(vu => vu.Veteriner)
                .WithMany(v => v.VeterinerUzmanliklar)
                .HasForeignKey(vu => vu.VeterinerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<VeterinerUzmanlik>()
                .HasOne(vu => vu.UzmanlikAlani)
                .WithMany(u => u.VeterinerUzmanliklar)
                .HasForeignKey(vu => vu.UzmanlikAlaniId)
                .OnDelete(DeleteBehavior.Restrict);

            // Coktan-coga ara tablo: VeterinerHizmet bilesik anahtari
            builder.Entity<VeterinerHizmet>()
                .HasKey(vh => new { vh.VeterinerId, vh.HizmetId });

            builder.Entity<VeterinerHizmet>()
                .HasOne(vh => vh.Veteriner)
                .WithMany(v => v.VeterinerHizmetler)
                .HasForeignKey(vh => vh.VeterinerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<VeterinerHizmet>()
                .HasOne(vh => vh.Hizmet)
                .WithMany(h => h.VeterinerHizmetler)
                .HasForeignKey(vh => vh.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kullanici - Veteriner bire-bir iliskisi
            builder.Entity<Veteriner>()
                .HasOne(v => v.Kullanici)
                .WithOne(k => k.VeterinerProfili)
                .HasForeignKey<Veteriner>(v => v.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            // SQL Server'da coklu cascade delete hatasini onlemek icin Restrict kullaniyoruz
            builder.Entity<Hizmet>()
                .HasOne(h => h.Klinik)
                .WithMany(k => k.Hizmetler)
                .HasForeignKey(h => h.KlinikId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Veteriner>()
                .HasOne(v => v.Klinik)
                .WithMany(k => k.Veterinerler)
                .HasForeignKey(v => v.KlinikId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<VeterinerMusaitlik>()
                .HasOne(vm => vm.Veteriner)
                .WithMany(v => v.Musaitlikler)
                .HasForeignKey(vm => vm.VeterinerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EvcilHayvan>()
                .HasOne(e => e.Sahip)
                .WithMany(k => k.EvcilHayvanlar)
                .HasForeignKey(e => e.SahipId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AsiKaydi>()
                .HasOne(a => a.EvcilHayvan)
                .WithMany(e => e.AsiKayitlari)
                .HasForeignKey(a => a.EvcilHayvanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Randevu>()
                .HasOne(r => r.EvcilHayvan)
                .WithMany(e => e.Randevular)
                .HasForeignKey(r => r.EvcilHayvanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Randevu>()
                .HasOne(r => r.Veteriner)
                .WithMany(v => v.Randevular)
                .HasForeignKey(r => r.VeterinerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Randevu>()
                .HasOne(r => r.Hizmet)
                .WithMany(h => h.Randevular)
                .HasForeignKey(r => r.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Randevu>()
                .HasOne(r => r.Klinik)
                .WithMany(k => k.Randevular)
                .HasForeignKey(r => r.KlinikId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}