using System;

namespace EnergyMonitor.Interface.Models
{
    public class Mesaj
    {
        public int Id { get; set; }
        public int GonderenId { get; set; }
        public int AliciId { get; set; }
        public string Icerik { get; set; } = string.Empty;
        public DateTime GonderilmeTarihi { get; set; } = DateTime.Now;
        
        // Gösterim için yardımcı özellik
        public string GonderenAdi { get; set; } = string.Empty;
    }
}
