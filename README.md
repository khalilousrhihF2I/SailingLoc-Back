 
# StartupApi — .NET 8 REST API

## Overview

StartupApi is a REST API built with **.NET 8**, **ASP.NET Core**, **Entity Framework Core**, and **Identity**. It supports JWT authentication, user management, file uploads, and audit logs. The API is designed to be consumed by web and mobile clients.

---

## Quick Start

### 1. Restore NuGet Packages

```bash
dotnet restore
```

### 2. Database Setup

#### Option A — Using Visual Studio (Local SQL Server)

1. Install **SQL Server** (Developer edition or Express) and **SQL Server Management Studio (SSMS)**.
2. Open the **SQL Server project (`.sqlproj`)** in Visual Studio.
3. Right-click the project → **Publish**.
4. Configure a target database (e.g., `MayaDB`), click **Publish**.
5. Update `appsettings.json` connection string if needed:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=MayaDB;Trusted_Connection=True;TrustServerCertificate=True"
}
```

#### Option B — Using Docker

1. Ensure **Docker Desktop** is installed and running.
2. Run SQL Server container:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=StartupDB2025!" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

3. Update `appsettings.json` to point to the container:

```json
"DefaultConnection": "Server=localhost,1433;Database=MayaDB;User Id=sa;Password=StartupDB2025!;TrustServerCertificate=True"
```

---

### 3. Run the API

#### Without Docker:

```bash
cd src/Api
dotnet run
```

#### With Docker (API only):

1. Build and run the API container:

```bash
docker build -t startupapi .
docker run -p 5000:80 startupapi
```

> ⚠️ Currently, the Dockerfile only contains the API. SQL Server is external or local.

2. Swagger UI will be available at: `http://localhost:5000/swagger`.

---

## Why Docker?

Docker ensures a **consistent environment** across machines, which is especially useful for developers not familiar with .NET or SQL Server. A mobile developer can spin up the API container without installing .NET or SQL locally.

---

## Seeded Users

* **Admin:** `admin@local.test`
  Password from `Seed:AdminPassword` in `appsettings.json` (default: `ChangeMe!123`)
* **Roles:** `Admin`, `User` (auto-created if missing)

---

## AuthController Endpoints (Mobile Integration)

| Endpoint                              | Method | Auth | Description                                   |
| ------------------------------------- | ------ | ---- | --------------------------------------------- |
| `/api/v1/auth/register`               | POST   | No   | Register new user (optional avatar)           |
| `/api/v1/auth/login`                  | POST   | No   | Login and get `accessToken` + `refreshToken`  |
| `/api/v1/auth/refresh`                | POST   | No   | Refresh access token using `refreshToken`     |
| `/api/v1/auth/logout`                 | POST   | Yes  | Revoke refresh token                          |
| `/api/v1/auth/request-password-reset` | POST   | No   | Request password reset email (link simulated) |
| `/api/v1/auth/reset-password`         | POST   | No   | Reset password using token                    |
| `/api/v1/auth/me`                     | GET    | Yes  | Get authenticated user's profile              |
| `/api/v1/auth/me`                     | PUT    | Yes  | Update authenticated user's profile           |
| `/api/v1/auth/upload-avatar`          | POST   | Yes  | Upload avatar image (max 5 MB)                |

**JWT Flow for Mobile Apps:**

1. **Register / Login** → receive `accessToken` and `refreshToken`.
2. Include `accessToken` in `Authorization: Bearer {token}` header for protected endpoints.
3. **Refresh token** when `accessToken` expires.
4. **Logout** to revoke refresh token.

**Example (C# / HttpClient):**

```csharp
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", accessToken);

var profile = await client.GetFromJsonAsync<UserDto>("api/v1/auth/me");
```

---

## AdminUsersController

This controller allows **admin users** to manage other users and audit logs:

| Endpoint                         | Method | Role Required | Description                      |
| -------------------------------- | ------ | ------------- | -------------------------------- |
| `/api/v1/admin/users`            | GET    | Admin         | List users (pagination + search) |
| `/api/v1/admin/users/{id}`       | GET    | Admin         | Get user details                 |
| `/api/v1/admin/users/{id}`       | PUT    | Admin         | Update user profile              |
| `/api/v1/admin/users/{id}`       | DELETE | Admin         | Delete user                      |
| `/api/v1/admin/users/{id}/roles` | POST   | Admin         | Assign roles to a user           |
| `/api/v1/admin/audit-logs`       | GET    | Admin         | Get audit logs (pagination)      |

> Mobile developers may not need admin endpoints but can use them for **admin-only features** in the app if needed.

---

## Publishing Database from Visual Studio

1. Open **SQL Server project (`.sqlproj`)**.
2. Right-click → **Publish**.
3. Choose target database (local SQL Server or container).
4. Click **Publish** → schema and seed data are applied.

> This avoids manually running scripts and ensures the DB matches your code.

---

## Environment Variables

| Key                                   | Purpose                |
| ------------------------------------- | ---------------------- |
| `Jwt:Issuer`                          | JWT token issuer       |
| `Jwt:Audience`                        | JWT token audience     |
| `Jwt:SigningKey`                      | Secret for signing JWT |
| `Jwt:AccessTokenMinutes`              | Access token lifetime  |
| `Jwt:RefreshTokenDays`                | Refresh token lifetime |
| `Seed:AdminPassword`                  | Default admin password |
| `ConnectionStrings:DefaultConnection` | Database connection    |

---

## Notes for Mobile Developers

* Use **`login` endpoint** to get JWTs.
* Include **`Authorization: Bearer {accessToken}`** in headers for protected routes.
* Use **`refresh` endpoint** to renew tokens before expiration.
* You can upload avatars with **multipart/form-data** in `upload-avatar`.
* Admin-only endpoints require the **Admin role** and will return `403` if unauthorized.
 
