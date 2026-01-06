using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;

namespace EnergyMonitor.SP
{
    public class SpAnaliz
    {
        private readonly string _baglantiCumlesi;

        public SpAnaliz(string baglantiCumlesi)
        {
            _baglantiCumlesi = baglantiCumlesi;
        }

        public async Task<List<(DateTime Zaman, decimal Deger)>> GecmisVerileriGetirAsync(string sensorTuru, DateTime baslangic, DateTime bitis)
        {
            var sonuc = new List<(DateTime, decimal)>();

            using (var baglanti = new NpgsqlConnection(_baglantiCumlesi))
            {
                await baglanti.OpenAsync();
                using (var komut = new NpgsqlCommand("SELECT timestamp, value FROM sensor_readings WHERE sensor_type = @t AND timestamp BETWEEN @s AND @e ORDER BY timestamp", baglanti))
                {
                    komut.Parameters.AddWithValue("t", sensorTuru);
                    komut.Parameters.AddWithValue("s", baslangic);
                    komut.Parameters.AddWithValue("e", bitis);

                    using (var okuyucu = await komut.ExecuteReaderAsync())
                    {
                        while (await okuyucu.ReadAsync())
                        {
                            sonuc.Add((okuyucu.GetDateTime(0), okuyucu.GetDecimal(1)));
                        }
                    }
                }
            }
            return sonuc;
        }
    }
}
