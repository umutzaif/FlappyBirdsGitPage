-- RUN THIS SCRIPT TO FIX SAVE ERRORS AND REPORT ERRORS
-- It adds missing columns and updates stored procedures to match the C# code signatures.

-- 1. Fix sensor_readings table
ALTER TABLE sensor_readings ADD COLUMN IF NOT EXISTS telemetry_data JSONB;

-- 2. Update sp_insert_reading
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

-- 3. Ensure devices table has new columns
ALTER TABLE devices ADD COLUMN IF NOT EXISTS status INT DEFAULT 0;
ALTER TABLE devices ADD COLUMN IF NOT EXISTS device_type INT DEFAULT 0;
ALTER TABLE devices ADD COLUMN IF NOT EXISTS wifi_ssid VARCHAR(100);
ALTER TABLE devices ADD COLUMN IF NOT EXISTS wifi_password VARCHAR(100);
ALTER TABLE devices ADD COLUMN IF NOT EXISTS data_period INT DEFAULT 5000;
ALTER TABLE devices ADD COLUMN IF NOT EXISTS sensor_selection VARCHAR(255) DEFAULT 'ALL';
ALTER TABLE devices ADD COLUMN IF NOT EXISTS is_master BOOLEAN DEFAULT FALSE;

-- 4. Fix sp_insert_device (12 parameters + p_type)
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

-- 5. Fix sp_update_device (13 parameters + p_type)
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
