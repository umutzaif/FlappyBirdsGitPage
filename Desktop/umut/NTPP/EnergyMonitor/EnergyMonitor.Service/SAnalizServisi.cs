using System;
using System.Collections.Generic;
using System.Linq;
using EnergyMonitor.Interface.Models;

namespace EnergyMonitor.Service
{
    public interface IAnalizServisi
    {
        DonerCihazTelemetrisi DonerCihazAnalizEt(List<decimal> titresimOrnekleri, decimal devirSayisi);
        double ArizaOlasiligiHesapla(Cihaz cihaz, List<SensorVerisi> gecmis);
        string BakimOnerisiGetir(Cihaz cihaz, double arizaOlasiligi);
    }

    public class SAnalizServisi : IAnalizServisi
    {
        public DonerCihazTelemetrisi DonerCihazAnalizEt(List<decimal> titresimOrnekleri, decimal devirSayisi)
        {
            var telemetri = new DonerCihazTelemetrisi { DevirSayisi = devirSayisi };
            
            if (titresimOrnekleri == null || !titresimOrnekleri.Any())
                return telemetri;

            // FFT (Fourier Dönüşümü) Simülasyonu
            // Gerçek uygulamada MathNet.Numerics veya benzeri kütüphane kullanılmalıdır
            telemetri.FrekansVerisi = titresimOrnekleri.Select(v => (decimal)Math.Abs(Math.Sin((double)v * 10)) * v).ToList();
            
            telemetri.TitresimX = titresimOrnekleri.Average();
            telemetri.TitresimY = titresimOrnekleri.Max() * 0.8m;
            telemetri.TitresimZ = titresimOrnekleri.Min() * 1.2m;

            // Basit Endüstriyel Mantık: Titreşim eşik değerini aşarsa veya RPM kararsızsa
            if (telemetri.TitresimX > 5.0m) // Örnek: 5mm/s eşik değeri
                telemetri.SaglikDurumu = "Kritik";
            else if (telemetri.TitresimX > 2.5m)
                telemetri.SaglikDurumu = "Uyari";
            else
                telemetri.SaglikDurumu = "Nominal";

            return telemetri;
        }

        public double ArizaOlasiligiHesapla(Cihaz cihaz, List<SensorVerisi> gecmis)
        {
            if (cihaz == null || gecmis == null || !gecmis.Any()) return 0.05;

            double olasilik = 0.05; // Baz %5 olasılık

            // 1. Yaş Faktörü
            if (cihaz.GirisTarihi.HasValue)
            {
                var gunSayisi = (DateTime.Now - cihaz.GirisTarihi.Value).TotalDays;
                olasilik += (gunSayisi / 365.0) * 0.1; // Yıl başına +%10 risk
            }

            // 2. Telemetri Faktörü (Titreşim/Isı trendleri)
            var sonAnomaliler = gecmis.Where(r => r.SensorTuru.ToLower().Contains("uyari") || r.SensorTuru.ToLower().Contains("kritik")).Count();
            olasilik += sonAnomaliler * 0.05;

            // 3. Bakım Faktörü
            if (cihaz.SonrakiBakim.HasValue && cihaz.SonrakiBakim < DateTime.Now)
            {
                olasilik += 0.2; // Gecikmişse +%20 risk
            }

            return Math.Min(olasilik, 0.99); // Maks %99
        }

        public string BakimOnerisiGetir(Cihaz cihaz, double arizaOlasiligi)
        {
            if (arizaOlasiligi > 0.7)
                return "ACİL BAKIM GEREKLİ: Rulman hasarı veya sargı ısınması tespit edildi. Üretim durdurulmalı.";
            if (arizaOlasiligi > 0.4)
                return "TEDBİRİ BAKIM: Titreşim değerleri nominal seyretmiyor. Haftalık periyotta kontrol edilmeli.";
            if (cihaz.SonrakiBakim.HasValue && (cihaz.SonrakiBakim.Value - DateTime.Now).TotalDays < 7)
                return "YAKLAŞAN BAKIM: Planlı bakım tarihine 1 haftadan az süre kaldı.";

            return "SİSTEM STABİL: Tüm parametreler nominal değerler içerisinde.";
        }
    }
}
