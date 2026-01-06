using System;

namespace EnergyMonitor.Interface.Models
{
    public enum CihazTuru
    {
        Statik = 0,     // Genel ekipman
        Doner = 1,      // Motorlar, pompalar (Titreşim/RPM/FFT)
        Termal = 2,     // Isıtıcılar, fırınlar (Sıcaklık/Gradyan)
        Guc = 3         // Trafolar, İnverterler (Harmonikler/Kalite)
    }

    public class Cihaz
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string? Marka { get; set; }
        public string? Model { get; set; }
        public CihazTuru Tur { get; set; }
        public string? MacAdresi { get; set; } // Karmaşık Kimlik (MAC)
        public int Durum { get; set; } // 0: Lobi, 1: Aktif, 2: Pasif, 3: Çöp
        public decimal? GerilimDegeri { get; set; }
        public decimal? AkimDegeri { get; set; }
        public decimal? Agirlik { get; set; }
        public DateTime? GirisTarihi { get; set; }
        public DateTime? SonBakim { get; set; }
        public DateTime? SonrakiBakim { get; set; }

        // IoT ve Ağ Ayarları
        public string? WifiAd { get; set; }
        public string? WifiSifre { get; set; }
        public int VeriPeriyodu { get; set; } = 5000; // ms
        public string SensorSecimi { get; set; } = "HEPSI"; // V,I,P,T,R,B
        public bool AnaCihazMi { get; set; }
    }
}
