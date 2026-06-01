# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

ASP.NET Core 8 application for managing persons with their addresses (Anschriften) and phone numbers (Telefonverbindungen). The codebase, comments, UI text, and domain model are in **German** — keep new code/comments consistent with that.

## Architecture

Three-layer architecture across three projects (referenced by `PersonenVerwaltung.sln`), all targeting `net8.0` with nullable + implicit usings enabled:

- **PersonenVerwaltung.Data** — EF Core + Npgsql data layer. `AppDbContext`, the `Person`/`Anschrift`/`Telefonverbindung` models, and `IPersonRepository`/`PersonRepository`. No business logic lives elsewhere; data access goes through the repository.
- **PersonenVerwaltung.API** — REST/JSON web service (controllers). Depends on Data. Single controller `PersonsController` at route `api/persons`.
- **PersonenVerwaltung.UI** — Blazor **Server** UI. Talks to the API only over HTTP via `PersonApiService` (a typed `HttpClient`); it does **not** reference the Data project or hit the database directly.

Data flow: Blazor page → `PersonApiService` (HTTP) → API controller → `IPersonRepository` → EF Core → PostgreSQL.

### Key conventions and gotchas

- **The database schema is owned by `database/init.sql`, not EF migrations.** There are no migrations; the SQL script creates tables, seeds sample data, and is mounted into the Postgres container's `docker-entrypoint-initdb.d`. EF entities must be kept manually in sync with this script. `OnModelCreating` maps entities to the singular PascalCase table names (`Person`, `Anschrift`, `Telefonverbindung`) used by the SQL.
- **Foreign keys use `DeleteBehavior.Restrict` / `ON DELETE NO ACTION`** by design — a Person cannot be deleted while addresses/phone numbers reference it (referential integrity is a stated requirement).
- **`NameUppercase`** is a denormalized column on `Person` kept in sync in `PersonRepository.UpdateNameAsync` (`name.ToUpper()`). It is also populated by the SQL script. Update it whenever `Name` changes.
- API controllers return **anonymous-typed projections**, not the EF entities directly (avoids over-exposing the model / cycle issues). The UI deserializes into mirror `record` types defined in `PersonApiService.cs` — keep these two shapes in sync.
- Configuration comes from **environment variables first**: `DATABASE_URL` (API) and `API_URL` (UI), each with a fallback in `appsettings.json`.

## Common commands

Build / run the whole stack (also seeds the DB and starts Adminer on :8082):

```powershell
docker compose up -d --build      # UI :8081, API :8080, Swagger :8080/swagger
docker compose down
```

Local development without Docker (needs .NET 8 SDK + a Postgres instance):

```powershell
# API on :5000
cd PersonenVerwaltung.API
$env:DATABASE_URL = "Host=localhost;Database=personenverwaltung;Username=postgres;Password=postgres"
dotnet run

# UI on :5001 (separate shell)
cd PersonenVerwaltung.UI
$env:API_URL = "http://localhost:5000"
dotnet run
```

Build / restore:

```powershell
dotnet build PersonenVerwaltung.sln
```

There is **no test project** in this repository.

## API endpoints

| Method | Endpoint            | Notes                                            |
|--------|---------------------|--------------------------------------------------|
| GET    | `/api/persons`      | List; optional `?name=` filters Name **or** Vorname (case-insensitive `Contains`) |
| GET    | `/api/persons/{id}` | Full detail incl. Anschriften + Telefonverbindungen |
| PUT    | `/api/persons/{id}` | Updates Name + Vorname only (body: `{ Name, Vorname }`) |

## Deployment

Push to `main` triggers `.github/workflows/deploy.yml`, which SSHes to the VPS, runs `git pull` + `docker compose up -d --build`. Nginx (`nginx/personenverwaltung.conf`) reverse-proxies the public HTTPS hostnames to the UI and API containers. CORS in the API currently allows any origin.
