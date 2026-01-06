CREATE OR REPLACE PROCEDURE sp_update_device_status(
    p_id INT,
    p_status INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE devices SET status = p_status WHERE id = p_id;
END;
$$;
