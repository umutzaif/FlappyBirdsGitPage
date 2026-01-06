-- Run this script in your PostgreSQL database (pgAdmin or psql) to fix the report error
ALTER TABLE sensor_readings ADD COLUMN IF NOT EXISTS telemetry_data JSONB;

-- Also update the PROCEDURE to match
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
