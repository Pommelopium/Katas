# Kata 09_01 — Minimal API mit vertikalen Slices

**Stufe 3: API, Persistenz, Architektur** · Zeitrahmen: 1–2 Abende

> **Ab hier waechst eine gemeinsame Codebase.** Kata 09_01 bis 13_02 bauen aufeinander auf und
> ergeben am Ende **ein** vorzeigbares System. Dieses Projekt ist der Startpunkt.
> Stelle das `Sdk`-Attribut in der `.csproj` auf `Microsoft.NET.Sdk.Web` um.

## Ziel

Eine HTTP-API bauen, die nach Fachlichkeit geschnitten ist statt nach Technik: jeder
Endpunkt ein eigener vertikaler Slice, Validierung und Fehlerform als Infrastruktur am
Rand, nicht als `if`-Kette im Handler.

## Domaene: Kata-Tracker

Bewusst nicht der uebliche TODO-Klon: Du erfasst, welche Kata du wann in welcher Zeit
geloest hast.

## Endpunkte

```
POST   /api/v1/katas                 Kata anlegen
GET    /api/v1/katas?tag=&page=      Filter + Pagination
GET    /api/v1/katas/{id}            Einzelabruf
POST   /api/v1/katas/{id}/attempts   Versuch mit Dauer erfassen
GET    /api/v1/stats/streak          laengste Serie an aufeinanderfolgenden Tagen
```

## Anforderungen

1. Minimal APIs mit `MapGroup` und `TypedResults` (nicht `Results.Ok`) — typisierte
   Rueckgaben, damit OpenAPI korrekt generiert wird.
2. **FluentValidation** als Endpoint-Filter, nicht als Aufruf im Handler.
3. Fehlerantworten als **ProblemDetails** (RFC 9457), inklusive `errors`-Dictionary bei
   Validierungsfehlern. Ein globaler `IExceptionHandler` fuer unerwartete Fehler.
4. Ordnerstruktur nach **Feature**, nicht nach Technik:
   `Features/Katas/Create/{Endpoint,Request,Validator,Handler}.cs`
5. `IOptions<T>` fuer Konfiguration, mit `ValidateDataAnnotations().ValidateOnStart()` —
   fehlende Konfiguration bricht beim Start ab, nicht beim ersten Request.
6. OpenAPI-Dokument, API-Versionierung ueber `/api/v1`.
7. Pagination als Cursor **oder** Offset — Entscheidung begruenden.

## Tests

Integrationstests mit `WebApplicationFactory<Program>`: echte HTTP-Calls gegen die
In-Memory gehostete App. Persistenz vorerst In-Memory (kommt in Kata 09_02).

## Beispiele und Testfaelle

Jeder Fall unten ist genau so ein Integrationstest: echter Request hinein, Statuscode und
Body geprueft.

1. **Anlegen glueckt.** `POST /api/v1/katas` mit `{ "name": "Bowling", "tags": ["tdd"] }`
   -> `201 Created`, Header `Location: /api/v1/katas/{id}`, Body enthaelt die vergebene
   `id` und noch keine Versuche.
2. **Validierungsfehler.** `POST /api/v1/katas` mit `{ "name": "" }` -> `400 Bad Request`,
   `Content-Type: application/problem+json`, `status: 400` und ein `errors`-Dictionary mit
   Schluessel `Name`. Der Handler wurde nie betreten — der Endpoint-Filter hat vorher
   abgebrochen.
3. **Unbekannte Kata.** `GET /api/v1/katas/{fremde Guid}` -> `404 Not Found` als
   ProblemDetails, nicht als leerer `200`-Body. Ebenso
   `POST /api/v1/katas/{fremde Guid}/attempts` -> `404`, und es entsteht kein Versuch.
4. **Versuch erfassen.** `POST /api/v1/katas/{id}/attempts` mit
   `{ "solvedOn": "2026-03-01", "duration": "00:42:00" }` -> `201 Created`; danach zeigt
   `GET /api/v1/katas/{id}` genau einen Versuch. Dauer `"00:00:00"` oder negativ -> `400`
   mit `errors["Duration"]`.
5. **Paging-Grenzfaelle.** Bei drei angelegten Katas liefert `?page=1&pageSize=2` einen
   `200` mit zwei Eintraegen, `?page=2&pageSize=2` einen `200` mit einem Eintrag und
   `?page=3&pageSize=2` einen `200` mit **leerer** Liste — kein `404`. `?page=0` und ein
   ueberhoehtes `?pageSize=1000` -> `400`. Bei Cursor-Pagination stattdessen: die letzte
   Seite gibt `nextCursor: null` zurueck, ein manipulierter Cursor ergibt `400`.
6. **Filter.** `?tag=tdd` liefert nur Katas mit diesem Tag; ein unbekanntes Tag liefert
   `200` mit leerer Liste, ebenfalls kein `404`.
7. **Idempotenz.** Derselbe Versuch (gleiche Kata, gleiches Datum, gleiche Dauer) zweimal
   gepostet: entscheide, ob daraus ein zweiter Versuch, ein `409 Conflict` oder dieselbe
   Antwort ohne Neuanlage wird — und halte die Entscheidung in einem Test fest. Der Test
   ist der Beleg, dass es eine Entscheidung war und kein Zufall.
8. **Streak.** Versuche an 2026-03-01, -03-02 und -03-03 -> `GET /api/v1/stats/streak`
   liefert `3`; mit Luecke (03-01, 03-02, 03-04) nur `2`; zwei Versuche am selben Tag
   zaehlen einmal; ohne jeden Versuch `0` und nicht `404`.
9. **Rand der Infrastruktur.** Fehlt ein Options-Wert, startet die App gar nicht — der
   Fehler faellt beim Hochfahren der `WebApplicationFactory` an, nicht beim ersten
   Request. Und ein Endpunkt, der eine unerwartete Exception wirft, antwortet mit `500`
   als ProblemDetails ohne Stacktrace im Body.

## Skills

ASP.NET Core, Minimal APIs, FluentValidation, ProblemDetails, DI, Konfiguration, OpenAPI

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
