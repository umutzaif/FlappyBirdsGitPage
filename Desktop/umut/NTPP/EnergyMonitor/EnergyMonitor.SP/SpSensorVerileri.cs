using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyMonitor.Interface.Models;
using Npgsql;

namespace EnergyMonitor.SP
{
    public class SpSensorVerileri
    {
        private readonly string _baglantiCumlesi;

        public SpSensorVerileri(string baglantiCumlesi)
        {
            _baglantiCumlesi = baglantiCumlesi;
        }

        public async Task VeriEkleAsync(int cihazId, string sensorTuru, decimal? deger, string? birim, string? telemetriVerisi = null)
        {
            using (var baglanti = new NpgsqlConnection(_baglantiCumlesi))
            {
                await baglanti.OpenAsync();
                
                string sql = @"INSERT INTO sensor_readings (device_id, sensor_type, value, unit, timestamp, telemetry_data)
                               VALUES (@p0, @p1, @p2, @p3, @p4, @p5)";
                
                using (var komut = new NpgsqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("p0", cihazId);
                    komut.Parameters.AddWithValue("p1", sensorTuru);
                    komut.Parameters.AddWithValue("p2", (object?)deger ?? DBNull.Value);
                    komut.Parameters.AddWithValue("p3", (object?)birim ?? DBNull.Value);
                    komut.Parameters.AddWithValue("p4", DateTime.Now);
                    komut.Parameters.Add(new NpgsqlParameter("p5", NpgsqlTypes.NpgsqlDbType.Jsonb) { Value = (object?)telemetriVerisi ?? DBNull.Value });

                    await komut.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<SensorVerisi>> VerileriGetirAsync(List<int> cihazIdleri, DateTime baslangic, DateTime bitis)
        {
            var sonuc = new List<SensorVerisi>();
            using (var baglanti = new NpgsqlConnection(_baglantiCumlesi))
            {
                await baglanti.OpenAsync();
                
                string sorgu = "SELECT device_id, sensor_type, value, unit, timestamp, telemetry_data FROM sensor_readings WHERE timestamp BETWEEN @start AND @end";
                
                if (cihazIdleri != null && cihazIdleri.Count > 0)
                {
                    sorgu += " AND device_id = ANY(@deviceIds)";
                }
                
                sorgu += " ORDER BY timestamp DESC";

                using (var komut = new NpgsqlCommand(sorgu, baglanti))
                {
                    komut.Parameters.AddWithValue("start", baslangic);
                    komut.Parameters.AddWithValue("end", bitis);
                    if (cihazIdleri != null && cihazIdleri.Count > 0)
                    {
                        komut.Parameters.AddWithValue("deviceIds", cihazIdleri.ToArray());
                    }

                    using (var okuyucu = await komut.ExecuteReaderAsync())
                    {
                        while (await okuyucu.ReadAsync())
                        {
                            sonuc.Add(new SensorVerisi
                            {
                                CihazId = okuyucu.GetInt32(0),
                                SensorTuru = okuyucu.GetString(1),
                                Deger = okuyucu.IsDBNull(2) ? 0 : okuyucu.GetDecimal(2),
                                Birim = okuyucu.IsDBNull(3) ? string.Empty : okuyucu.GetString(3),
                                ZamanDamgasi = okuyucu.GetDateTime(4),
                                TelemetriVerisi = okuyucu.IsDBNull(5) ? null : okuyucu.GetString(5)
                            });
                        }
                    }
                }
            }
            return sonuc;
        }
    }
}
