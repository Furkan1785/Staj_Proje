namespace SakaryaERP.ViewModels;

public class MizanViewModel
{
    public DateTime Baslangic { get; set; }
    public DateTime Bitis { get; set; }
    public List<MizanSatiriViewModel> Satirlar { get; set; } = [];

    public decimal ToplamBorc => Satirlar.Sum(s => s.ToplamBorc);
    public decimal ToplamAlacak => Satirlar.Sum(s => s.ToplamAlacak);
    public decimal ToplamBorcBakiyesi => Satirlar.Sum(s => s.BorcBakiyesi);
    public decimal ToplamAlacakBakiyesi => Satirlar.Sum(s => s.AlacakBakiyesi);
}

public class MizanSatiriViewModel
{
    public string HesapKodu { get; set; } = "";
    public string HesapAdi { get; set; } = "";
    public decimal ToplamBorc { get; set; }
    public decimal ToplamAlacak { get; set; }

    // Klasik mizan sunumu: net bakiye borç tarafındaysa BorcBakiyesi, alacak tarafındaysa
    // AlacakBakiyesi dolu olur — ikisi aynı anda dolu olmaz.
    public decimal BorcBakiyesi => ToplamBorc > ToplamAlacak ? ToplamBorc - ToplamAlacak : 0;
    public decimal AlacakBakiyesi => ToplamAlacak > ToplamBorc ? ToplamAlacak - ToplamBorc : 0;
}
