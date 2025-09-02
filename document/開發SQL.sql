-- 查資料庫使用者
SELECT usename FROM pg_user;

/* 匯入.csv 資料到 psql 指令要在 powershell 使用
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_1.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_2.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_3.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_4.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_5.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_6.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
\copy postal(zip_code, city, district, road) FROM 'C:/Users/win10/Desktop/postal_part_7.csv' WITH (FORMAT csv, HEADER true, ENCODING 'UTF8');
*/


SELECT * FROM postal;

SELECT * FROM postal WHERE district='南港區';

SELECT * FROM customers;

SELECT * FROM orders;

SELECT * FROM events;

SELECT * FROM users;

DELETE FROM users WHERE id=5;

UPDATE users SET id_number='H138406654' WHERE id=4;

TRUNCATE TABLE users RESTART IDENTITY;

SELECT * FROM users;
SELECT * FROM groups;
SELECT * FROM programs;

SELECT * FROM user_groups;
SELECT * FROM group_programs;

DROP TABLE users;

/* 
用 INSERT INTO users (id, ...) VALUES (1, ...) 插了幾筆測試資料
結果 PostgreSQL 自動遞增的序列還停留在 1
請執行這段 SQL，重設 users_id_seq 為目前最大 id + 1：
*/
SELECT setval('users_id_seq', (SELECT MAX(id) FROM users) + 1);

-- 查詢驗證目前序列值：
SELECT last_value FROM users_id_seq;

-- 新增欄位並設定DEFAULT
ALTER TABLE users ADD COLUMN address VARCHAR(255) NOT NULL DEFAULT '';

-- 刪除欄位DEFAULT，因為程式裡會給預設值空白，就不在資料庫也給預設值避免混淆
ALTER TABLE users ALTER COLUMN postal_code DROP DEFAULT;

-- 修改欄位名稱
ALTER TABLE users RENAME COLUMN password_hash TO password;

-- 查表建立的索引
SELECT * FROM pg_indexes WHERE tablename='postal';

-- 建立索引
CREATE INDEX idx_postal_zip_code ON postal(zip_code);
CREATE INDEX idx_postal_city_district_road ON postal(city, district, road);

-- PostgreSQL 內建 uuid 型態，但要能自動產生，需要啟用 pgcrypto extension：
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

CREATE EXTENSION IF NOT EXISTS "pgcrypto"; -- 提供 gen_random_uuid()

SELECT * FROM refresh_tokens;

-- refresh_tokens 建表 SQL
CREATE TABLE public.refresh_tokens (
    rt_uuid         uuid PRIMARY KEY DEFAULT gen_random_uuid(),  -- Token PK
    user_uuid       uuid NOT NULL,                               -- 關聯使用者
    token_hash      text NOT NULL,                               -- Refresh Token 雜湊（不存明文）
    created_at      timestamptz NOT NULL DEFAULT now(),          -- 建立時間
    created_by_ip   text,                                        -- 建立時來源 IP
    user_agent      text,                                        -- 建立時 User-Agent

    expires_at      timestamptz NOT NULL,                        -- 到期時間
    revoked_at      timestamptz,                                 -- 撤銷時間（登出/輪轉/風控）
    replaced_by     text,                                        -- 被哪個新 RT 取代（存新 RT 的 hash）
    revoked_reason  text,                                        -- 撤銷原因（可選）

    last_used_at    timestamptz,                                 -- 最後使用時間
    last_used_ip    text,                                        -- 最後使用來源 IP

    CONSTRAINT uq_refresh_token UNIQUE (token_hash)              -- 保證 hash 唯一
);

-- 索引建議（方便查詢）
CREATE INDEX idx_refresh_tokens_user_uuid ON public.refresh_tokens(user_uuid);
CREATE INDEX idx_refresh_tokens_expires   ON public.refresh_tokens(expires_at);
CREATE INDEX idx_refresh_tokens_revoked   ON public.refresh_tokens(revoked_at);


ALTER TABLE refresh_tokens
ALTER COLUMN created_by_ip TYPE text USING created_by_ip::text;

ALTER TABLE refresh_tokens
ALTER COLUMN last_used_ip TYPE text USING last_used_ip::text;