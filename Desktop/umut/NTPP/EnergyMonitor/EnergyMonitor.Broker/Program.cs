using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Server;
using MQTTnet.Protocol;

namespace EnergyMonitor.Broker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            /*
             * Creating a standard MQTT broker using MQTTnet.
             * This avoids the need for external software installation (Mosquitto)
             * and keeps the solution self-contained.
             */

            var mqttFactory = new MqttFactory();

            // The options for the MQTT server
            var mqttServerOptions = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint() // Default port 1883
                .WithDefaultEndpointPort(1883)
                .Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                // Event handlers for logging
                mqttServer.ClientConnectedAsync += e =>
                {
                    Console.WriteLine($"Client Loaded: {e.ClientId}");
                    return Task.CompletedTask;
                };

                mqttServer.ClientDisconnectedAsync += e =>
                {
                    Console.WriteLine($"Client Disconnected: {e.ClientId}");
                    return Task.CompletedTask;
                };
                
                // Start the server
                await mqttServer.StartAsync();

                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine("Internal MQTT Broker Running on Port 1883");
                Console.WriteLine("Press any key to stop...");
                Console.WriteLine("---------------------------------------------------------");

                Console.ReadLine();

                await mqttServer.StopAsync();
            }
        }
    }
}
