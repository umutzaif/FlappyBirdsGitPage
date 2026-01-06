using System;
using System.Threading.Tasks;

namespace EnergyMonitor.Interface.Services
{
    public interface IMqttIstemcisi
    {
        Task BaglanAsync(string brokerIp, int port);
        Task BaglantiyiKesAsync();
        Task AboneOlAsync(string konu);
        event EventHandler<MqttMesajAlindiOlayArgumanlari> MesajAlindi;
    }

    public class MqttMesajAlindiOlayArgumanlari : EventArgs
    {
        public string Konu { get; set; } = string.Empty;
        public string Veri { get; set; } = string.Empty;
    }
}
