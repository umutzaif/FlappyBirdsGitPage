-- Add status column to devices
-- 0: Lobby (New/Waiting), 1: Active (Paired), 2: Passive (User disabled), 3: Trash (Pending Delete)
ALTER TABLE devices ADD COLUMN IF NOT EXISTS status INT DEFAULT 0;

-- Create business_profile table
CREATE TABLE IF NOT EXISTS business_profile (
    id SERIAL PRIMARY KEY,
    company_name VARCHAR(100) NOT NULL DEFAULT 'Enerji İzleme A.Ş.',
    establishment_date DATE DEFAULT CURRENT_DATE,
    budget_limit DECIMAL(18, 2) DEFAULT 0,
    currency VARCHAR(10) DEFAULT 'TL',
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insert initial record if not exists
INSERT INTO business_profile (company_name, budget_limit)
SELECT 'Enerji İzleme A.Ş.', 50000
WHERE NOT EXISTS (SELECT 1 FROM business_profile);
