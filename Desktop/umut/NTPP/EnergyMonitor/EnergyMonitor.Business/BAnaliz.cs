using System;
using System.Linq;
using System.Threading.Tasks;
using EnergyMonitor.SP;

namespace EnergyMonitor.Business
{
    public class BAnaliz
    {
        private readonly SpAnaliz _spAnaliz;
        private const decimal ELEKTRIK_BIRIM_FIYATI = 2.5m; // kWh başına TL

        public BAnaliz(string baglantiCumlesi)
        {
            _spAnaliz = new SpAnaliz(baglantiCumlesi);
        }

        public async Task<decimal> AylikTuketimTahminEtAsync()
        {
            // Basit mantık: Son 24 saatin ortalama 'güç' kullanımı * 24 * 30
            var bitis = DateTime.UtcNow;
            var baslangic = bitis.AddHours(-24);
            
            var veriler = await _spAnaliz.GecmisVerileriGetirAsync("power", baslangic, bitis);
            
            if (!veriler.Any()) return 0;

            var ortalamaGucWatt = veriler.Average(r => r.Deger);
            var gunlukKwh = (ortalamaGucWatt / 1000) * 24;
            
            return gunlukKwh * 30; // Aylık kWh tahmini
        }

        public async Task<decimal> FaturaTahminEtAsync()
        {
            var aylikKwh = await AylikTuketimTahminEtAsync();
            return aylikKwh * ELEKTRIK_BIRIM_FIYATI;
        }

        public async Task<string> TasarrufOnerisiGetirAsync()
        {
            var bitis = DateTime.UtcNow;
            var baslangic = bitis.AddDays(-7); // Son 7 günü analiz et
            
            var veriler = await _spAnaliz.GecmisVerileriGetirAsync("power", baslangic, bitis);

            if (!veriler.Any()) 
                return "Analiz için yeterli veri yok. Cihazları çalıştırın.";

            // Günün saatlerine göre gruplayarak yoğun kullanım zamanlarını bul
            var saatlikKullanim = veriler
                .GroupBy(r => r.Zaman.ToLocalTime().Hour)
                .Select(g => new { Saat = g.Key, OrtGuc = g.Average(r => r.Deger) })
                .OrderByDescending(x => x.OrtGuc)
                .ToList();

            if (!saatlikKullanim.Any()) return "Veri analizi yapılamadı.";

            var zirve = saatlikKullanim.First();
            var genelOrtalama = saatlikKullanim.Average(x => x.OrtGuc);

            // Mantık: Eğer zirve saat kullanımı ortalamadan %20 daha fazlaysa azaltma öner
            if (zirve.OrtGuc > (genelOrtalama * 1.2m))
            {
                return $"Makine 1 (Ana Jeneratör) saat {zirve.Saat}:00 itibariyle çok yüksek güç tüketiyor ({zirve.OrtGuc:0.0} W). Saat {zirve.Saat}:00'den sonra yarım güçte çalıştırılsın.";
            }

            return "Tüketim profiliniz dengeli. Mevcut işletim planına devam edebilirsiniz.";
        }
    }
}
