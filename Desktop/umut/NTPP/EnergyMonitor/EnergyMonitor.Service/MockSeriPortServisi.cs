using System;
using System.Threading.Tasks;
using EnergyMonitor.Interface.Services;

namespace EnergyMonitor.Service
{
    public class MockSeriPortServisi : ISeriPortServisi
    {
        private bool _bagliMi;
        public event EventHandler<string>? VeriAlindi;

        public bool AcikMi => _bagliMi;

        public string[] MevcutPortlariGetir()
        {
            return new string[] { "COM3 (Simüle)", "COM4 (Simüle)" };
        }

        public bool BaglantiyiAc(string portAdi, int baudRate = 115200)
        {
            _bagliMi = true;
            return true;
        }

        public void BaglantiyiKapat()
        {
            _bagliMi = false;
        }

        public void VeriYaz(string mesaj)
        {
            if (!_bagliMi) return;

            // Donanım Yanıtını Simüle Et
            Task.Run(async () => 
            {
                await Task.Delay(500); // İşleme süresini simüle et

                if (mesaj.StartsWith("REG_ID:"))
                {
                    // Simüle edilmiş ESP32 Mantığı: ID'yi al, kaydet ve onayla
                    // Örn: REG_ID:12|MAC:AA:BB:CC...
                    string idPart = mesaj.Split('|')[0];
                    string id = idPart.Split(':')[1];
                    VeriAlindi?.Invoke(this, $"REG_OK:{id}");
                }
                else if (mesaj.StartsWith("DEL_ID:"))
                {
                    string id = mesaj.Split(':')[1];
                    VeriAlindi?.Invoke(this, $"DEL_OK:{id}");
                }
                else if (mesaj.StartsWith("WIFI_SET:"))
                {
                    VeriAlindi?.Invoke(this, "WIFI_OK");
                }
                else if (mesaj.StartsWith("CFG_SET:"))
                {
                    VeriAlindi?.Invoke(this, "CFG_OK:SAVED");
                }
                else if (mesaj.StartsWith("CMD_PING:"))
                {
                    VeriAlindi?.Invoke(this, "PONG:ACTIVE");
                }
            });
        }
    }
}
