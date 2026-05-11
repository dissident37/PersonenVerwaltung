# PersonenVerwaltung

ASP.NET Core 8 Anwendung zur Verwaltung von Personen, Anschriften und Telefonnummern.

## Projektstruktur

```
PersonenVerwaltung/
├── PersonenVerwaltung.sln
├── PersonenVerwaltung.API/         ← REST API (ASP.NET Core)
├── PersonenVerwaltung.Data/        ← Datenschicht (EF Core + PostgreSQL)
├── PersonenVerwaltung.UI/          ← Blazor Server UI
├── database/
│   └── init.sql                    ← Datenbankschema + Beispieldaten
├── docker-compose.yml
├── docker-compose.override.yml
├── nginx/
│   └── personenverwaltung.conf     ← Nginx Reverse-Proxy Konfiguration
└── .github/
    └── workflows/
        └── deploy.yml              ← GitHub Actions Deployment
```

## Lokales Starten mit Docker

### Voraussetzungen
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Start

```bash
docker compose up -d --build
```

Nach dem Start sind folgende Dienste erreichbar:

| Dienst  | URL                        |
|---------|----------------------------|
| UI      | http://localhost:8081      |
| API     | http://localhost:8080      |
| Swagger | http://localhost:8080/swagger |
| Adminer | http://localhost:8082      |

### Adminer-Login

- **System:** PostgreSQL
- **Server:** db
- **Benutzer:** postgres
- **Passwort:** postgres
- **Datenbank:** personenverwaltung

### Stop

```bash
docker compose down
```

## Lokale Entwicklung (ohne Docker)

### Voraussetzungen
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- PostgreSQL (lokal oder via Docker)

### API starten

```bash
cd PersonenVerwaltung.API
$env:DATABASE_URL = "Host=localhost;Database=personenverwaltung;Username=postgres;Password=postgres"
dotnet run
```

### UI starten

```bash
cd PersonenVerwaltung.UI
$env:API_URL = "http://localhost:5000"
dotnet run
```

## Deployment (VPS)

### Voraussetzungen auf dem VPS

1. Docker und Docker Compose installieren
2. Repository klonen:
   ```bash
   git clone <repo-url> /opt/personenverwaltung
   ```

### GitHub Secrets konfigurieren

Im GitHub Repository unter **Settings → Secrets and variables → Actions** folgende Secrets anlegen:

| Secret        | Beschreibung                            |
|---------------|-----------------------------------------|
| `VPS_HOST`    | Hostname/IP des VPS (z.B. `konstantinsittner.de`) |
| `VPS_USER`    | SSH-Benutzer (z.B. `root`)             |
| `VPS_SSH_KEY` | Privater SSH-Schlüssel für den VPS     |

### Nginx einrichten

Nginx-Konfiguration auf dem VPS kopieren:

```bash
cp nginx/personenverwaltung.conf /etc/nginx/sites-available/
ln -s /etc/nginx/sites-available/personenverwaltung.conf /etc/nginx/sites-enabled/
nginx -t && systemctl reload nginx
```

### Automatisches Deployment

Bei jedem Push auf `main` wird automatisch via GitHub Actions deployed:
1. SSH-Verbindung zum VPS
2. `git pull origin main`
3. `docker compose up -d --build`

## API Endpunkte

| Methode | Endpoint             | Beschreibung                              |
|---------|----------------------|-------------------------------------------|
| GET     | `/api/persons`       | Alle Personen (optional `?name=` Filter)  |
| GET     | `/api/persons/{id}`  | Eine Person mit Anschriften und Telefon   |
| PUT     | `/api/persons/{id}`  | Name und Vorname aktualisieren            |

## Umgebungsvariablen

| Variable       | Dienst | Beschreibung                    |
|----------------|--------|---------------------------------|
| `DATABASE_URL` | API    | PostgreSQL Verbindungszeichenfolge |
| `API_URL`      | UI     | Basis-URL der API               |
