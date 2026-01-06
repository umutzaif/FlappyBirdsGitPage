using System;

namespace EnergyMonitor.Interface.Models
{
    public class Kullanici
    {
        public int Id { get; set; }
        public string KullaniciAdi { get; set; } = string.Empty;
        public string? SifreOzeti { get; set; }
        public string Rol { get; set; } = "Personel"; // Yonetici, Muhasebe, Personel
        public int RolId { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public string? ProfilFotografi { get; set; } // Base64 string
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;       
        
        public string RolAdi { get; set; } = string.Empty; 
        public bool AktifMi { get; set; } = false;
        public int? IsletmeId { get; set; }
        public string? IsletmeAdi { get; set; }
    }
}
