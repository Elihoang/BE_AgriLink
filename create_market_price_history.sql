-- Create market_price_history table
CREATE TABLE IF NOT EXISTS market_price_history (
    id SERIAL PRIMARY KEY,
    product_type VARCHAR(50) NOT NULL,
    product_name VARCHAR(100) NOT NULL,
    region VARCHAR(50),
    region_code VARCHAR(20),
    price DECIMAL(18,2) NOT NULL,
    change DECIMAL(18,2) NOT NULL DEFAULT 0,
    change_percent DECIMAL(18,2) NOT NULL DEFAULT 0,
    unit VARCHAR(20) NOT NULL DEFAULT 'kg',
    recorded_date TIMESTAMP NOT NULL,
    source VARCHAR(100),
    updated_by VARCHAR(100),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    notes VARCHAR(500)
);

-- Create indexes
CREATE INDEX idx_market_price_history_composite 
    ON market_price_history (product_type, region_code, recorded_date);

CREATE INDEX idx_market_price_history_date 
    ON market_price_history (recorded_date);

-- Seed data (Cà phê và Hồ tiêu cho 4 tỉnh, 2 ngày)
INSERT INTO market_price_history (
    id, product_type, product_name, region, region_code, 
    price, change, change_percent, unit, recorded_date, 
    source, updated_by, created_at, notes
) VALUES
-- Cà phê - Hôm nay (2026-01-25)
(1, 'COFFEE', 'Cà phê Robusta', 'Đắk Lắk', 'DAK_LAK', 100900, 1700, 1.71, 'VND/kg', '2026-01-25', 'Admin', 'System', CURRENT_TIMESTAMP, 'Seed data'),
(2, 'COFFEE', 'Cà phê Robusta', 'Lâm Đồng', 'LAM_DONG', 100500, 1700, 1.72, 'VND/kg', '2026-01-25', 'Admin', 'System', CURRENT_TIMESTAMP, 'Seed data'),
(3, 'COFFEE', 'Cà phê Robusta', 'Gia Lai', 'GIA_LAI', 100800, 1600, 1.61, 'VND/kg', '2026-01-25', 'Admin', 'System', CURRENT_TIMESTAMP, 'Seed data'),
(4, 'COFFEE', 'Cà phê Robusta', 'Đắk Nông', 'DAK_NONG', 101000, 1700, 1.71, 'VND/kg', '2026-01-25', 'Admin', 'System', CURRENT_TIMESTAMP, 'Seed data'),

-- Cà phê - Hôm qua (2026-01-24)
(5, 'COFFEE', 'Cà phê Robusta', 'Đắk Lắk', 'DAK_LAK', 99200, -800, -0.80, 'VND/kg', '2026-01-24', 'Admin', 'System', CURRENT_TIMESTAMP, 'Seed data'),
(6, 'COFFEE', 'Cà phê Robusta', 'Lâm Đồng', 'LAM_DONG', 98800, -700, -0.70, 'VND/kg', '2026-01-24', 'Admin', 'System', CURRENT_TIMESTAMP, 'Seed data'),
(7, 'COFFEE', 'Cà phê Robusta', 'Gia Lai', 'GIA_LAI', 99200, -800, -0.80, 'VND/kg', '2026-01-24', 'Admin', 'System', CURRENT_TIMESTAMP, 'Seed data'),
(8, 'COFFEE', 'Cà phê Robusta', 'Đắk Nông', 'DAK_NONG', 99300, -900, -0.90, 'VND/kg', '2026-01-24', 'Admin', 'System', CURRENT_TIMESTAMP, 'Seed data'),

-- Hồ tiêu - Hôm nay (2026-01-25)
(9, 'PEPPER', 'Hồ tiêu', 'Đắk Lắk', 'DAK_LAK', 149000, 0, 0, 'VND/kg', '2026-01-25', 'Admin', 'System', CURRENT_TIMESTAMP, 'Seed data'),
(10, 'PEPPER', 'Hồ tiêu', 'Lâm Đồng', 'LAM_DONG', 149000, 0, 0, 'VND/kg', '2026-01-25', 'Admin', 'System', CURRENT_TIMESTAMP, 'Seed data'),
(11, 'PEPPER', 'Hồ tiêu', 'Gia Lai', 'GIA_LAI', 149000, 0, 0, 'VND/kg', '2026-01-25', 'Admin', 'System', CURRENT_TIMESTAMP, 'Seed data'),
(12, 'PEPPER', 'Hồ tiêu', 'Đắk Nông', 'DAK_NONG', 149000, 0, 0, 'VND/kg', '2026-01-25', 'Admin', 'System', CURRENT_TIMESTAMP, 'Seed data')

ON CONFLICT DO NOTHING;

-- Reset sequence
SELECT setval('market_price_history_id_seq', (SELECT MAX(id) FROM market_price_history));
