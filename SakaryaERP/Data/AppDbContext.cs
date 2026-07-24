using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SakaryaERP.Models;

namespace SakaryaERP.Data;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Sube> Subeler { get; set; }
    public DbSet<Cari> Cariler { get; set; }
    public DbSet<BankaHesabi> BankaHesaplari { get; set; }
    public DbSet<KasaHesabi> KasaHesaplari { get; set; }
    public DbSet<CariFisi> CariFisleri { get; set; }
    public DbSet<CekSenet> CekSenetler { get; set; }
    public DbSet<HesapPlani> HesapPlani { get; set; }
    public DbSet<MuhasebeFisi> MuhasebeFisleri { get; set; }
    public DbSet<MuhasebeFisiKalemi> MuhasebeFisiKalemleri { get; set; }
    public DbSet<MalzemeKategori> MalzemeKategoriler { get; set; }
    public DbSet<Malzeme> Malzemeler { get; set; }
    public DbSet<MalzemeHareketFisi> MalzemeHareketFisleri { get; set; }
    public DbSet<MalzemeHareketFisiKalemi> MalzemeHareketFisiKalemleri { get; set; }
    public DbSet<AlisTalebi> AlisTalepleri { get; set; }
    public DbSet<AlisTeklifi> AlisTeklifleri { get; set; }
    public DbSet<AlisTeklifiKalemi> AlisTeklifiKalemleri { get; set; }
    public DbSet<AlisSiparisi> AlisSiparisleri { get; set; }
    public DbSet<AlisSiparisiKalemi> AlisSiparisiKalemleri { get; set; }
    public DbSet<AlisIrsaliyesi> AlisIrsaliyeleri { get; set; }
    public DbSet<AlisIrsaliyesiKalemi> AlisIrsaliyesiKalemleri { get; set; }
    public DbSet<AlisFaturasi> AlisFaturalari { get; set; }
    public DbSet<AlisFaturasiKalemi> AlisFaturasiKalemleri { get; set; }
    public DbSet<MusteriTalebi> MusteriTalepleri { get; set; }
    public DbSet<SatisTeklifi> SatisTeklifleri { get; set; }
    public DbSet<SatisTeklifiKalemi> SatisTeklifiKalemleri { get; set; }
    public DbSet<SatisSiparisi> SatisSiparisleri { get; set; }
    public DbSet<SatisSiparisiKalemi> SatisSiparisiKalemleri { get; set; }
    public DbSet<SevkIrsaliyesi> SevkIrsaliyeleri { get; set; }
    public DbSet<SevkIrsaliyesiKalemi> SevkIrsaliyesiKalemleri { get; set; }
    public DbSet<SatisFaturasi> SatisFaturalari { get; set; }
    public DbSet<SatisFaturasiKalemi> SatisFaturasiKalemleri { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global soft-delete query filter for all BaseEntity types
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var condition = Expression.Equal(property, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }

        // Npgsql, DateTime.Kind'a göre sütun tipini zorluyor: "with time zone" sadece Kind=Utc,
        // "without time zone" sadece Kind=Unspecified kabul ediyor. Uygulama tek saat diliminde (TR)
        // çalıştığından tüm DateTime sütunlarını "without time zone" yapıp, hangi Kind ile gelirse gelsin
        // (form'dan Unspecified, DateTime.UtcNow'dan Utc, DateTime.Now/Today'den Local) bir value converter
        // ile Unspecified'a normalize ediyoruz. Böylece her yeni tarih alanında (Gün 8 Vade Tarihi vb.)
        // bu hatayla tekrar karşılaşılmaz.
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified),
            v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified));

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Unspecified) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Unspecified) : v);

        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
        {
            property.SetColumnType("timestamp without time zone");
            property.SetValueConverter(property.ClrType == typeof(DateTime) ? dateTimeConverter : nullableDateTimeConverter);
        }

        // HesapPlani öz-ilişki (hiyerarşik)
        modelBuilder.Entity<HesapPlani>()
            .HasOne(h => h.Parent)
            .WithMany(h => h.AltHesaplar)
            .HasForeignKey(h => h.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // MalzemeKategori öz-ilişki (hiyerarşik)
        modelBuilder.Entity<MalzemeKategori>()
            .HasOne(k => k.Parent)
            .WithMany(k => k.AltKategoriler)
            .HasForeignKey(k => k.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // CariFisi — BankaHesabi ve KasaHesabi nullable FK
        modelBuilder.Entity<CariFisi>()
            .HasOne(f => f.BankaHesabi)
            .WithMany(b => b.CariFisleri)
            .HasForeignKey(f => f.BankaHesabiId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CariFisi>()
            .HasOne(f => f.KasaHesabi)
            .WithMany(k => k.CariFisleri)
            .HasForeignKey(f => f.KasaHesabiId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // MuhasebeFisi — SatisFaturasi ve AlisFaturasi nullable FK (1-to-1)
        modelBuilder.Entity<MuhasebeFisi>()
            .HasOne(m => m.SatisFaturasi)
            .WithOne(s => s.MuhasebeFisi)
            .HasForeignKey<MuhasebeFisi>(m => m.SatisFaturasiId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MuhasebeFisi>()
            .HasOne(m => m.AlisFaturasi)
            .WithOne(a => a.MuhasebeFisi)
            .HasForeignKey<MuhasebeFisi>(m => m.AlisFaturasiId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // AppUser — Sube nullable FK
        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Sube)
            .WithMany(s => s.Kullanicilar)
            .HasForeignKey(u => u.SubeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Kalem tabloları cascade delete
        modelBuilder.Entity<MuhasebeFisiKalemi>()
            .HasOne(k => k.MuhasebeFisi)
            .WithMany(f => f.Kalemler)
            .HasForeignKey(k => k.MuhasebeFisiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MalzemeHareketFisiKalemi>()
            .HasOne(k => k.MalzemeHareketFisi)
            .WithMany(f => f.Kalemler)
            .HasForeignKey(k => k.MalzemeHareketFisiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AlisSiparisiKalemi>()
            .HasOne(k => k.AlisSiparisi)
            .WithMany(s => s.Kalemler)
            .HasForeignKey(k => k.AlisSiparisiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AlisTeklifiKalemi>()
            .HasOne(k => k.AlisTeklifi)
            .WithMany(t => t.Kalemler)
            .HasForeignKey(k => k.AlisTeklifiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AlisIrsaliyesiKalemi>()
            .HasOne(k => k.AlisIrsaliyesi)
            .WithMany(i => i.Kalemler)
            .HasForeignKey(k => k.AlisIrsaliyesiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AlisFaturasiKalemi>()
            .HasOne(k => k.AlisFaturasi)
            .WithMany(f => f.Kalemler)
            .HasForeignKey(k => k.AlisFaturasiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SatisTeklifiKalemi>()
            .HasOne(k => k.SatisTeklifi)
            .WithMany(t => t.Kalemler)
            .HasForeignKey(k => k.SatisTeklifiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SatisSiparisiKalemi>()
            .HasOne(k => k.SatisSiparisi)
            .WithMany(s => s.Kalemler)
            .HasForeignKey(k => k.SatisSiparisiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SevkIrsaliyesiKalemi>()
            .HasOne(k => k.SevkIrsaliyesi)
            .WithMany(i => i.Kalemler)
            .HasForeignKey(k => k.SevkIrsaliyesiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SatisFaturasiKalemi>()
            .HasOne(k => k.SatisFaturasi)
            .WithMany(f => f.Kalemler)
            .HasForeignKey(k => k.SatisFaturasiId)
            .OnDelete(DeleteBehavior.Cascade);

        // Malzeme FK'leri — Restrict (malzeme silinemez, stok hareketi varsa)
        modelBuilder.Entity<MalzemeHareketFisiKalemi>()
            .HasOne(k => k.Malzeme)
            .WithMany(m => m.HareketKalemleri)
            .HasForeignKey(k => k.MalzemeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique kısıtlamalar
        modelBuilder.Entity<Cari>()
            .HasIndex(c => c.CariKodu)
            .IsUnique();

        modelBuilder.Entity<Malzeme>()
            .HasIndex(m => m.MalzemeKodu)
            .IsUnique();

        modelBuilder.Entity<HesapPlani>()
            .HasIndex(h => h.HesapKodu)
            .IsUnique();

        modelBuilder.Entity<CariFisi>()
            .HasIndex(f => f.FisNo)
            .IsUnique();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.IsDeleted = false;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}