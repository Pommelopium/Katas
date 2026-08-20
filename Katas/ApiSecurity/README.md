# Kata 12_01 — Authentifizierung und Autorisierung

**Stufe 3: API, Persistenz, Architektur** · Zeitrahmen: ein Wochenende

## Ziel

Die groesste Luecke jeder selbstgebauten Kata-Sammlung und gleichzeitig das Thema, das in
fast jedem realen System vorkommt. Interessant ist nicht der `[Authorize]`-Aufsatz, sondern
die Fragen dahinter: Wer stellt das Token aus, wer prueft es, was steht drin, was passiert
beim Ablauf, und wo ist es zwischen zwei Requests gespeichert.

## Domaene: der abgesicherte Kata-Tracker

Geuebt wird an der Tracker-API aus Kata 09_01: Katas anlegen, Versuche erfassen, Streak
abfragen. Neu ist, dass die Daten jetzt **jemandem gehoeren**. Jede Kata und jeder Versuch
haengen an einem Trainierenden; ein zweiter Nutzer darf sie weder lesen noch aendern. Dazu
kommen zwei weitere Rollen: ein **Trainer**, der die Versuche der ihm zugeordneten
Trainierenden lesen, aber nicht aendern darf, und ein **Hintergrunddienst** (der
Outbox-Consumer aus Stufe 3), der ohne Benutzerkontext Statistiken nachrechnet. Oeffentlich
bleibt genau ein Endpunkt: der Katalog der verfuegbaren Katas
(`GET /api/v1/catalog`) — er dient als Gegenprobe, dass die Absicherung nicht global,
sondern pro Endpunkt greift.

Scopes: `katas:read`, `katas:write`, `stats:read`. Rollen: `trainee`, `coach`, `service`.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Du brauchst nur eine API mit mindestens zwei
Endpunkten — einem oeffentlichen und einem geschuetzten.
**Empfohlen, nicht erforderlich:** Kata 09_01 (Minimal API). Wenn sie geloest ist, sicherst du
diese Endpunkte ab; sonst legst du zwei Endpunkte in 20 Minuten selbst an.
**Werkzeuge:** Docker Desktop (Keycloak als Identity Provider — kein Azure-Abo noetig).

## Minimalpfad

Punkte 1, 2, 4 und 8. Damit ist der Skill freigeschaltet; der Rest ist Ausbau.

## Aufgaben

1. **Token-Validierung selbst verstehen, bevor du sie konfigurierst.** Nimm ein JWT
   auseinander: Header, Payload, Signatur. Pruefe die Signatur einmal von Hand gegen den
   oeffentlichen Schluessel aus dem JWKS-Endpunkt des Providers. Danach das Gleiche mit
   `AddJwtBearer` — und du weisst, was die drei Zeilen Konfiguration tun.
2. Identity Provider (Keycloak im Container) mit Authorization Code Flow **plus PKCE**.
   Schreib auf, welches Problem PKCE loest und warum der Implicit Flow tot ist.
3. Validierung vollstaendig: `Issuer`, `Audience`, Signatur, Ablauf, Clock Skew.
   **Schalte jede Pruefung einmal einzeln ab** und zeig mit einem Test, welcher Angriff
   dadurch moeglich wird. Das ist der Kern der Kata.
4. **Autorisierung in drei Stufen** am selben Endpunkt:
   - rollenbasiert (`RequireRole`) — und warum das schnell nicht mehr reicht
   - claim- und policy-basiert (`RequireClaim`, `AddPolicy` mit `IAuthorizationRequirement`)
   - **ressourcenbasiert** (`IAuthorizationHandler<TRequirement, TResource>`):
     "darf dieser Nutzer *diesen* Attempt aendern?"
   Der dritte Fall ist der, der in echten Systemen fehlt und zu IDOR-Luecken fuehrt.
5. **Refresh-Token-Rotation** mit Wiederverwendungserkennung: ein zweites Mal eingeloester
   Refresh Token invalidiert die ganze Familie. Begruende, warum.
6. Machine-to-Machine: Client Credentials Flow fuer den Outbox-Consumer oder einen
   Hintergrunddienst. Kein Benutzerkontext, andere Scopes.
7. **Secrets richtig:** nichts Geheimes in `appsettings.json`. Lokal `dotnet user-secrets`,
   im Betrieb Umgebung/Key-Vault-Abstraktion (`IConfiguration`-Provider), plus die Frage,
   wie du ein Signaturzertifikat **rotierst**, ohne alle Nutzer auszuloggen (zwei gueltige
   Schluessel im JWKS gleichzeitig).
8. **Baue je eine Luecke absichtlich ein und schliess sie wieder**, jede mit einem Test,
   der den Angriff ausfuehrt:
   - IDOR: fremde `Id` in der Route, ohne Ownership-Pruefung
   - `alg: none` bzw. ein Token, das mit dem falschen Verfahren signiert ist
   - fehlende Audience-Pruefung: ein Token fuer Service A wird bei Service B akzeptiert
   - Mass Assignment: `IsAdmin` im Request-Body, das ins Modell durchschlaegt
   - Rate Limiting am Login-Endpunkt fehlt (Credential Stuffing)
