using System;
using System.Threading.Tasks;
using Dapper;
using EnergyMonitor.Interface.Models;
using EnergyMonitor.Interface.Services;
using Npgsql;

namespace EnergyMonitor.Service
{
    public class SIsletme : IIsletmeServisi
    {
        private readonly string _baglantiCumlesi;

        public SIsletme(string baglantiCumlesi)
        {
            _baglantiCumlesi = baglantiCumlesi;
            _ = SutunlariKontrolEtAsync(); // Açılışta göç testi (Migration check)
        }

        private async Task SutunlariKontrolEtAsync()
        {
            try
            {
                using (var baglanti = new NpgsqlConnection(_baglantiCumlesi))
                {
                    await baglanti.OpenAsync();
                    await baglanti.ExecuteAsync("ALTER TABLE business_profile ADD COLUMN IF NOT EXISTS gemini_api_key TEXT");
                    await baglanti.ExecuteAsync("ALTER TABLE business_profile ADD COLUMN IF NOT EXISTS unit_cost NUMERIC DEFAULT 2.5");
                    await baglanti.ExecuteAsync("ALTER TABLE business_profile ADD COLUMN IF NOT EXISTS tax_rate NUMERIC DEFAULT 18.0");
                    await baglanti.ExecuteAsync("ALTER TABLE business_profile ADD COLUMN IF NOT EXISTS subscriber_no TEXT");
                    await baglanti.ExecuteAsync("ALTER TABLE business_profile ADD COLUMN IF NOT EXISTS customer_no TEXT");
                }
            }
            catch (Exception) { /* Veritabanı seviyesinde hatalar raporlanıyor */ }
        }

        public async Task<IsletmeBilgisi?> ProfilGetirAsync(int isletmeId)
        {
            await SutunlariKontrolEtAsync();
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            var sql = @"SELECT id as Id, company_name as SirketAdi, establishment_date as KurulusTarihi, 
                               budget_limit as ButceLimiti, gemini_api_key as GeminiApiAnahtari, 
                               unit_cost as BirimMaliyet, tax_rate as VergiOrani, 
                               subscriber_no as AboneNo, customer_no as MusteriNo 
                        FROM business_profile LIMIT 1";
            var sonuc = await baglanti.QueryFirstOrDefaultAsync<IsletmeBilgisi>(sql);
            return sonuc ?? new IsletmeBilgisi();
        }

        public async Task<bool> ProfilKaydetAsync(IsletmeBilgisi profil)
        {
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            var sql = @"UPDATE business_profile SET 
                        company_name = @SirketAdi, 
                        establishment_date = @KurulusTarihi,
                        budget_limit = @ButceLimiti,
                        gemini_api_key = @GeminiApiAnahtari,
                        unit_cost = @BirimMaliyet,
                        tax_rate = @VergiOrani,
                        subscriber_no = @AboneNo,
                        customer_no = @MusteriNo
                        WHERE id = @Id";
            
            var etkilenenSatir = await baglanti.ExecuteAsync(sql, profil);
            if (etkilenenSatir == 0)
            {
                var ekleSql = @"INSERT INTO business_profile (company_name, establishment_date, budget_limit, gemini_api_key, unit_cost, tax_rate, subscriber_no, customer_no)
                                VALUES (@SirketAdi, @KurulusTarihi, @ButceLimiti, @GeminiApiAnahtari, @BirimMaliyet, @VergiOrani, @AboneNo, @MusteriNo)";
                await baglanti.ExecuteAsync(ekleSql, profil);
            }
            return true;
        }
    }
}
