using System;
using System.IO;
using System.Text.Json;

namespace EnergyMonitor.Forms
{
    public class ProgramAyarlari
    {
        public string GeminiApiAnahtari { get; set; } = "";

        // Uygulama çalışma dizinine kaydeder
        private static string YapilandirmaYolu => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user.config.json");

        public static ProgramAyarlari Yukle()
        {
            if (File.Exists(YapilandirmaYolu))
            {
                try 
                { 
                    var ayarlar = JsonSerializer.Deserialize<ProgramAyarlari>(File.ReadAllText(YapilandirmaYolu));
                    return ayarlar ?? new ProgramAyarlari();
                }
                catch 
                {
                    // Bozuk dosyayı görmezden gel, varsayılanı dön
                }
            }
            return new ProgramAyarlari();
        }

        public void Kaydet()
        {
            try
            {
                var secenekler = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(YapilandirmaYolu, JsonSerializer.Serialize(this, secenekler));
            }
            catch (Exception)
            {
                // Hata günlüğü tutulabilir
            }
        }
    }
}
