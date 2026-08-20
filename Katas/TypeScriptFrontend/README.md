# Kata 13_03 — TypeScript-Frontend mit BFF

**Stufe 5: Differenzierung** · Zeitrahmen: ein Wochenende · baut auf Kata 09_01 und 13_01 auf

## Ziel

Ein .NET-Backend trifft im Browser entweder auf Blazor oder auf Angular/React. Kata 13_01
hat den Blazor-Weg abgedeckt — hier kommt der andere. Es geht nicht darum,
Frontend-Entwickler zu werden, sondern darum, die Grenze zwischen .NET-Backend und
TypeScript-Client begruendet ziehen zu koennen.

## Aufgabe: der Trainingsplan im Browser

Dieselbe Oberflaeche zum Kata-Tracker aus Kata 09_01 wie in Kata 13_01 — bewusst derselbe
Funktionsumfang, denn nur dann traegt der Vergleich am Ende. Vier Seiten, nicht mehr:

- **Anmeldung**: ein Knopf, der die Anmeldung am .NET-Host anstoesst. Danach kennt das
  Frontend nur noch den angemeldeten Benutzer, nie ein Token.
- **Startseite** mit den Kennzahlen des Trainingsplans: *aktuelle Streak* aus
  `/api/v1/stats/streak`, *Versuche diese Woche*, *durchschnittliche Dauer der letzten zehn
  Versuche*. Jede Kennzahl laedt fuer sich und zeigt bis dahin einen Ladezustand.
- **Kata-Liste** mit Filter nach Tag und serverseitiger Pagination. Der Filter steht in der
  URL (`/katas?tag=tdd&page=2`), damit die Ansicht teilbar und der Zurueck-Knopf brauchbar
  bleibt. Klick auf eine Zeile fuehrt zur Detailseite.
- **Detailseite einer Kata** mit der Versuchshistorie und dem Formular "Versuch erfassen"
  (Datum, Dauer, Notiz). Der neue Versuch erscheint ohne Reload in der Historie, und die
  Kennzahlen der Startseite sind danach nicht mehr aus dem Cache, sondern neu geholt.

## Voraussetzung

**Muss zuvor erledigt sein:** Kata 09_01 (Minimal API) — Punkt 2 und 5 leben vom
OpenAPI-Dokument einer echten API. Alternativ genuegt eine API mit zwei Endpunkten und
einem gueltigen OpenAPI-Dokument.
**Empfohlen, nicht erforderlich:** Kata 13_01 (Blazor) als Vergleichsobjekt fuer die
Abschlussbewertung, Kata 11_03 fuer den gemeinsamen Build.
**Werkzeuge:** Node.js (npm), optional Docker Desktop.

## Aufbau

```
TypeScriptFrontend/          .NET-Host: BFF, Auth, Reverse Proxy auf die API aus Kata 09_01
  frontend/                  Vite + TypeScript (React oder Angular — deine Wahl, einmal begruenden)
```

Der Host liefert im Release die gebauten Assets aus, im Debug proxied er auf den
Vite-Dev-Server (`SpaProxy` oder YARP).

## Aufgaben

1. **`strict: true`** in der `tsconfig.json`, dazu `noUncheckedIndexedAccess`.
   Kein einziges `any` im Projekt — `unknown` plus Narrowing statt Abkuerzung.
2. **Typen generieren, nicht schreiben:** aus dem OpenAPI-Dokument aus Kata 09_01
   (`openapi-typescript` oder `nswag`). Aendere ein DTO im Backend und zeig, dass der
   Frontend-Build **rot** wird. Das ist der eigentliche Gewinn der Kata.
3. Laufzeitvalidierung an der Grenze (`zod` o.ae.): das generierte Typ-Wissen ist eine
   Behauptung, keine Garantie. Parse die Response, statt sie zu casten.
4. Datenzugriff mit Cache, Deduplizierung und Invalidierung (TanStack Query oder eigener
   kleiner Store). Kein `useEffect` mit `fetch` von Hand.
5. Formular zum Erfassen eines Attempts. Die Validierungsregeln sind **dieselben** wie im
   Backend — leite sie aus dem OpenAPI-Schema ab, statt sie zu duplizieren. Vergleich zu
   dem, was du in Kata 13_01 fuer Blazor gemacht hast.
6. ProblemDetails aus Kata 09_01 typisiert auswerten und feldbezogen anzeigen; `AbortController`
   bricht laufende Requests bei Navigation ab (das Gegenstueck zum `CancellationToken`).
7. **BFF-Pattern**: Token bleibt im `HttpOnly`-Cookie beim .NET-Host, das Frontend sieht
   ihn nie. Erklaere schriftlich, warum ein Access Token im `localStorage` ein XSS-Problem
   ist und was CSRF-Schutz hier noch braucht.
