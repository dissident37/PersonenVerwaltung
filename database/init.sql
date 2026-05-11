-- Datenbankinitialisierung fuer PersonenVerwaltung

-- Datenbank erstellen (wird vom Docker-Entrypoint automatisch erstellt)
-- CREATE DATABASE personenverwaltung;

-- Tabelle Person
CREATE TABLE IF NOT EXISTS "Person" (
    "Id"            SERIAL PRIMARY KEY,
    "Name"          VARCHAR(100) NOT NULL,
    "Vorname"       VARCHAR(100) NOT NULL,
    "Geburtsdatum"  DATE NOT NULL,
    "NameUppercase" VARCHAR(100)
);

-- Tabelle Anschrift
CREATE TABLE IF NOT EXISTS "Anschrift" (
    "Id"            SERIAL PRIMARY KEY,
    "PersonId"      INTEGER NOT NULL,
    "Postleitzahl"  VARCHAR(10) NOT NULL,
    "Ort"           VARCHAR(100) NOT NULL,
    "Strasse"       VARCHAR(150) NOT NULL,
    "Hausnummer"    VARCHAR(10) NOT NULL,
    CONSTRAINT "FK_Anschrift_Person"
        FOREIGN KEY ("PersonId") REFERENCES "Person"("Id")
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
);

-- Tabelle Telefonverbindung
CREATE TABLE IF NOT EXISTS "Telefonverbindung" (
    "Id"        SERIAL PRIMARY KEY,
    "PersonId"  INTEGER NOT NULL,
    "Nummer"    VARCHAR(50) NOT NULL,
    CONSTRAINT "FK_Telefonverbindung_Person"
        FOREIGN KEY ("PersonId") REFERENCES "Person"("Id")
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
);

-- Indizes fuer Fremdschluessel
CREATE INDEX IF NOT EXISTS "IX_Anschrift_PersonId" ON "Anschrift"("PersonId");
CREATE INDEX IF NOT EXISTS "IX_Telefonverbindung_PersonId" ON "Telefonverbindung"("PersonId");

-- Beispieldaten: Personen
INSERT INTO "Person" ("Name", "Vorname", "Geburtsdatum") VALUES
    ('Müller',     'Hans',     '1985-03-12'),
    ('Schmidt',    'Anna',     '1990-07-24'),
    ('Weber',      'Klaus',    '1978-11-05'),
    ('Fischer',    'Maria',    '1995-01-18'),
    ('Hoffmann',   'Peter',    '1982-09-30'),
    ('Koch',       'Sandra',   '1988-06-15'),
    ('Bauer',      'Thomas',   '1975-04-22');

-- Beispieldaten: Anschriften
INSERT INTO "Anschrift" ("PersonId", "Postleitzahl", "Ort", "Strasse", "Hausnummer") VALUES
    -- Hans Müller (Id=1) - Dresden
    (1, '01067', 'Dresden',  'Prager Straße',       '14'),
    (1, '01069', 'Dresden',  'Weißeritzstraße',     '3a'),
    -- Anna Schmidt (Id=2) - Dresden
    (2, '01099', 'Dresden',  'Bautzner Straße',     '72'),
    (2, '01187', 'Dresden',  'Chemnitzer Straße',   '45'),
    -- Klaus Weber (Id=3) - Leipzig
    (3, '04109', 'Leipzig',  'Markt',               '1'),
    (3, '04103', 'Leipzig',  'Karl-Liebknecht-Str', '22'),
    -- Maria Fischer (Id=4) - Berlin
    (4, '10115', 'Berlin',   'Unter den Linden',    '5'),
    (4, '10178', 'Berlin',   'Alexanderplatz',      '10'),
    -- Peter Hoffmann (Id=5) - Dresden
    (5, '01307', 'Dresden',  'Blasewitzer Straße',  '88'),
    -- Sandra Koch (Id=6) - München
    (6, '80331', 'München',  'Marienplatz',         '2'),
    (6, '80469', 'München',  'Reichenbachstraße',   '19'),
    -- Thomas Bauer (Id=7) - Dresden
    (7, '01277', 'Dresden',  'Tolkewitzer Straße',  '33'),
    (7, '01309', 'Dresden',  'Pillnitzer Straße',   '7');

