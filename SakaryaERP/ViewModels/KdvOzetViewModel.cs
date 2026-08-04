namespace SakaryaERP.ViewModels;

public class KdvOzetViewModel
{
    public DateTime Baslangic { get; set; }
    public DateTime Bitis { get; set; }
    public decimal HesaplananKdv { get; set; }
    public decimal IndirilecekKdv { get; set; }
    public decimal OdenecekKdv => HesaplananKdv - IndirilecekKdv;
    public List<AylikKdvViewModel> AylikKirilim { get; set; } = [];
}

public class AylikKdvViewModel
{
    public int Yil { get; set; }
    public int Ay { get; set; }
    public string Etiket { get; set; } = "";
    public decimal HesaplananKdv { get; set; }
    public decimal IndirilecekKdv { get; set; }
    public decimal OdenecekKdv => HesaplananKdv - IndirilecekKdv;
}
