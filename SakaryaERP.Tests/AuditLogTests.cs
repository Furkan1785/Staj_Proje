using SakaryaERP.Models;
using Xunit;

namespace SakaryaERP.Tests;

public class AuditLogTests
{
    [Fact]
    public async Task SaveChangesAsync_TrackedEntityAlanDegisirse_DenetimKaydiOlusur()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var malzeme = new Malzeme { MalzemeKodu = "M001", MalzemeAdi = "Eski Ad", Birim = "Adet" };
        baglam.Malzemeler.Add(malzeme);
        await baglam.SaveChangesAsync();

        malzeme.MalzemeAdi = "Yeni Ad";
        await baglam.SaveChangesAsync();

        var kayit = Assert.Single(baglam.AuditLoglari, a => a.AlanAdi == nameof(Malzeme.MalzemeAdi));
        Assert.Equal(nameof(Malzeme), kayit.EntityAdi);
        Assert.Equal(malzeme.Id, kayit.EntityId);
        Assert.Equal("Eski Ad", kayit.EskiDeger);
        Assert.Equal("Yeni Ad", kayit.YeniDeger);
    }

    [Fact]
    public async Task SaveChangesAsync_YeniKayitEklenirse_DenetimKaydiOlusmaz()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();

        baglam.Malzemeler.Add(new Malzeme { MalzemeKodu = "M001", MalzemeAdi = "Yeni Malzeme", Birim = "Adet" });
        await baglam.SaveChangesAsync();

        Assert.Empty(baglam.AuditLoglari);
    }

    [Fact]
    public async Task SaveChangesAsync_DegismeyenAlanIcinDenetimKaydiOlusmaz()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var malzeme = new Malzeme { MalzemeKodu = "M001", MalzemeAdi = "Ad", Marka = "Sabit Marka", Birim = "Adet" };
        baglam.Malzemeler.Add(malzeme);
        await baglam.SaveChangesAsync();

        malzeme.MalzemeAdi = "Değişen Ad";
        // Marka aynı değere yeniden atanıyor — bu alan için denetim kaydı oluşmamalı
        malzeme.Marka = "Sabit Marka";
        await baglam.SaveChangesAsync();

        Assert.DoesNotContain(baglam.AuditLoglari, a => a.AlanAdi == nameof(Malzeme.Marka));
        Assert.Contains(baglam.AuditLoglari, a => a.AlanAdi == nameof(Malzeme.MalzemeAdi));
    }

    [Fact]
    public async Task SaveChangesAsync_SoftDelete_IsDeletedDegisimiDenetlenir()
    {
        var baglam = TestDbContextFactory.OlusturYeniBaglam();
        var cari = new Cari { CariKodu = "C001", Unvan = "Test Cari", CariTipi = CariTipi.Musteri };
        baglam.Cariler.Add(cari);
        await baglam.SaveChangesAsync();

        cari.IsDeleted = true;
        await baglam.SaveChangesAsync();

        var kayit = Assert.Single(baglam.AuditLoglari, a => a.AlanAdi == nameof(Cari.IsDeleted));
        Assert.Equal("False", kayit.EskiDeger);
        Assert.Equal("True", kayit.YeniDeger);
    }
}