8. Ein Build, ein Artefakt: `npm ci && npm run build` als MSBuild-Target, alles in das
   Docker-Image aus Kata 11_03.

## Tests

- Unit- und Komponententests mit **Vitest** + Testing Library.
- Ein Playwright-E2E-Test gegen den laufenden Compose-Stack: Attempt erfassen, in der
  Liste wiederfinden.

## Nachweise

Eine Seite **Blazor vs. TypeScript-SPA** auf Basis der beiden Katas, die du jetzt gebaut
hast: Bundle-Groesse, Time-to-Interactive, Typsicherheit ueber die Grenze, Teamfaehigkeit,
Oekosystem, Betriebsaufwand. Erfahrung statt Meinung — nur so wird die Wahl belastbar.

## Beispiele und Testfaelle

Jeder Fall unten ist ein Test: Vitest mit gefakter API fuer die Komponentenfaelle, Playwright
gegen den Compose-Stack fuer die Faelle, an denen der .NET-Host beteiligt ist. Der erste Fall
ist kein Testframework-Fall, sondern ein Build — er zaehlt trotzdem.

1. **Vertragsbruch bricht den Build.** Benenne im Backend ein Feld um (`duration` ->
   `elapsed`), generiere die Typen neu, und `npm run build` bzw. `tsc --noEmit` scheitert an
   genau den Stellen, die das Feld lesen. Rueckbenennen macht den Build wieder gruen. Ohne
   Neugenerierung darf der Build **nicht** gruen bleiben — pruefe das mit einem
   CI-Schritt, der die generierte Datei nach der Generierung auf Aenderungen prueft.
2. **Antwort, die nicht zum Vertrag passt.** Die API liefert fuer eine Dauer `null`, obwohl
   das Schema einen Wert verlangt: der `zod`-Parse an der Grenze scheitert, die Seite zeigt
   einen Fehler, und **kein** `undefined` sickert bis in die Anzeige. Der Testfall ist eine
   gefakte Response, kein echter Server.
3. **Validierungsfehler am richtigen Feld.** `POST /api/v1/katas/{id}/attempts` mit Dauer
   `"00:00:00"` -> die API antwortet `400` als ProblemDetails mit `errors["Duration"]`, und
   die Meldung erscheint am Eingabefeld *Dauer* — nicht in einem globalen Fehlerbanner. Ein
   `errors`-Schluessel ohne passendes Feld landet dagegen sichtbar in der Zusammenfassung
   und wird nicht verschluckt.
4. **Ladezustand und Leerzustand.** Waehrend der Request laeuft, zeigt die Kata-Liste einen
   Ladezustand; eine Antwort mit leerer Liste (unbekanntes Tag) zeigt "keine Katas gefunden"
   und nicht denselben Ladezustand fuer immer. Beides sind zwei getrennte Tests mit einer
   Response, die man verzoegern bzw. leer liefern kann.
5. **401 fuehrt zur Anmeldung.** Ein Request mit abgelaufener Sitzung antwortet `401`; die
   App leitet zur Anmeldung, statt eine leere Liste zu rendern. Nach der Anmeldung landet
   der Benutzer auf der urspruenglich angefragten Seite, nicht auf der Startseite.
6. **Das Token bleibt beim Host.** Der Playwright-Test liest nach der Anmeldung
   `localStorage`, `sessionStorage` und `document.cookie` aus: kein Access Token zu finden,
   das Cookie ist `HttpOnly` und im Skript unsichtbar. Trotzdem beantwortet die API den
   Aufruf ueber den BFF mit `200`. Ein manipulierter bzw. fehlender CSRF-Token auf einem
   `POST` liefert dagegen `400`/`403` — und der Versuch entsteht nicht.
7. **Optimistische Aktualisierung wird zurueckgerollt.** Der neue Versuch erscheint sofort
   in der Historie; die API antwortet dann `500`. Danach ist der Eintrag wieder
   verschwunden, ein Fehler ist sichtbar, und die Historie zeigt genau die Versuche von
   vorher — keinen doppelten und keinen halben Eintrag.
8. **Abbruch bei Navigation.** Waehrend der Detailseiten-Request laeuft, wird weg
   navigiert: der `AbortController` bricht ab, die spaet eintreffende Antwort schreibt
   nichts mehr in die verlassene Ansicht, und es gibt keine Warnung ueber ein Update an
   einer nicht mehr vorhandenen Komponente.

## Skills

TypeScript (strict), OpenAPI-Typgenerierung, React oder Angular, BFF-Pattern,
Cookie-Auth, Vitest, Playwright, SPA-Build in .NET

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
