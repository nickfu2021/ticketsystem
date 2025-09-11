-- 查資料庫使用者
SELECT usename FROM pg_user;

SELECT * FROM user_tokens;

-- DELETE FROM user_tokens;

SELECT * FROM refresh_tokens;

SELECT * FROM users;

-- DELETE FROM users WHERE user_uuid='501632eb-e25c-4afb-a619-a8a16c6af7c4';

-- 刪除資料並重置 PRIMARY KEY
-- TRUNCATE TABLE users RESTART IDENTITY;

-- 用 INSERT INTO users (id, ...) VALUES (1, ...) 插了幾筆測試資料
-- 結果 PostgreSQL 自動遞增的序列還停留在 1
-- 請執行這段 SQL，重設 users_id_seq 為目前最大 id + 1：
SELECT setval('users_id_seq', (SELECT MAX(id) FROM users) + 1);

-- 查詢驗證目前序列值：
SELECT last_value FROM users_id_seq;

-- 新增欄位並設定DEFAULT
-- ALTER TABLE users ADD COLUMN address VARCHAR(255) NOT NULL DEFAULT '';

-- 刪除欄位DEFAULT，因為程式裡會給預設值空白，就不在資料庫也給預設值避免混淆
-- ALTER TABLE users ALTER COLUMN postal_code DROP DEFAULT;

-- 修改欄位名稱
-- ALTER TABLE users RENAME COLUMN password_hash TO password;

-- 查表的索引
SELECT * FROM pg_indexes WHERE tablename='postal';

-- 建立索引
CREATE INDEX idx_postal_zip_code ON postal(zip_code);
CREATE INDEX idx_postal_city_district_road ON postal(city, district, road);

-- PostgreSQL 內建 uuid 型態，但要能自動產生，需要啟用 pgcrypto extension：
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

/* 匯入.csv 資料到 psql 指令要在 powershell 使用
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_1.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_2.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_3.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_4.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_5.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_6.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_7.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
*/
