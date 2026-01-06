DROP TABLE IF EXISTS sensor_readings;
DROP TABLE IF EXISTS devices;

-- Table: devices
-- Stores inventory of machines/devices to be monitored.
CREATE TABLE devices (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    brand VARCHAR(50),
    model VARCHAR(50),
    mac_address VARCHAR(50) UNIQUE, -- New Complex ID
    voltage_rating DECIMAL(10, 2), -- Volts
    current_rating DECIMAL(10, 2), -- Amps
    weight DECIMAL(10, 2),         -- kg
    entry_date DATE DEFAULT CURRENT_DATE,
    last_maintenance DATE,
    next_maintenance DATE,
    status INT DEFAULT 0, -- 0: Lobby, 1: Active, 2: Passive, 3: Trash
    device_type INT DEFAULT 0 -- 0: Static, 1: Rotating, 2: Thermal, 3: Power
);

-- Table: sensor_readings
-- Stores the sensor data received from ESP32 via MQTT.
CREATE TABLE sensor_readings (
    id SERIAL PRIMARY KEY,
    device_id INT REFERENCES devices(id), -- Linked to a specific device
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    sensor_type VARCHAR(50) NOT NULL, -- e.g., 'current', 'voltage', 'rotating_telemetry'
    value DECIMAL(10, 2), -- Primary value if applicable
    unit VARCHAR(20),
    telemetry_data JSONB, -- For complex data (Vibration, RPM, FFT, etc.)
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Index for faster time-range queries (Analysis API)
CREATE INDEX IF NOT EXISTS idx_sensor_readings_timestamp ON sensor_readings(timestamp);

-- Procedure: sp_insert_reading
CREATE OR REPLACE PROCEDURE sp_insert_reading(
    p_device_id INT,
    p_sensor_type VARCHAR,
    p_value DECIMAL,
    p_unit VARCHAR,
    p_timestamp TIMESTAMP WITH TIME ZONE,
    p_telemetry JSONB DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO sensor_readings (device_id, sensor_type, value, unit, timestamp, telemetry_data)
    VALUES (p_device_id, p_sensor_type, p_value, p_unit, p_timestamp, p_telemetry);
END;
$$;

-- Procedure: sp_insert_device
CREATE OR REPLACE PROCEDURE sp_insert_device(
    p_name VARCHAR,
    p_brand VARCHAR,
    p_model VARCHAR,
    p_mac VARCHAR,
    p_voltage DECIMAL,
    p_current DECIMAL,
    p_weight DECIMAL,
    p_entry_date DATE,
    p_last_maint DATE,
    p_next_maint DATE,
    p_status INT,
    p_type INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO devices (name, brand, model, mac_address, voltage_rating, current_rating, weight, entry_date, last_maintenance, next_maintenance, status, device_type)
    VALUES (p_name, p_brand, p_model, p_mac, p_voltage, p_current, p_weight, p_entry_date, p_last_maint, p_next_maint, p_status, p_type);
END;
$$;

-- Procedure: sp_update_device
CREATE OR REPLACE PROCEDURE sp_update_device(
    p_id INT,
    p_name VARCHAR,
    p_brand VARCHAR,
    p_model VARCHAR,
    p_mac VARCHAR,
    p_voltage DECIMAL,
    p_current DECIMAL,
    p_weight DECIMAL,
    p_entry_date DATE,
    p_last_maint DATE,
    p_next_maint DATE,
    p_status INT,
    p_type INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE devices 
    SET name = p_name, brand = p_brand, model = p_model, mac_address = p_mac,
        voltage_rating = p_voltage, current_rating = p_current, weight = p_weight,
        entry_date = p_entry_date, last_maintenance = p_last_maint, next_maintenance = p_next_maint,
        status = p_status, device_type = p_type
    WHERE id = p_id;
END;
$$;

-- Procedure: sp_update_device_status
CREATE OR REPLACE PROCEDURE sp_update_device_status(
    p_id INT,
    p_s INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE devices SET status = p_s WHERE id = p_id;
END;
$$;

-- Procedure: sp_delete_device
CREATE OR REPLACE PROCEDURE sp_delete_device(p_id INT)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM devices WHERE id = p_id;
END;
$$;

-- Seed Data (Optional: Default Device for Mock Data)
INSERT INTO devices (name, brand, model, voltage_rating, current_rating, weight, entry_date)
VALUES ('Main Generator', 'Siemens', 'V200', 220.0, 10.0, 50.0, CURRENT_DATE);

---------------------------------------------------------
-- AUTHENTICATION SCHEMA
---------------------------------------------------------
CREATE TABLE IF NOT EXISTS roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL,
    description TEXT
);

CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role_id INT REFERENCES roles(id),
    full_name VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Seed Roles
INSERT INTO roles (name, description) VALUES 
('Admin', 'Tam Yetki: İzleme, Raporlama, Kullanıcı Yönetimi'),
('Teknik_Personel', 'Cihaz Yönetimi, Raporlama, İzleme'),
('Muhasebe_Personeli', 'Bütçe, Z-Raporu, Yeşil Rapor'),
('Izleme_Personeli', 'Sadece Canlı İzleme')
ON CONFLICT (name) DO NOTHING;

-- Seed Default Admin (Password: 1234)
INSERT INTO users (username, password_hash, role_id, full_name) 
VALUES ('admin', '1234', (SELECT id FROM roles WHERE name='Admin'), 'Sistem Yöneticisi')
ON CONFLICT (username) DO NOTHING;
