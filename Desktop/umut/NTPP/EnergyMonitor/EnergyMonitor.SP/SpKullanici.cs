using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using EnergyMonitor.Interface.Models;
using Npgsql;

namespace EnergyMonitor.SP
{
    public class SpKullanici
    {
        private readonly string _baglantiCumlesi;

        public SpKullanici(string baglantiCumlesi)
        {
            _baglantiCumlesi = baglantiCumlesi;
        }

        public async Task SemayiHazirlaAsync()
        {
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            await baglanti.OpenAsync();

            // 1. Roller ve Kullanıcılar
            await baglanti.ExecuteAsync("CREATE TABLE IF NOT EXISTS roles (id SERIAL PRIMARY KEY, name VARCHAR(50) UNIQUE NOT NULL, description TEXT)");
            await baglanti.ExecuteAsync("CREATE TABLE IF NOT EXISTS users (id SERIAL PRIMARY KEY, username VARCHAR(50) UNIQUE NOT NULL, password_hash VARCHAR(255) NOT NULL, role_id INT REFERENCES roles(id))");
            await SutunEkleEgerYoksa(baglanti, "users", "full_name", "VARCHAR(100)");
            await SutunEkleEgerYoksa(baglanti, "users", "created_at", "TIMESTAMP DEFAULT CURRENT_TIMESTAMP");
            await SutunEkleEgerYoksa(baglanti, "users", "is_active", "BOOLEAN DEFAULT FALSE");
            await SutunEkleEgerYoksa(baglanti, "users", "business_id", "INT");

            // 2. İşletme Profili
            await baglanti.ExecuteAsync("CREATE TABLE IF NOT EXISTS business_profile (id SERIAL PRIMARY KEY)");
            await SutunEkleEgerYoksa(baglanti, "business_profile", "company_name", "VARCHAR(255)");
            await SutunEkleEgerYoksa(baglanti, "business_profile", "gemini_api_key", "TEXT");
            await SutunEkleEgerYoksa(baglanti, "business_profile", "establishment_date", "DATE DEFAULT CURRENT_DATE");
            await SutunEkleEgerYoksa(baglanti, "business_profile", "budget_limit", "DECIMAL(18, 2) DEFAULT 0");
            await SutunEkleEgerYoksa(baglanti, "business_profile", "currency", "VARCHAR(10) DEFAULT 'TL'");
            await SutunEkleEgerYoksa(baglanti, "business_profile", "unit_cost", "DECIMAL(10, 4) DEFAULT 1.5");
            await SutunEkleEgerYoksa(baglanti, "business_profile", "tax_rate", "DECIMAL(5, 2) DEFAULT 18.0");
            await SutunEkleEgerYoksa(baglanti, "business_profile", "subscriber_no", "VARCHAR(50)");
            await SutunEkleEgerYoksa(baglanti, "business_profile", "customer_no", "VARCHAR(50)");

            // 3. Cihazlar
            await baglanti.ExecuteAsync("CREATE TABLE IF NOT EXISTS devices (id SERIAL PRIMARY KEY)");
            await SutunEkleEgerYoksa(baglanti, "devices", "name", "VARCHAR(100)");
            await SutunEkleEgerYoksa(baglanti, "devices", "brand", "VARCHAR(50)");
            await SutunEkleEgerYoksa(baglanti, "devices", "model", "VARCHAR(50)");
            await SutunEkleEgerYoksa(baglanti, "devices", "mac_address", "VARCHAR(50)");
            await SutunEkleEgerYoksa(baglanti, "devices", "voltage_rating", "DECIMAL(10, 2)");
            await SutunEkleEgerYoksa(baglanti, "devices", "current_rating", "DECIMAL(10, 2)");
            await SutunEkleEgerYoksa(baglanti, "devices", "weight", "DECIMAL(10, 2)");
            await SutunEkleEgerYoksa(baglanti, "devices", "entry_date", "DATE DEFAULT CURRENT_DATE");
            await SutunEkleEgerYoksa(baglanti, "devices", "last_maintenance", "DATE");
            await SutunEkleEgerYoksa(baglanti, "devices", "next_maintenance", "DATE");
            await SutunEkleEgerYoksa(baglanti, "devices", "status", "INT DEFAULT 0");
            await SutunEkleEgerYoksa(baglanti, "devices", "device_type", "INT DEFAULT 0");
            
            // IoT Sütunları
            await SutunEkleEgerYoksa(baglanti, "devices", "wifi_ssid", "VARCHAR(100)");
            await SutunEkleEgerYoksa(baglanti, "devices", "wifi_password", "VARCHAR(100)");
            await SutunEkleEgerYoksa(baglanti, "devices", "data_period", "INT DEFAULT 5000");
            await SutunEkleEgerYoksa(baglanti, "devices", "sensor_selection", "VARCHAR(255) DEFAULT 'ALL'");
            await SutunEkleEgerYoksa(baglanti, "devices", "is_master", "BOOLEAN DEFAULT FALSE");

            // 4. Sensör Verileri
            await baglanti.ExecuteAsync("CREATE TABLE IF NOT EXISTS sensor_readings (id SERIAL PRIMARY KEY)");
            await SutunEkleEgerYoksa(baglanti, "sensor_readings", "device_id", "INT REFERENCES devices(id)");
            await SutunEkleEgerYoksa(baglanti, "sensor_readings", "timestamp", "TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP");
            await SutunEkleEgerYoksa(baglanti, "sensor_readings", "sensor_type", "VARCHAR(50) NOT NULL DEFAULT 'unknown'");
            await SutunEkleEgerYoksa(baglanti, "sensor_readings", "value", "DECIMAL(10, 2)");
            await SutunEkleEgerYoksa(baglanti, "sensor_readings", "unit", "VARCHAR(20)");
            await SutunEkleEgerYoksa(baglanti, "sensor_readings", "telemetry_data", "JSONB");
            
            // 5. Mesajlaşma
            await baglanti.ExecuteAsync("CREATE TABLE IF NOT EXISTS messages (id SERIAL PRIMARY KEY, sender_id INT REFERENCES users(id), receiver_id INT REFERENCES users(id), content TEXT NOT NULL, sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, is_read BOOLEAN DEFAULT FALSE)");

            // 6. Örnek Veri (Seed)
            await baglanti.ExecuteAsync("INSERT INTO roles (name, description) VALUES ('Yonetici', 'Tam Yetki') ON CONFLICT (name) DO NOTHING");
            await baglanti.ExecuteAsync("INSERT INTO users (username, password_hash, role_id, full_name, is_active) VALUES ('admin', '1234', (SELECT id FROM roles WHERE name='Yonetici' LIMIT 1), 'Sistem Yöneticisi', TRUE) ON CONFLICT (username) DO NOTHING");
            await baglanti.ExecuteAsync("UPDATE users SET is_active = TRUE WHERE username = 'admin'");
        }

