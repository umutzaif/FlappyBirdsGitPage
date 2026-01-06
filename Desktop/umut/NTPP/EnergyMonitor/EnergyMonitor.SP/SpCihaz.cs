using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyMonitor.Interface.Models;
using Npgsql;
using NpgsqlTypes;

namespace EnergyMonitor.SP
{
    public class SpCihaz
    {
        private readonly string _baglantiCumlesi;

        public SpCihaz(string baglantiCumlesi)
        {
            _baglantiCumlesi = baglantiCumlesi;
        }

        public async Task<List<Cihaz>> TumCihazlariGetirAsync()
        {
            var liste = new List<Cihaz>();
            using (var baglanti = new NpgsqlConnection(_baglantiCumlesi))
            {
                await baglanti.OpenAsync();
                using (var komut = new NpgsqlCommand("SELECT * FROM devices", baglanti))
                {
                    using (var okuyucu = await komut.ExecuteReaderAsync())
                    {
                        while (await okuyucu.ReadAsync())
                        {
                            liste.Add(new Cihaz
                            {
                                Id = okuyucu.GetInt32(okuyucu.GetOrdinal("id")),
                                Ad = okuyucu.GetString(okuyucu.GetOrdinal("name")),
                                Marka = MetinGetir(okuyucu, "brand"),
                                Model = MetinGetir(okuyucu, "model"),
                                MacAdresi = MetinGetir(okuyucu, "mac_address"),
                                GerilimDegeri = SayisalGetir(okuyucu, "voltage_rating"),
                                AkimDegeri = SayisalGetir(okuyucu, "current_rating"),
                                Agirlik = SayisalGetir(okuyucu, "weight"),
                                GirisTarihi = TarihGetir(okuyucu, "entry_date"),
                                SonBakim = TarihGetir(okuyucu, "last_maintenance"),
                                SonrakiBakim = TarihGetir(okuyucu, "next_maintenance"),
                                Durum = TamSayiGetir(okuyucu, "status", 0),
                                Tur = (CihazTuru)TamSayiGetir(okuyucu, "device_type", 0),
                                
                                // IoT Sütunları
                                WifiAd = MetinGetir(okuyucu, "wifi_ssid"),
                                WifiSifre = MetinGetir(okuyucu, "wifi_password"),
                                VeriPeriyodu = TamSayiGetir(okuyucu, "data_period", 5000),
                                SensorSecimi = MetinGetir(okuyucu, "sensor_selection") ?? "HEPSI",
                                AnaCihazMi = MantiksalGetir(okuyucu, "is_master")
                            });
                        }
                    }
                }
            }
            return liste;
        }

