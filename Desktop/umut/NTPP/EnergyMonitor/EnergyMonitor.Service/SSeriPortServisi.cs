using System;
using System.IO.Ports;
using EnergyMonitor.Interface.Services;

namespace EnergyMonitor.Service
{
    public class SSeriPortServisi : ISeriPortServisi
    {
        private SerialPort? _seriPort;

        public event EventHandler<string>? VeriAlindi;

        public bool AcikMi => _seriPort != null && _seriPort.IsOpen;

        public string[] MevcutPortlariGetir()
        {
            return SerialPort.GetPortNames();
        }

        public bool BaglantiyiAc(string portAdi, int baudRate = 115200)
        {
            if (AcikMi) BaglantiyiKapat();

            try
            {
                _seriPort = new SerialPort(portAdi, baudRate);
                _seriPort.DtrEnable = true; // Bazı ESP32 kartları için gerekli
                _seriPort.RtsEnable = true; 
                _seriPort.DataReceived += SeriPort_VeriAlindi;
                _seriPort.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seri Port Hatası: {ex.Message}");
                return false;
            }
        }

        public void BaglantiyiKapat()
        {
            if (_seriPort != null)
            {
                if (_seriPort.IsOpen)
                {
                    _seriPort.Close();
                }
                _seriPort.DataReceived -= SeriPort_VeriAlindi;
                _seriPort.Dispose();
                _seriPort = null;
            }
        }

        public void VeriYaz(string veri)
        {
            if (AcikMi && _seriPort != null)
            {
                try
                {
                    _seriPort.WriteLine(veri);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Yazma Hatası: {ex.Message}");
                }
            }
        }

        private void SeriPort_VeriAlindi(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_seriPort != null && _seriPort.IsOpen)
                {
                    string veri = _seriPort.ReadLine();
                    VeriAlindi?.Invoke(this, veri);
                }
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"Okuma Hatası: {ex.Message}");
            }
        }
    }
}