        private async Task SutunEkleEgerYoksa(NpgsqlConnection baglanti, string tablo, string sutun, string tip)
        {
            string sql = $@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='{tablo}' AND column_name='{sutun}') THEN 
                        ALTER TABLE {tablo} ADD COLUMN {sutun} {tip}; 
                    END IF; 
                END $$;";
            await baglanti.ExecuteAsync(sql);
        }

        public async Task<Kullanici?> GirisYapAsync(string kullaniciAdi, string sifre)
        {
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            var sql = @"
                SELECT u.id as Id, u.username as KullaniciAdi, u.password_hash as SifreOzeti,
                       u.full_name as AdSoyad, u.created_at as OlusturulmaTarihi,
                       u.is_active as AktifMi, u.business_id as IsletmeId,
                       r.name as RolAdi, b.company_name as IsletmeAdi
                FROM users u 
                LEFT JOIN roles r ON u.role_id = r.id 
                LEFT JOIN business_profile b ON u.business_id = b.id
                WHERE u.username = @kullaniciAdi AND u.password_hash = @sifre AND u.is_active = TRUE";
            
            return await baglanti.QueryFirstOrDefaultAsync<Kullanici>(sql, new { kullaniciAdi, sifre });
        }

        public async Task<IEnumerable<Rol>> TumRolleriGetirAsync()
        {
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            return await baglanti.QueryAsync<Rol>("SELECT id as Id, name as Ad, description as Aciklama FROM roles");
        }
        
        public async Task<IEnumerable<Kullanici>> TumKullanicilariGetirAsync()
        {
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            var sql = @"
                SELECT u.id as Id, u.username as KullaniciAdi, u.password_hash as SifreOzeti, 
                       u.full_name as AdSoyad, u.created_at as OlusturulmaTarihi,
                       u.is_active as AktifMi, u.business_id as IsletmeId,
                       r.name as RolAdi, b.company_name as IsletmeAdi
                FROM users u 
                LEFT JOIN roles r ON u.role_id = r.id
                LEFT JOIN business_profile b ON u.business_id = b.id
                ORDER BY u.id";
            return await baglanti.QueryAsync<Kullanici>(sql);
        }

        public async Task KullaniciEkleAsync(Kullanici kullanici)
        {
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            var sql = @"
                INSERT INTO users (username, password_hash, role_id, full_name, is_active, business_id)
                VALUES (@KullaniciAdi, @SifreOzeti, (SELECT id FROM roles WHERE name = @RolAdi), @AdSoyad, @AktifMi, @IsletmeId)";
            
            await baglanti.ExecuteAsync(sql, kullanici);
        }

        public async Task<IEnumerable<IsletmeBilgisi>> IsletmeleriGetirAsync()
        {
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            return await baglanti.QueryAsync<IsletmeBilgisi>("SELECT id as Id, company_name as SirketAdi FROM business_profile");
        }

        public async Task KullaniciGuncelleAsync(Kullanici kullanici)
        {
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            var sql = @"
                UPDATE users 
                SET username = @KullaniciAdi, 
                    full_name = @AdSoyad,
                    is_active = @AktifMi,
                    business_id = @IsletmeId,
                    role_id = (SELECT id FROM roles WHERE name = @RolAdi)
                WHERE id = @Id";
            
             if (!string.IsNullOrEmpty(kullanici.SifreOzeti))
             {
                 sql = @"UPDATE users 
                         SET username = @KullaniciAdi, 
                             full_name = @AdSoyad,
                             password_hash = @SifreOzeti,
                             is_active = @AktifMi,
                             business_id = @IsletmeId,
                             role_id = (SELECT id FROM roles WHERE name = @RolAdi)
                         WHERE id = @Id";
             }

            await baglanti.ExecuteAsync(sql, kullanici);
        }

        public async Task KullaniciSilAsync(int id)
        {
            using var baglanti = new NpgsqlConnection(_baglantiCumlesi);
            await baglanti.ExecuteAsync("DELETE FROM users WHERE id = @id", new { id });
        }
    }
}
