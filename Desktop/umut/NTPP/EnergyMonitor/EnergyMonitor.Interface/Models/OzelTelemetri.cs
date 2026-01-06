using System.Collections.Generic;

namespace EnergyMonitor.Interface.Models
{
    public class DonerCihazTelemetrisi
    {
        public decimal DevirSayisi { get; set; } // RPM
        public decimal TitresimX { get; set; }  // mm/s
        public decimal TitresimY { get; set; }  // mm/s
        public decimal TitresimZ { get; set; }  // mm/s
        public List<decimal>? FrekansVerisi { get; set; } // FFT
        public string SaglikDurumu { get; set; } = "Nominal"; // Nominal, Uyari, Kritik
    }

    public class TermalCihazTelemetrisi
    {
        public decimal Sicaklik { get; set; }
        public decimal Nem { get; set; }
        public decimal IsilGradyan { get; set; }
    }

    public class GucKalitesiTelemetrisi
    {
        public List<decimal>? Harmonikler { get; set; }
        public decimal GucFaktoru { get; set; }
        public decimal ToplamHarmonikBozunma { get; set; } // THD
    }
}
