using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraPrinting;
using EnergyMonitor.Interface.Services;
using EnergyMonitor.Interface.Models;
using System.Diagnostics;
using System.IO;

namespace EnergyMonitor.Forms
{
    public class RaporYoneticisi
    {
        private readonly ICihazServisi _cihazServisi;
        private readonly IVeriKaydedici _veriKaydedici;
        
        public RaporYoneticisi(ICihazServisi cihazServisi, IVeriKaydedici veriKaydedici)
        {
            _cihazServisi = cihazServisi;
            _veriKaydedici = veriKaydedici;
        }

        public async System.Threading.Tasks.Task CihazRaporuOlusturAsync(string dosyaYolu, DateTime baslangic, DateTime bitis, List<int> cihazIdleri, bool loglariDahilEt)
        {
            if (loglariDahilEt)
            {
                var loglar = await _veriKaydedici.VerileriGetirAsync(cihazIdleri.FirstOrDefault(), baslangic, bitis);
                
                var detayliListe = loglar.Select(l => new 
                {
                    Tarih = l.ZamanDamgasi,
                    CihazID = l.CihazId,
                    Sensor = l.SensorTuru,
                    Deger = l.Deger,
                    Birim = l.Birim
                }).ToList();

                if (detayliListe.Count == 0)
                {
                     detayliListe.Add(new { Tarih = DateTime.Now, CihazID = 0, Sensor = "BILGI", Deger = 0m, Birim = "Veri Yok" });
                }

                ListeyiPdfOlarakDisariAktar(detayliListe, $"Detaylı Cihaz Logları ({baslangic.ToShortDateString()} - {bitis.ToShortDateString()})", dosyaYolu);
            }
            else
            {
                var cihazlar = await _cihazServisi.TumCihazlariGetirAsync();
                
                if (cihazIdleri != null && cihazIdleri.Count > 0)
                {
                    cihazlar = cihazlar.Where(d => cihazIdleri.Contains(d.Id)).ToList();
                }

                var liste = cihazlar.Select(d => new 
                {
                    ID = d.Id,
                    Ad = d.Ad,
                    Marka = d.Marka,
                    Model = d.Model,
                    Voltaj = d.GerilimDegeri,
                    Akim = d.AkimDegeri,
                    MAC = d.MacAdresi,
                    Kayit = d.GirisTarihi
                }).ToList();

                ListeyiPdfOlarakDisariAktar(liste, $"Cihaz Raporu ({baslangic.ToShortDateString()} - {bitis.ToShortDateString()})", dosyaYolu);
            }
        }

        public async System.Threading.Tasks.Task ZRaporuOlusturAsync(string dosyaYolu)
        {
            DateTime baslangic = DateTime.Today;
            DateTime bitis = DateTime.Today.AddDays(1).AddTicks(-1);
            
            var loglar = await _veriKaydedici.VerileriGetirAsync(null, baslangic, bitis);
            
            var ozet = loglar
                .Where(l => l.SensorTuru == "Power" || l.SensorTuru == "Energy" || l.SensorTuru == "Current")
                .GroupBy(l => l.CihazId)
                .Select(g => new 
                {
                    CihazID = g.Key,
                    OkumaSayisi = g.Count(),
                    OrtalamaGuc = g.Where(x => x.SensorTuru == "Power").Select(x => x.Deger).DefaultIfEmpty(0m).Average(),
                    MinGuc = g.Where(x => x.SensorTuru == "Power").Select(x => x.Deger).DefaultIfEmpty(0m).Min(),
                    MaxGuc = g.Where(x => x.SensorTuru == "Power").Select(x => x.Deger).DefaultIfEmpty(0m).Max()
                }).ToList();

            ListeyiPdfOlarakDisariAktar(ozet, $"Z-Raporu / Özet ({baslangic.ToShortDateString()})", dosyaYolu);
        }

