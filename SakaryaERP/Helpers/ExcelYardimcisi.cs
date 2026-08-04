using ClosedXML.Excel;

namespace SakaryaERP.Helpers;

// 16 modülün "Excel'e Aktar" action'larında tekrar eden ClosedXML iskeletini
// (başlık satırı + veri satırları + otomatik sütun genişliği) tek yerde toplar.
public static class ExcelYardimcisi
{
    public static byte[] ListeOlustur(string sayfaAdi, string[] basliklar, IEnumerable<object?[]> satirlar)
    {
        using var workbook = new XLWorkbook();
        var sayfa = workbook.Worksheets.Add(sayfaAdi);

        for (var i = 0; i < basliklar.Length; i++)
        {
            sayfa.Cell(1, i + 1).Value = basliklar[i];
            sayfa.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var satirNo = 2;
        foreach (var satir in satirlar)
        {
            for (var i = 0; i < satir.Length; i++)
                HucreyeYaz(sayfa.Cell(satirNo, i + 1), satir[i]);
            satirNo++;
        }

        sayfa.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void HucreyeYaz(IXLCell hucre, object? deger)
    {
        switch (deger)
        {
            case null:
                break;
            case DateTime tarih:
                hucre.Value = tarih;
                hucre.Style.DateFormat.Format = "dd.MM.yyyy";
                break;
            case decimal ondalik:
                hucre.Value = ondalik;
                break;
            case int tamsayi:
                hucre.Value = tamsayi;
                break;
            default:
                hucre.Value = deger.ToString();
                break;
        }
    }
}
