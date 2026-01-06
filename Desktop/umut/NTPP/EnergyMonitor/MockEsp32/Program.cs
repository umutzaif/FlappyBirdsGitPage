using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;

namespace MockEsp32
{
    class Program
    {
        private static IMqttClient? _mqttClient;
        private static MqttClientOptions? _options;
        private const string TOPIC = "sensor/energy";
        private static string EEPROM_FILE = "eeprom_sim.json";
        private static List<int> RegisteredDeviceIds = new List<int>();

        static async Task Main(string[] args)
        {
            // Load persistent state
            LoadState();
            Console.WriteLine($"[Hardware] Loaded {RegisteredDeviceIds.Count} devices from virtual EEPROM.");

            // Start Serial Listener in background
            _ = Task.Run(() => StartSerialListener());

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            _options = new MqttClientOptionsBuilder()
                .WithTcpServer("127.0.0.1", 1883)
                .WithClientId("MockEsp32Device")
                .Build();

            _mqttClient.DisconnectedAsync += async (e) =>
            {
                Console.WriteLine("Bağlantı koptu.");
                await Task.CompletedTask;
            };

            // Initial connection loop
            while (true)
            {
                try
                {
                    if (!_mqttClient.IsConnected)
                    {
                        Console.WriteLine("MQTT Broker'a bağlanılıyor...");
                        await _mqttClient.ConnectAsync(_options, CancellationToken.None);
                        Console.WriteLine("MQTT Broker'a bağlandı.");
                        break;
                    }
                }
                catch
                {
                    Console.WriteLine("Bağlantı başarısız. 5 saniye içinde tekrar denenecek...");
                    await Task.Delay(5000);
                }
            }

            var random = new Random();

            while (true)
            {
                try
                {
                    if (!_mqttClient.IsConnected)
                    {
                        Console.WriteLine("Yeniden bağlanılıyor...");
                        await _mqttClient.ConnectAsync(_options, CancellationToken.None);
                        Console.WriteLine("Yeniden bağlandı.");
                    }

                    // Simulate multiple devices (1: General, 2: Motor)
                    string[] deviceMacs = { "AA:BB:CC:11:22:33", "MM:OT:OR:00:11:22" };
                    
                    foreach (var mac in deviceMacs)
                    {
                        if (mac.StartsWith("MM")) // Simulate a Motor (Rotating)
                        {
                            var rpm = 1450 + random.Next(-10, 10);
                            var vibSamples = new List<double> { 
                                random.NextDouble() * 3.0, 
                                random.NextDouble() * 3.5, 
                                random.NextDouble() * 2.8 
                            };
                            
                            var rotatingData = new {
                                rpm = rpm,
                                vib_x = vibSamples[0],
                                vib_y = vibSamples[1],
                                vib_z = vibSamples[2],
                                temp = 45 + random.NextDouble() * 5
                            };

                            await PublishData(mac, "rotating_telemetry", 0, "JSON", rotatingData);
                        }
                        else 
                        {
                            // ONLY PUBLISH DATA FOR REGISTERED DEVICES (Except the hardcoded default for testing)
                            // Note: In a real scenario, we'd map registration to specific sensors.
                            
                             // Simulate basic readings
                            var current = Math.Round(random.NextDouble() * 10, 2); // 0-10 A
                            var voltage = Math.Round(220 + (random.NextDouble() * 10 - 5), 1); // 215-225 V
                            
                            await PublishData(mac, "current", (decimal)current, "A");
                            await Task.Delay(200);
                            await PublishData(mac, "voltage", (decimal)voltage, "V");
                        }
                        
                        await Task.Delay(1000); // Wait between devices
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata: {ex.Message}. 5 saniye bekleniyor...");
                    await Task.Delay(5000);
                }
            }
        }

        static void StartSerialListener()
        {
            // Note: In a real simulation, we might use a Virtual Serial Port (com0com)
            // For now, we'll just log that we are ready to receive commands if a port were open.
            Console.WriteLine("[Simulator] Serial listener standby (Simulating COM port responses).");
            
            // To actually test with FrmDeviceManager, we would need System.IO.Ports here
            // But since this is a mock console app, we will use a simplified simulation logic
            // that responds to simulated commands if we were to link them.
        }

        public static void SimulateCommand(string cmd)
        {
            if (cmd.StartsWith("REG_ID:"))
            {
                string idStr = cmd.Split(':')[1].Split('|')[0];
                if (int.TryParse(idStr, out int id))
                {
                    if (!RegisteredDeviceIds.Contains(id))
                    {
                        RegisteredDeviceIds.Add(id);
                        SaveState();
                        Console.WriteLine($"[Hardware] REGISTERED ID: {id} (Saved to EEPROM)");
                    }
                    Thread.Sleep(500);
                    Console.WriteLine($"REG_OK:{id}"); 
                }
            }
            else if (cmd.StartsWith("DEL_ID:"))
            {
                string idStr = cmd.Split(':')[1];
                if (int.TryParse(idStr, out int id))
                {
                    if (RegisteredDeviceIds.Contains(id))
                    {
                        RegisteredDeviceIds.Remove(id);
                        SaveState();
                        Console.WriteLine($"[Hardware] DELETED ID: {id} (Removed from EEPROM)");
                    }
                    Thread.Sleep(500);
                    Console.WriteLine($"DEL_OK:{id}");
                }
            }
            else if (cmd.StartsWith("WIFI_SET:"))
            {
                Console.WriteLine($"[Hardware] WiFi Configured: {cmd.Split(':')[1]}");
                Thread.Sleep(1000);
                Console.WriteLine("WIFI_OK");
            }
            else if (cmd.StartsWith("CFG_SET:"))
            {
                // Format: CFG_SET:id|period|sensors
                string parts = cmd.Split(':')[1];
                Console.WriteLine($"[Hardware] Applied Config to Node: {parts}");
                Thread.Sleep(800);
                Console.WriteLine($"CFG_OK:{parts.Split('|')[0]}");
            }
            else if (cmd.StartsWith("CMD_PING:"))
            {
                string id = cmd.Split(':')[1];
                Console.WriteLine($"[Hardware] Ping received for {id}");
                Thread.Sleep(200);
                Console.WriteLine($"PONG:{id}");
            }
            else if (cmd.Equals("NET_SCAN"))
            {
                Console.WriteLine("[Hardware] Scanning local IoT network...");
                Thread.Sleep(2000);
                Console.WriteLine("SCAN_COMPLETE:Found 2 Nodes, 1 Master");
            }
        }

        private static void LoadState()
        {
            try {
                if(File.Exists(EEPROM_FILE)) {
                    string json = File.ReadAllText(EEPROM_FILE);
                    RegisteredDeviceIds = JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
                }
            } catch { RegisteredDeviceIds = new List<int>(); }
        }

        private static void SaveState()
        {
            try {
                string json = JsonSerializer.Serialize(RegisteredDeviceIds);
                File.WriteAllText(EEPROM_FILE, json);
            } catch { }
        }

        static async Task PublishData(string mac, string sensor, decimal value, string unit, object? telemetry = null)
        {
            var data = new
            {
                mac_address = mac,
                sensor = sensor,
                value = value,
                unit = unit,
                timestamp = DateTime.UtcNow,
                telemetry = telemetry
            };

            var payload = JsonSerializer.Serialize(data);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(TOPIC)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            if (_mqttClient != null && _mqttClient.IsConnected)
            {
                await _mqttClient.PublishAsync(message);
                Console.WriteLine($"Veri gönderildi: {payload}");
            }
        }
    }
}
