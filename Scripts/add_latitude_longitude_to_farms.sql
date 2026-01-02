-- Migration: Add Latitude and Longitude to farms table
-- Date: 2025-12-31
-- Purpose: Allow users to select farm location from map without GPS device

-- Add latitude column
ALTER TABLE farms ADD COLUMN IF NOT EXISTS latitude NUMERIC(10, 7);

-- Add longitude column
ALTER TABLE farms ADD COLUMN IF NOT EXISTS longitude NUMERIC(10, 7);

-- Add comments for documentation
COMMENT ON COLUMN farms.latitude IS 'Vĩ độ - Người dùng chọn từ bản đồ (VD: 12.6667000)';
COMMENT ON COLUMN farms.longitude IS 'Kinh độ - Người dùng chọn từ bản đồ (VD: 108.0500000)';

-- Example: Update existing farm with coordinates
-- UPDATE farms SET latitude = 12.6667, longitude = 108.0500 WHERE name = 'Rẫy Đắk Mil';

-- Example: Insert new farm with map-selected coordinates
-- INSERT INTO farms (id, owner_user_id, name, area_size, latitude, longitude, created_at)
-- VALUES (
--   gen_random_uuid(),
--   'user-guid-here',
--   'Rẫy Mới',
--   2.5,
--   12.6667,
--   108.0500,
--   NOW()
-- );

-- Verify migration
SELECT column_name, data_type, numeric_precision, numeric_scale
FROM information_schema.columns
WHERE table_name = 'farms'
AND column_name IN ('latitude', 'longitude');
