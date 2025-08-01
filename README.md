# 🎟️ TicketSystemApi

一個使用 ASP.NET Core 開發的售票系統後端 API，支援使用者註冊、活動管理、訂單處理等功能，搭配 PostgreSQL 資料庫，並支援 Docker 化部署與本機容器化開發環境。

---

## 📁 專案目錄結構

```
TicketSystemApi/
│
├── .devcontainer/           # VS Code Dev Container 設定
├── Common/                  # 共用參數、常數、擴充工具等
├── Controllers/             # API 控制器
├── Data/                    # DbContext 與資料初始化
├── document/                # 備份 SQL 檔與開發筆記
├── Dtos/                    # Data Transfer Objects
├── Middleware/              # 中介層 (Exception 處理等)
├── Models/                  # Entity 資料模型
├── Profiles/                # AutoMapper 設定檔
├── Properties/
├── Repositories/            # 資料存取層 (Repository Pattern)
├── Services/                # 商業邏輯層 (Service Layer)
├── Validators/              # FluentValidation 驗證邏輯
│
├── appsettings.json
├── appsettings.Development.json
├── docker-compose.yml       # 多容器部署設定
├── Dockerfile               # API Docker 建置設定
├── Program.cs               # 應用程式進入點
├── TicketSystemApi.csproj
├── TicketSystemApi.http     # API 測試用 (REST Client)
└── TicketSystemApi.sln
```

---

## 🚀 開發環境

- .NET 9 SDK
- PostgreSQL 14+
- Visual Studio 2022 / VS Code
- Docker + Dev Container
- Swagger / REST Client
- AutoMapper / FluentValidation / JWT（如後續導入）

---

## ⚙️ 快速啟動（本機環境）

### 🔧 前置作業
1. 安裝 [.NET 9 SDK](https://dotnet.microsoft.com/)
2. 安裝 [Docker Desktop](https://www.docker.com/products/docker-desktop)
3. 安裝 [VS Code](https://code.visualstudio.com/) 並安裝 Dev Containers 外掛

### 🚢 使用 Dev Container（推薦）
專案已內建 `.devcontainer` 設定，開啟 VS Code 後選擇：
```
Reopen in Container
```

容器將會自動安裝所有開發相依套件與 PostgreSQL。

### 🧪 本機執行
```bash
dotnet restore
dotnet ef database update    # 如有使用 Migration
dotnet run
```

預設啟動網址：
```
http://localhost:5000
http://localhost:5000/swagger
```

---

## 🐘 資料庫說明

- 使用 PostgreSQL
- 預設資料庫名稱：`concert_test`
- 備份檔位於 `document/concert_test_backup.sql`
- 可透過 docker-compose 啟動資料庫：
```bash
docker-compose up -d
```

---

## 📡 API 測試

- 使用內建的 `TicketSystemApi.http` 檔，可用 VS Code REST Client 測試 API。
- 或透過 Swagger UI 測試：

```
http://localhost:5000/swagger
```

---

## 📦 建置 Docker 映像檔

```bash
docker build -t ticketsystem-api .
docker run -p 5000:80 ticketsystem-api
```

---

## 📄 文件與筆記

- `document/開發筆記.txt`：紀錄開發流程、遇到的問題與修正紀錄
- `concert_test_backup.sql`：PostgreSQL 資料匯出備份
- `2025-06-25-[PostgreSQL]資料庫備份輸出...`：資料快照說明檔案

---

## ✅ 未來規劃（建議可放這區）

- [ ] 加入使用者認證（JWT）
- [ ] 建立前端 UI（例如 Next.js）
- [ ] API 測試覆蓋率提升
- [ ] CI/CD 自動部署（GitHub Actions）

---

## 🧾 授權條款

MIT License © 2025 NickFu