-- Beispieldaten: Telefonverbindungen
INSERT INTO "Telefonverbindung" ("PersonId", "Nummer") VALUES
    -- Hans Müller: mehrere Nummern
    (1, '0351-4567890'),
    (1, '+49 351 9876543'),
    (1, '015X-12345678'),
    -- Anna Schmidt: mehrere Nummern
    (2, '0351-1234567'),
    (2, '+49 172 9988776'),
    -- Klaus Weber: eine Nummer
    (3, '0341-9876543'),
    -- Maria Fischer: mehrere Nummern
    (4, '030-11223344'),
    (4, '+49 30 55667788'),
    (4, '49-abc-invalid'),
    -- Peter Hoffmann: mehrere Nummern
    (5, '0351-6655443'),
    (5, '+49 160 1234567'),
    -- Sandra Koch: eine Nummer
    (6, '089-44556677'),
    -- Thomas Bauer: mehrere Nummern
    (7, '0351-7778899'),
    (7, '+49 176 5544332'),
    (7, 'keine-nummer');

-- ==========================================================
-- Abfragen (als Kommentare)
-- ==========================================================

-- Anzahl aller Personen:
-- SELECT COUNT(*) AS "AnzahlPersonen" FROM "Person";

-- Anzahl der Personen, die in Dresden wohnen:
-- SELECT COUNT(DISTINCT p."Id") AS "PersonenInDresden"
-- FROM "Person" p
-- JOIN "Anschrift" a ON a."PersonId" = p."Id"
-- WHERE a."Ort" = 'Dresden';

-- Anzahl der Personen mit mehr als einer Telefonnummer:
-- SELECT COUNT(*) AS "PersonenMitMehrerenNummern"
-- FROM (
--     SELECT "PersonId"
--     FROM "Telefonverbindung"
--     GROUP BY "PersonId"
--     HAVING COUNT(*) > 1
-- ) sub;

-- Anzahl der Personen pro Ort:
-- SELECT a."Ort", COUNT(DISTINCT a."PersonId") AS "AnzahlPersonen"
-- FROM "Anschrift" a
-- GROUP BY a."Ort"
-- ORDER BY "AnzahlPersonen" DESC;

-- ==========================================================
-- ALTER TABLE: Spalte NameUppercase hinzufuegen
-- (bereits in CREATE TABLE enthalten, hier als nachtraegliche Aenderung)
-- ==========================================================
-- ALTER TABLE "Person" ADD COLUMN IF NOT EXISTS "NameUppercase" VARCHAR(100);

-- UPDATE: NameUppercase befuellen
UPDATE "Person" SET "NameUppercase" = UPPER("Name");

-- ==========================================================
-- DELETE: Telefonnummern loeschen, die nicht mit '0' oder '+' beginnen
-- ==========================================================
DELETE FROM "Telefonverbindung"
WHERE "Nummer" NOT LIKE '0%'
  AND "Nummer" NOT LIKE '+%';

-- ==========================================================
-- VIEW: vw_PersonDetails
-- ==========================================================
CREATE OR REPLACE VIEW "vw_PersonDetails" AS
SELECT
    p."Id"            AS "PersonId",
    p."Name",
    p."Vorname",
    p."Geburtsdatum",
    p."NameUppercase",
    a."Id"            AS "AnschriftId",
    a."Postleitzahl",
    a."Ort",
    a."Strasse",
    a."Hausnummer",
    t."Id"            AS "TelefonId",
    t."Nummer"
FROM "Person" p
LEFT JOIN "Anschrift" a         ON a."PersonId" = p."Id"
LEFT JOIN "Telefonverbindung" t ON t."PersonId" = p."Id";
