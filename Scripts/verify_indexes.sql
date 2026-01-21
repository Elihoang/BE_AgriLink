-- Script để kiểm tra các indexes mới đã được tạo trong PostgreSQL
-- Chạy script này trong pgAdmin hoặc psql để verify

-- 1. Xem TẤT CẢ các indexes trong database
SELECT 
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE schemaname = 'public'
ORDER BY tablename, indexname;

-- 2. Kiểm tra các indexes MỚI được tạo bởi migration AddPerformanceIndexes
SELECT 
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE schemaname = 'public'
  AND indexname IN (
    'IX_weather_logs_farm_id_log_date',
    'IX_material_usages_season_id_usage_date',
    'IX_material_usages_is_deleted',
    'IX_worker_advances_worker_id_season_id',
    'IX_worker_advances_is_deducted',
    'IX_workers_is_active',
    'IX_crop_seasons_status',
    'IX_crop_seasons_is_deleted',
    'IX_daily_work_logs_is_deleted',
    'IX_farms_is_deleted'
  )
ORDER BY tablename, indexname;

-- 3. Kiểm tra index usage statistics (sau khi chạy app một thời gian)
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan as "Số lần dùng index",
    idx_tup_read as "Số rows đọc",
    idx_tup_fetch as "Số rows fetch"
FROM pg_stat_user_indexes
WHERE tablename IN (
    'weather_logs', 
    'material_usages', 
    'worker_advances',
    'workers',
    'crop_seasons',
    'daily_work_logs',
    'farms'
)
ORDER BY idx_scan DESC;

-- 4. Kiểm tra kích thước của indexes
SELECT
    schemaname,
    tablename,
    indexname,
    pg_size_pretty(pg_relation_size(indexrelid)) as "Kích thước index"
FROM pg_stat_user_indexes
WHERE tablename IN (
    'weather_logs', 
    'material_usages', 
    'worker_advances',
    'workers',
    'crop_seasons',
    'daily_work_logs',
    'farms'
)
ORDER BY pg_relation_size(indexrelid) DESC;

-- 5. Kiểm tra các indexes với hiệu suất kém (không được dùng)
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan,
    pg_size_pretty(pg_relation_size(indexrelid)) as size
FROM pg_stat_user_indexes
WHERE idx_scan = 0
  AND schemaname = 'public'
ORDER BY pg_relation_size(indexrelid) DESC;

-- 6. Analyze performance cho một query cụ thể (example)
EXPLAIN ANALYZE
SELECT * FROM weather_logs 
WHERE farm_id = '00000000-0000-0000-0000-000000000001'
  AND log_date >= CURRENT_DATE - INTERVAL '7 days'
  AND log_date <= CURRENT_DATE;

-- 7. Kiểm tra migration history
SELECT * FROM "__EFMigrationsHistory" 
ORDER BY "MigrationId" DESC 
LIMIT 5;
