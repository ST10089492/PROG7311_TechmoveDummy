# TechMove (PROG7311 POE Part 3)

Logistics contract management system for the PROG7311 portfolio. Part 3 splits the Part 2 monolith into a Service-Oriented Architecture: an ASP.NET Core Web API backend, an MVC frontend that calls the API over HTTP, and a SQL Server database, all runnable with Docker Compose.

## Projects

- **TechMove.Api** - the Web API, the only project that talks to the database. Holds the EF Core context, the Factory / Observer / Strategy patterns, the services, JWT auth and Swagger.
- **TechMove.Web** - the MVC frontend. No database access, it calls the API with typed `HttpClient` services and keeps the JWT in session.
- **TechMove.Tests** - xUnit unit tests and `WebApplicationFactory` integration tests.

## Running with Docker (the whole system)

From the solution root:

```
docker compose up --build
```

This starts three containers on one network:

| Service | What it is | URL |
|---|---|---|
| `sql-server-db` | SQL Server 2022 | localhost:1433 |
| `techmove-api` | Web API + Swagger | http://localhost:5080/swagger |
| `techmove-web` | MVC frontend | http://localhost:5000 |

The API waits for the database health check before it migrates and starts. The frontend reaches the API by its service name (`http://techmove-api:8080`) on the internal network.

## Running locally in Visual Studio

1. Start **TechMove.Api** (runs on https://localhost:7257, Swagger opens automatically).
2. Start **TechMove.Web** (its `ApiBaseUrl` in `appsettings.json` already points at the API).
3. Update the database the first time with `Update-Database` in the Package Manager Console, or just let the API migrate on startup.

## Login

Browsing (all the index and details pages) is open to everyone. Creating, editing, deleting and changing status requires logging in with the seeded account:

- Username: `admin`
- Password: `Admin123!`

## Running the tests

```
dotnet test TechMove.sln
```

The same suite runs automatically in GitHub Actions on every push (see `.github/workflows/dotnet.yml`).

## Key endpoints

- `GET /api/contracts` (supports `from`, `to`, `status` filters)
- `POST /api/contracts`, `PUT /api/contracts/{id}`, `PATCH /api/contracts/{id}/status`
- `POST /api/contracts/{id}/agreement` (PDF upload)
- `GET/POST /api/clients`, `GET/POST /api/servicerequests`, `PATCH /api/servicerequests/{id}/status`
- `POST /api/auth/login`

The technical reflection report is in `docs/Technical-Reflection-Report.md`.