        public async Task CihazEkleAsync(Cihaz cihaz)
        {
            using (var baglanti = new NpgsqlConnection(_baglantiCumlesi))
            {
                await baglanti.OpenAsync();
                var sql = @"INSERT INTO devices (name, brand, model, mac_address, voltage_rating, current_rating, weight, entry_date, last_maintenance, next_maintenance, status, device_type)
                            VALUES (@n, @b, @m, @mac, @v, @c, @w, @e, @l, @nx, @s, @t)";
                using (var komut = new NpgsqlCommand(sql, baglanti))
                {
                    komut.Parameters.Add(new NpgsqlParameter("n", NpgsqlDbType.Varchar) { Value = cihaz.Ad });
                    komut.Parameters.Add(new NpgsqlParameter("b", NpgsqlDbType.Varchar) { Value = (object?)cihaz.Marka ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("m", NpgsqlDbType.Varchar) { Value = (object?)cihaz.Model ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("mac", NpgsqlDbType.Varchar) { Value = (object?)cihaz.MacAdresi ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("v", NpgsqlDbType.Numeric) { Value = (object?)cihaz.GerilimDegeri ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("c", NpgsqlDbType.Numeric) { Value = (object?)cihaz.AkimDegeri ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("w", NpgsqlDbType.Numeric) { Value = (object?)cihaz.Agirlik ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("e", NpgsqlDbType.Date) { Value = (object?)cihaz.GirisTarihi ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("l", NpgsqlDbType.Date) { Value = (object?)cihaz.SonBakim ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("nx", NpgsqlDbType.Date) { Value = (object?)cihaz.SonrakiBakim ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("s", NpgsqlDbType.Integer) { Value = cihaz.Durum });
                    komut.Parameters.Add(new NpgsqlParameter("t", NpgsqlDbType.Integer) { Value = (int)cihaz.Tur });
                    await komut.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task CihazGuncelleAsync(Cihaz cihaz)
        {
            using (var baglanti = new NpgsqlConnection(_baglantiCumlesi))
            {
                await baglanti.OpenAsync();
                var sql = @"UPDATE devices SET 
                            name = @n, brand = @b, model = @m, mac_address = @mac,
                            voltage_rating = @v, current_rating = @c, weight = @w,
                            entry_date = @e, last_maintenance = @l, next_maintenance = @nx,
                            status = @s, device_type = @t
                            WHERE id = @id";
                using (var komut = new NpgsqlCommand(sql, baglanti))
                {
                    komut.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Integer) { Value = cihaz.Id });
                    komut.Parameters.Add(new NpgsqlParameter("n", NpgsqlDbType.Varchar) { Value = cihaz.Ad });
                    komut.Parameters.Add(new NpgsqlParameter("b", NpgsqlDbType.Varchar) { Value = (object?)cihaz.Marka ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("m", NpgsqlDbType.Varchar) { Value = (object?)cihaz.Model ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("mac", NpgsqlDbType.Varchar) { Value = (object?)cihaz.MacAdresi ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("v", NpgsqlDbType.Numeric) { Value = (object?)cihaz.GerilimDegeri ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("c", NpgsqlDbType.Numeric) { Value = (object?)cihaz.AkimDegeri ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("w", NpgsqlDbType.Numeric) { Value = (object?)cihaz.Agirlik ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("e", NpgsqlDbType.Date) { Value = (object?)cihaz.GirisTarihi ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("l", NpgsqlDbType.Date) { Value = (object?)cihaz.SonBakim ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("nx", NpgsqlDbType.Date) { Value = (object?)cihaz.SonrakiBakim ?? DBNull.Value });
                    komut.Parameters.Add(new NpgsqlParameter("s", NpgsqlDbType.Integer) { Value = cihaz.Durum });
                    komut.Parameters.Add(new NpgsqlParameter("t", NpgsqlDbType.Integer) { Value = (int)cihaz.Tur });
                    await komut.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task CihazSilAsync(int id)
        {
            using (var baglanti = new NpgsqlConnection(_baglantiCumlesi))
            {
                await baglanti.OpenAsync();
                using (var komut = new NpgsqlCommand("DELETE FROM devices WHERE id = @id", baglanti))
                {
                    komut.Parameters.AddWithValue("id", id);
                    await komut.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task CihazDurumuGuncelleAsync(int id, int durum)
        {
            using (var baglanti = new NpgsqlConnection(_baglantiCumlesi))
            {
                await baglanti.OpenAsync();
                using (var komut = new NpgsqlCommand("UPDATE devices SET status = @s WHERE id = @id", baglanti))
                {
                    komut.Parameters.AddWithValue("id", id);
                    komut.Parameters.AddWithValue("s", durum);
                    await komut.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task IotAyarlariniGuncelleAsync(Cihaz cihaz)
        {
            using (var baglanti = new NpgsqlConnection(_baglantiCumlesi))
            {
                await baglanti.OpenAsync();
                var sql = @"UPDATE devices SET 
                            wifi_ssid = @s, 
                            wifi_password = @p, 
                            data_period = @per, 
                            sensor_selection = @sel, 
                            is_master = @m 
                            WHERE id = @id";
                using (var komut = new NpgsqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("s", (object?)cihaz.WifiAd ?? DBNull.Value);
                    komut.Parameters.AddWithValue("p", (object?)cihaz.WifiSifre ?? DBNull.Value);
                    komut.Parameters.AddWithValue("per", cihaz.VeriPeriyodu);
                    komut.Parameters.AddWithValue("sel", cihaz.SensorSecimi);
                    komut.Parameters.AddWithValue("m", cihaz.AnaCihazMi);
                    komut.Parameters.AddWithValue("id", cihaz.Id);
                    await komut.ExecuteNonQueryAsync();
                }
            }
        }

        private string? MetinGetir(NpgsqlDataReader okuyucu, string kolon) {
            try { int i = okuyucu.GetOrdinal(kolon); return okuyucu.IsDBNull(i) ? null : okuyucu.GetString(i); } catch { return null; }
        }
        private int TamSayiGetir(NpgsqlDataReader okuyucu, string kolon, int varsayilan) {
            try { int i = okuyucu.GetOrdinal(kolon); return okuyucu.IsDBNull(i) ? varsayilan : okuyucu.GetInt32(i); } catch { return varsayilan; }
        }
        private bool MantiksalGetir(NpgsqlDataReader okuyucu, string kolon) {
            try { int i = okuyucu.GetOrdinal(kolon); return okuyucu.IsDBNull(i) ? false : okuyucu.GetBoolean(i); } catch { return false; }
        }
        private decimal? SayisalGetir(NpgsqlDataReader okuyucu, string kolon) {
            try { int i = okuyucu.GetOrdinal(kolon); return okuyucu.IsDBNull(i) ? null : okuyucu.GetDecimal(i); } catch { return null; }
        }
        private DateTime? TarihGetir(NpgsqlDataReader okuyucu, string kolon) {
            try { int i = okuyucu.GetOrdinal(kolon); return okuyucu.IsDBNull(i) ? null : okuyucu.GetDateTime(i); } catch { return null; }
        }
    }
}
