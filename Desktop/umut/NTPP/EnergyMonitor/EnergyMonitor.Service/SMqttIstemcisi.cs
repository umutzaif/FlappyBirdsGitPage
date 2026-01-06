using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using EnergyMonitor.Interface.Services;

namespace EnergyMonitor.Service
{
    public class SMqttIstemcisi : IMqttIstemcisi
    {
        private IMqttClient _mqttIstemcisi;
        private MqttClientOptions? _ayarlar;

        public event EventHandler<MqttMesajAlindiOlayArgumanlari>? MesajAlindi;

        public SMqttIstemcisi()
        {
            var fabrika = new MqttFactory();
            _mqttIstemcisi = fabrika.CreateMqttClient();
            
            _mqttIstemcisi.ApplicationMessageReceivedAsync += MesajGelinceIsle;
        }

        public async Task BaglanAsync(string brokerIp, int port)
        {
            _ayarlar = new MqttClientOptionsBuilder()
                .WithTcpServer(brokerIp, port)
                .WithClientId("EnergyMonitorMasaustuUygulamasi")
                .Build();

            if (!_mqttIstemcisi.IsConnected)
            {
                await _mqttIstemcisi.ConnectAsync(_ayarlar, CancellationToken.None);
            }
        }

        public async Task BaglantiyiKesAsync()
        {
            if (_mqttIstemcisi.IsConnected)
            {
                await _mqttIstemcisi.DisconnectAsync();
            }
        }

        public async Task AboneOlAsync(string konu)
        {
            if (!_mqttIstemcisi.IsConnected) return;

            var konuFiltresi = new MqttTopicFilterBuilder()
                .WithTopic(konu)
                .Build();

            await _mqttIstemcisi.SubscribeAsync(konuFiltresi);
        }

        private Task MesajGelinceIsle(MqttApplicationMessageReceivedEventArgs e)
        {
            var veri = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            var konu = e.ApplicationMessage.Topic;

            MesajAlindi?.Invoke(this, new MqttMesajAlindiOlayArgumanlari
            {
                Konu = konu,
                Veri = veri
            });

            return Task.CompletedTask;
        }
    }
}
