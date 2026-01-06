using System;

namespace EnergyMonitor.Interface.Models
{
    public class SensorVerisi
    {
        public int Id { get; set; }
        public int CihazId { get; set; }
        public string SensorTuru { get; set; } = string.Empty;
        public decimal Deger { get; set; }
        public string Birim { get; set; } = string.Empty;
        public DateTime ZamanDamgasi { get; set; }
        public string? TelemetriVerisi { get; set; } // JSON Verisi
    }
}
