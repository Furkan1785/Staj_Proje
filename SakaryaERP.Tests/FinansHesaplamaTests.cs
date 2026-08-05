using SakaryaERP.Helpers;
using SakaryaERP.Models;
using Xunit;

namespace SakaryaERP.Tests;

public class FinansHesaplamaTests
{
    [Fact]
    public void BorcAlacakDengesiniDogrula_DengeliKalemler_HataFirlatmaz()
    {
        List<MuhasebeFisiKalemi> kalemler =
        [
            new() { Borc = 600, Alacak = 0 },
            new() { Borc = 0, Alacak = 500 },
            new() { Borc = 0, Alacak = 100 }
        ];

        var hata = Record.Exception(() => FinansHesaplama.BorcAlacakDengesiniDogrula(kalemler));

        Assert.Null(hata);
    }

    [Fact]
    public void BorcAlacakDengesiniDogrula_DengesizKalemler_HataFirlatir()
    {
        List<MuhasebeFisiKalemi> kalemler =
        [
            new() { Borc = 600, Alacak = 0 },
            new() { Borc = 0, Alacak = 500 }
        ];

        var hata = Assert.Throws<InvalidOperationException>(() => FinansHesaplama.BorcAlacakDengesiniDogrula(kalemler));

        Assert.Contains("dengesiz", hata.Message, StringComparison.OrdinalIgnoreCase);
    }
}