        public async System.Threading.Tasks.Task ButceRaporuOlusturAsync(string dosyaYolu, decimal birimFiyat = 2.5m)
        {
            DateTime baslangic = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime bitis = DateTime.Now;
            
            var loglar = await _veriKaydedici.VerileriGetirAsync(null, baslangic, bitis);

            var butce = loglar
                 .Where(l => l.SensorTuru == "Power") 
                 .GroupBy(l => l.CihazId)
                 .Select(g => new 
                 {
                     CihazID = g.Key,
                     KayitSayisi = g.Count(),
                     TahminiTuketimkWh = (g.Select(x => x.Deger).Average() * 24 * 30 / 1000), 
                     BirimFiyat = birimFiyat,
                     TahminiFatura = ((g.Select(x => x.Deger).Average() * 24 * 30 / 1000) * birimFiyat)
                 }).ToList();

             ListeyiPdfOlarakDisariAktar(butce, $"Bütçe Tahmin Raporu ({DateTime.Now:MMMM yyyy})", dosyaYolu);
        }

        public async System.Threading.Tasks.Task YogunlukRaporuOlusturAsync(string dosyaYolu, DateTime baslangic, DateTime bitis, List<int> cihazIdleri)
        {
            var loglar = await _veriKaydedici.VerileriGetirAsync(cihazIdleri.FirstOrDefault(), baslangic, bitis);

            var yogunluk = loglar
                .GroupBy(l => l.ZamanDamgasi.Hour)
                .Select(g => new 
                {
                    Saat = $"{g.Key}:00 - {g.Key + 1}:00",
                    OkumaSayisi = g.Count(),
                    OrtalamaDeger = g.Select(x => x.Deger).Average()
                })
                .OrderByDescending(x => x.OrtalamaDeger)
                .ToList();

            ListeyiPdfOlarakDisariAktar(yogunluk, $"Kullanım Yoğunluk Raporu ({baslangic.ToShortDateString()} - {bitis.ToShortDateString()})", dosyaYolu);
        }

        public void YesilEnerjiRaporuOlustur(decimal toplamGucKw, string dosyaYolu)
        {
            double karbonAyakIzi = (double)toplamGucKw * 0.4;
            double gerekliAgac = karbonAyakIzi / 20.0; 

            var veri = new List<object>
            {
                new { Metrik = "Toplam Tüketim", Deger = $"{toplamGucKw:0.00} kWh" },
                new { Metrik = "Karbon Ayak İzi", Deger = $"{karbonAyakIzi:0.00} kg CO2" },
                new { Metrik = "Gerekli Ağaç (Yıllık)", Deger = $"{Math.Ceiling(gerekliAgac)} Adet Ağaç" },
                new { Metrik = "Durum", Deger = karbonAyakIzi > 100 ? "KRİTİK - İyileştirme Gerekli" : "İYİ - Standartlar Altında" }
            };

            ListeyiPdfOlarakDisariAktar(veri, "Yeşil Enerji / Karbon Raporu", dosyaYolu);
        }

        public void AiAnalizRaporuOlustur(string analizMetni, string dosyaYolu)
        {
            var satirlar = analizMetni.Split(new[] { '\r', '\n', '.' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(line => new { Analiz_Detayi = line.Trim() })
                                    .Where(x => x.Analiz_Detayi.Length > 5)
                                    .ToList();

            ListeyiPdfOlarakDisariAktar(satirlar, "Yapay Zeka Destekli Enerji Analiz Raporu", dosyaYolu);
        }

        private void ListeyiPdfOlarakDisariAktar(object verikaynagi, string baslik, string dosyaYolu)
        {
            using (var grid = new GridControl())
            {
                grid.BindingContext = new System.Windows.Forms.BindingContext();
                var view = new GridView(grid);
                grid.MainView = view;
                grid.DataSource = verikaynagi;
                
                grid.ForceInitialize();
                view.PopulateColumns();

                view.OptionsPrint.AutoWidth = false;
                view.OptionsPrint.ShowPrintExportProgress = false;
                
                view.OptionsView.ShowViewCaption = true;
                view.ViewCaption = baslik;

                view.ExportToPdf(dosyaYolu);
            }
            
            try 
            { 
                 Process.Start(new ProcessStartInfo(dosyaYolu) { UseShellExecute = true }); 
            } 
            catch { }
        }
    }
}
