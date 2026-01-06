using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using EnergyMonitor.Interface.Models;
using EnergyMonitor.Interface.Services;
using Npgsql;

namespace EnergyMonitor.Service
{
    public class SMesajServisi : IMesajServisi
    {
        private readonly string _baglantiCumlesi;

        public SMesajServisi(string baglantiCumlesi)
        {
            _baglantiCumlesi = baglantiCumlesi;
        }

        public async Task<bool> MesajGonderAsync(Mesaj mesaj)
        {
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            var sql = @"INSERT INTO messages (sender_id, receiver_id, content, sent_at) 
                        VALUES (@GonderenId, @AliciId, @Icerik, @GonderilmeTarihi)";
            await baglanti.ExecuteAsync(sql, mesaj);
            return true;
        }

        public async Task<List<Mesaj>> MesajlariGetirAsync(int kullaniciId)
        {
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            var sql = @"SELECT m.id as Id, m.sender_id as GonderenId, m.receiver_id as AliciId, m.content as Icerik, m.sent_at as GonderilmeTarihi, m.is_read as OkunduMu,
                               u1.full_name as GonderenAdi
                        FROM messages m
                        JOIN users u1 ON m.sender_id = u1.id
                        WHERE m.receiver_id = @kullaniciId OR m.sender_id = @kullaniciId
                        ORDER BY m.sent_at ASC";
            var sonuc = await baglanti.QueryAsync<Mesaj>(sql, new { kullaniciId });
            return new List<Mesaj>(sonuc);
        }

        public async Task OkunduOlarakIsaretleAsync(int mesajId)
        {
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            await baglanti.ExecuteAsync("UPDATE messages SET is_read = TRUE WHERE id = @mesajId", new { mesajId });
        }

        public async Task MesajSilAsync(int mesajId)
        {
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            await baglanti.ExecuteAsync("DELETE FROM messages WHERE id = @mesajId", new { mesajId });
        }
    }
}
