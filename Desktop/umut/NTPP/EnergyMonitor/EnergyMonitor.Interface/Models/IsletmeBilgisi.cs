using System;

namespace EnergyMonitor.Interface.Models
{
    public class IsletmeBilgisi
    {
        public int Id { get; set; }
        public string SirketAdi { get; set; } = string.Empty;
        public DateTime KurulusTarihi { get; set; }
        public decimal ButceLimiti { get; set; }
        public string ParaBirimi { get; set; } = "TL";
        public string GeminiApiAnahtari { get; set; } = string.Empty;
        
        // Faturalandırma Parametreleri
        public decimal BirimMaliyet { get; set; } = 2.5m;
        public decimal VergiOrani { get; set; } = 18.0m;
        public string AboneNo { get; set; } = string.Empty;
        public string MusteriNo { get; set; } = string.Empty;
        
        // Görseller
        public string LogoVerisi { get; set; } = string.Empty; // Base64 dizesi
        public DateTime GuncellenmeTarihi { get; set; }
    }
}
