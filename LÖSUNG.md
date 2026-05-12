# Lösung der fachlichen Aufgaben

Dieses Dokument ordnet jeden Punkt der Aufgabenstellung der entsprechenden Stelle im Quellcode zu.

## Live-Demo

- **UI:** https://personen.konstantinsittner.de
- **API + Swagger:** https://personen-api.konstantinsittner.de/swagger
- **GitHub:** https://github.com/dissident37/PersonenVerwaltung

## Lokales Starten

```bash
docker compose up -d --build
```

Danach ist die Anwendung unter http://localhost:8081 erreichbar. Weitere Details siehe [README.md](README.md).

---

## Teil 1 — Datenbanksysteme und SQL

Die komplette Lösung der SQL-Aufgabe befindet sich als ein einziges Skript in [database/init.sql](database/init.sql).

| Aufgabenstellung | Umsetzung |
|---|---|
| Tabellen für Person, Anschrift, Telefonverbindung mit geeigneten Datentypen | [init.sql:8–38](database/init.sql) |
| 1:N-Beziehungen (Person → Anschrift, Person → Telefonverbindung) | Foreign Keys in init.sql:23–27 und 34–38 |
| Referentielle Integrität: Löschen ablehnen, wenn Anschriften oder Telefonnummern existieren | `ON DELETE NO ACTION` (init.sql:25, 36) |
| Befüllen mit Musterdaten | 7 Personen, 13 Anschriften, 15 Telefonnummern (init.sql:49–104) |
| Anzahl aller Personendatensätze | `SELECT COUNT(*) FROM "Person"` (init.sql:111) |
| Anzahl der Personen in Dresden | init.sql:114–117 |
| Anzahl der Personen mit mehr als einer Telefonnummer | init.sql:120–126 |
| Anzahl der Personen pro Ort | init.sql:129–132 |
| View mit allen Personen + Anschriften + Telefonnummern | `vw_PersonDetails` (init.sql:137–152) |
| DELETE für Telefonnummern, die nicht mit '0' oder '+' beginnen | init.sql:157–159 |
| ALTER TABLE: Spalte für Name in Großschreibung | init.sql:164 |
| UPDATE: Spalte mit Großbuchstaben befüllen | `UPDATE ... SET "NameUppercase" = UPPER("Name")` (init.sql:169) |

### Hinweis zur Datenbankwahl

Die Aufgabe nennt Microsoft SQL Server als **bevorzugtes**, aber nicht zwingendes RDBMS. Gewählt wurde **PostgreSQL** aus folgenden Gründen:

- Open Source, keine Lizenzkosten
- Plattformunabhängig (läuft im Docker-Container)
- Einfache Reproduzierbarkeit der gesamten Umgebung mit einem einzigen `docker compose up`
- Voller SQL-Standard-Support für alle in der Aufgabe geforderten Konstrukte

Die SQL-Syntax ist nahezu identisch zu T-SQL; ein Portieren auf MSSQL wäre mit minimalen Änderungen möglich (`SERIAL` → `IDENTITY`).

---

## Teil 2 — C# und .NET Framework

Die Applikation ist als **3-Schichten-Architektur** in drei separaten Projekten umgesetzt:

| Schicht | Projekt | Zweck |
|---|---|---|
| Datenschicht | [PersonenVerwaltung.Data](PersonenVerwaltung.Data/) | EF Core, Models, Repository |
| Applikationslogik | [PersonenVerwaltung.API](PersonenVerwaltung.API/) | REST-Webservice (ASP.NET Core) |
| Präsentationsschicht | [PersonenVerwaltung.UI](PersonenVerwaltung.UI/) | Blazor Server UI |

### Zuordnung Aufgabenstellung → Code

| Aufgabenstellung | Umsetzung |
|---|---|
| Relationales DBMS + Struktur aus SQL-Aufgabe | PostgreSQL (Container `personenverwaltung-db`), Schema aus [database/init.sql](database/init.sql) |
| UI-Technologie aus .NET-Framework | **Blazor Server** (in der Aufgabenstellung explizit als Option genannt) |
| ORM für Datenbankzugriff | **Entity Framework Core** + Npgsql, siehe [AppDbContext.cs](PersonenVerwaltung.Data/AppDbContext.cs) |
| 3-Schichten-Architektur | drei getrennte Projekte (siehe Tabelle oben) |
| Webservices auf Basis REST + JSON | [PersonsController.cs](PersonenVerwaltung.API/Controllers/PersonsController.cs) |
| Leere Ergebnistabelle + Schaltfläche „Personen laden" beim Start | [Pages/Index.razor](PersonenVerwaltung.UI/Pages/Index.razor) (Zeile 10–16, 26–80) |
| Optionales Suchkriterium in Textbox | Index.razor:11–14 → `GET /api/persons?name=...` |
| Detaildaten in neuem Dialog-Fenster bei Auswahl einer Person | Separate Seite [Pages/PersonDetail.razor](PersonenVerwaltung.UI/Pages/PersonDetail.razor) unter Route `/persons/{id}` |
| **Zusatzaufgabe:** Namen ändern + Schaltfläche „Sichern" | Inline-Editor in Index.razor:45–75, gespeichert über `PUT /api/persons/{id}` |

### REST-API-Endpunkte

| Methode | Route | Beschreibung |
|---|---|---|
| `GET` | `/api/persons?name=` | Liste aller Personen, optional gefiltert nach Name oder Vorname |
| `GET` | `/api/persons/{id}` | Eine Person mit allen Anschriften und Telefonnummern |
| `PUT` | `/api/persons/{id}` | Name und Vorname aktualisieren (Zusatzaufgabe) |

Implementierung: [PersonsController.cs](PersonenVerwaltung.API/Controllers/PersonsController.cs).
Interaktive Dokumentation via Swagger: https://personen-api.konstantinsittner.de/swagger.

### Datenfluss

```
Browser
   ↓ HTTP (Blazor Server)
UI  ─────►  PersonApiService  ─────►  HTTP/JSON
                                          ↓
                                    PersonsController (REST)
                                          ↓
                                    IPersonRepository
                                          ↓
                                    AppDbContext (EF Core)
                                          ↓
                                    PostgreSQL
```

---

## Bereitstellung / Deployment

- **Containerisierung:** komplette Umgebung in [docker-compose.yml](docker-compose.yml)
- **Reverse-Proxy:** Nginx-Konfiguration in [nginx/personenverwaltung.conf](nginx/personenverwaltung.conf) (HTTPS via Let's Encrypt)
- **CI/CD:** automatisches Deployment via GitHub Actions bei Push auf `main` ([.github/workflows/deploy.yml](.github/workflows/deploy.yml))
