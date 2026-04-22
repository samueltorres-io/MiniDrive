<div align="center">

```
███╗   ███╗██╗███╗   ██╗██╗██████╗ ██████╗ ██╗██╗   ██╗███████╗
████╗ ████║██║████╗  ██║██║██╔══██╗██╔══██╗██║██║   ██║██╔════╝
██╔████╔██║██║██╔██╗ ██║██║██║  ██║██████╔╝██║██║   ██║█████╗  
██║╚██╔╝██║██║██║╚██╗██║██║██║  ██║██╔══██╗██║╚██╗ ██╔╝██╔══╝  
██║ ╚═╝ ██║██║██║ ╚████║██║██████╔╝██║  ██║██║ ╚████╔╝ ███████╗
╚═╝     ╚═╝╚═╝╚═╝  ╚═══╝╚═╝╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═══╝  ╚══════╝
```

**Your personal cloud storage — self-hosted, lightweight, and yours.**

[![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)](https://angular.io/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![MinIO](https://img.shields.io/badge/MinIO-C72E49?style=for-the-badge&logo=minio&logoColor=white)](https://min.io/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)

</div>

---

## 📦 What is MiniDrive?

**MiniDrive** is a self-hosted file storage API built with **ASP.NET Core (.NET 10)** and backed by **MinIO** for object storage and **PostgreSQL** for metadata. Think of it as a minimal Google Drive clone you can run entirely on your own infrastructure.

Users can create accounts, organize files into folders, upload and download content, and manage everything through a clean REST API — with an **Angular** frontend to tie it all together.

---

## 🏗️ Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core (.NET 10) |
| Frontend | Angular |
| Database | PostgreSQL 16 |
| Object Storage | MinIO (S3-compatible) |
| ORM | Entity Framework Core 10 + Npgsql |
| API Docs | OpenAPI (built-in .NET 10) |
| Infrastructure | Docker + Docker Compose |

---

## 🚀 Getting Started

The entire stack runs with a single command. Make sure you have **Docker** and **Docker Compose** installed.

```bash
# Clone the repository
git clone https://github.com/your-username/minidrive.git
cd minidrive

# Spin everything up
docker compose up
```

That's it. Docker Compose will handle spinning up PostgreSQL, MinIO (including bucket creation), the .NET API with hot reload, and the Angular frontend.

| Service | URL |
|---|---|
| API | http://localhost:8080 |
| Frontend | http://localhost:4200 |
| MinIO Console | http://localhost:9001 |
| OpenAPI Docs | http://localhost:8080/openapi/v1.json *(dev only)* |

> **MinIO credentials (local dev):** `minioadmin` / `minioadmin`

---

## 📡 API Reference

### Users

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/users` | Create a new user |
| `GET` | `/api/users` | Get a user by `id` and/or `username` |

**Create user** — `POST /api/users`
```json
{
  "username": "john"
}
```

**Get user** — `GET /api/users?id=1` or `GET /api/users?username=john`

---

### Folders

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/folders` | Create a new folder |
| `GET` | `/api/folders` | Get folder contents (navigate hierarchy) |
| `DELETE` | `/api/folders/{id}` | Soft delete a folder |

**Create folder** — `POST /api/folders`
```json
{
  "userId": 1,
  "name": "My Documents",
  "parentId": null
}
```

**Browse folder** — `GET /api/folders?userId=1&folderId=3`  
Omit `folderId` to start from the root.

---

### Files

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/files` | Upload a file (multipart/form-data) |
| `GET` | `/api/files` | List files in a folder |
| `GET` | `/api/files/{id}/download` | Download a file |
| `DELETE` | `/api/files/{id}` | Soft delete a file |

**Upload file** — `POST /api/files` (multipart/form-data)

| Field | Type | Required |
|---|---|---|
| `userId` | int | ✅ |
| `folderId` | int | ❌ |
| `file` | binary | ✅ |

> Maximum upload size: **512 MB**

**List files** — `GET /api/files?userId=1&folderId=3`  
Omit `folderId` to list files at the root level.

**Download** — `GET /api/files/{id}/download?userId=1`

**Delete** — `DELETE /api/files/{id}?userId=1`

---

## ⚙️ Configuration

All configuration is handled via environment variables. The `docker-compose.yml` sets sensible defaults for local development.

| Variable | Description | Default |
|---|---|---|
| `ConnectionStrings__Default` | PostgreSQL connection string | `Host=postgres;...` |
| `Minio__Endpoint` | MinIO S3 endpoint | `http://minio:9000` |
| `Minio__AccessKey` | MinIO access key | `minioadmin` |
| `Minio__SecretKey` | MinIO secret key | `minioadmin` |
| `Minio__Bucket` | Storage bucket name | `minidrive` |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` |

> In **Development** mode the database schema is auto-created via `EnsureCreatedAsync()` and the OpenAPI endpoint is exposed. No migrations needed to get started.

---

## 🛠️ Local Development (without Docker)

If you prefer to run the API directly on your machine:

```bash
# Make sure PostgreSQL and MinIO are running, then:
cd MiniDrive

dotnet restore
dotnet watch run
```

Update your `appsettings.Development.json` with the correct connection strings pointing to your local services.

---

## 📝 License

This project is open source. Feel free to use it, break it, and make it your own.