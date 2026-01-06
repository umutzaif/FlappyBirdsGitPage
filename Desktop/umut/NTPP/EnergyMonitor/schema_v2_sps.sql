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
    p_status INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO devices (name, brand, model, mac_address, voltage_rating, current_rating, weight, entry_date, last_maintenance, next_maintenance, status)
    VALUES (p_name, p_brand, p_model, p_mac, p_voltage, p_current, p_weight, p_entry_date, p_last_maint, p_next_maint, p_status);
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
    p_status INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE devices 
    SET name = p_name, brand = p_brand, model = p_model, mac_address = p_mac,
        voltage_rating = p_voltage, current_rating = p_current, weight = p_weight,
        entry_date = p_entry_date, last_maintenance = p_last_maint, next_maintenance = p_next_maint, status = p_status
    WHERE id = p_id;
END;
$$;
