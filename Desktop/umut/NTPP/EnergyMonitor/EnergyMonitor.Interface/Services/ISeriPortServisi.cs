using System;

namespace EnergyMonitor.Interface.Services
{
    public interface ISeriPortServisi
    {
        bool BaglantiyiAc(string portAdi, int baudRate = 115200);
        void BaglantiyiKapat();
        bool AcikMi { get; }
        void VeriYaz(string veri);
        event EventHandler<string> VeriAlindi;
    }
}