9. Passwoerter, falls du selbst welche speicherst: Hashing mit einem
   Password-Hasher-Verfahren nach Stand der Technik, nie selbst gebaut. Miss die Dauer
   eines Hash-Vorgangs und erklaere, warum "langsam" hier das Feature ist.
10. Ausgabe absichern: Security Headers, CORS bewusst und eng konfiguriert (nicht `*`),
    keine Tokens und keine personenbezogenen Daten in Logs.

## Beispiele und Testfaelle

Jeder Fall unten ist ein Integrationstest gegen die gehostete App: echter Request mit
echtem (oder absichtlich kaputtem) Token hinein, Statuscode und Body geprueft.

1. **Ohne Token.** `GET /api/v1/katas` ohne `Authorization`-Header -> `401 Unauthorized`
   mit `WWW-Authenticate: Bearer`. Derselbe Aufruf auf `GET /api/v1/catalog` -> `200`. Der
   oeffentliche Endpunkt beweist, dass nicht einfach die ganze Pipeline blockt.
2. **Falscher Scope.** Ein Token mit `katas:read`, aber ohne `katas:write`, auf
   `POST /api/v1/katas` -> `403 Forbidden`, nicht `401` — authentifiziert, aber nicht
   berechtigt. Und es entsteht keine Kata; der anschliessende `GET` zeigt die Liste
   unveraendert.
3. **Abgelaufenes Token.** Token mit `exp` in der Vergangenheit -> `401`. Ein Token, dessen
   `exp` nur wenige Sekunden zurueckliegt, wird bei einem Clock Skew von fuenf Minuten
   dagegen noch akzeptiert; mit `ClockSkew = TimeSpan.Zero` nicht mehr. Beide Faelle als
   eigener Test, damit die Skew-Entscheidung sichtbar ist.
4. **Manipulierte Signatur.** Nimm ein gueltiges Token, aendere im Payload ein Zeichen
   (etwa `"role": "trainee"` zu `"coach"`), lass die Signatur unveraendert -> `401`. Ebenso
   ein Token mit `alg: none` und leerer Signatur, und ein mit einem fremden Schluessel
   signiertes Token: beide `401`. Das gleiche gilt fuer ein Token mit fremder `aud` und
   fremdem `iss` — je ein Test pro abgeschalteter Pruefung aus Aufgabe 3.
5. **IDOR.** Nutzer A legt eine Kata an und erhaelt `{id}`. Nutzer B ruft
   `GET /api/v1/katas/{id}` mit seinem eigenen, voll gueltigen Token auf -> keine Daten,
   sondern `404 Not Found` (bewusste Entscheidung: `404` verraet nicht, dass die Id
   existiert; `403` waere die Alternative, dann begruende sie im Test). `PUT` und
   `POST .../attempts` auf dieselbe fremde Id ebenso, und danach ist der Datensatz von A
   unveraendert. Der Trainer von A bekommt auf denselben `GET` ein `200`, darf aber beim
   `PUT` ein `403` — dieselbe Route, drei verschiedene Ergebnisse je Aufrufer.
6. **Rate Limit.** Bei einem Limit von N Anfragen pro Fenster liefern die Aufrufe 1..N ein
   `200`, der Aufruf N+1 ein `429 Too Many Requests` mit `Retry-After`-Header. Nach Ablauf
   des Fensters wieder `200`. Am Login-Endpunkt derselbe Test mit falschen Passwoertern:
   nach N Fehlversuchen `429`, und das Limit zaehlt pro Konto, nicht global — ein zweiter
   Nutzer kommt weiterhin durch.
7. **Fehlerantworten verraten nichts.** Keine der `401`/`403`/`404`-Antworten enthaelt
   Stacktrace, Exception-Typ, SQL, Dateipfad, den erwarteten Scope oder den Hinweis, ob der
   Benutzer existiert. Login mit unbekanntem Benutzer und Login mit falschem Passwort
   liefern denselben Text und ungefaehr dieselbe Antwortzeit. Ein Test greift zusaetzlich
   das Log ab und prueft, dass dort kein Token und keine Mail-Adresse landet.
8. **Mass Assignment und Rotation.** `POST /api/v1/katas` mit `{ "name": "Bowling",
   "ownerId": "<fremde Guid>", "isAdmin": true }` -> die angelegte Kata gehoert dem Aufrufer
   aus dem Token, `isAdmin` wird ignoriert oder mit `400` abgelehnt. Und: liegen zwei
   Schluessel im JWKS, sind Tokens des alten **und** des neuen Schluessels gueltig — nach
   Entfernen des alten Schluessels ergibt das alte Token `401`, das neue weiter `200`.

## Nachweise

Eine Liste **Angriff | Test, der ihn ausfuehrt | Gegenmassnahme | warum sie greift.**
Ohne den ausfuehrenden Test ist die Gegenmassnahme nur eine Behauptung.

## Fertig, wenn

Du erklaeren kannst, was zwischen Klick auf "Anmelden" und dem ersten autorisierten
API-Call passiert — jeden Schritt, jeden Redirect, jedes Token.

## Skills

OpenID Connect, OAuth 2.0, PKCE, JWT-Validierung, Policy- und ressourcenbasierte
Autorisierung, Refresh-Token-Rotation, Secrets-Handling, Schluesselrotation, OWASP-Basics

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
