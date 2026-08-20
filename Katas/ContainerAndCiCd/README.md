# Kata 11_03 — Containerisierung und CI/CD

**Stufe 4: Verteilte Systeme und Betrieb** · Zeitrahmen: ein Wochenende

## Ziel

Der Punkt, an dem aus Code ein deploybares Artefakt wird.

## Domaene: Kata-Tracker im Betrieb

Weiter an derselben Codebase: die Kata-Tracker-API aus 09_01 bis 09_04, der Outbox-Versand
aus 11_01 und die Telemetrie aus 11_02. Neu ist nicht die Fachlichkeit, sondern ihre
Betriebsform — dieselbe API, die Katas anlegt und Versuche erfasst, soll jetzt als Image
gebaut, als Compose-Stack samt SQL Server, RabbitMQ und Jaeger hochgefahren, von einer
Pipeline verifiziert und auf einem lokalen Cluster im Rolling Update ausgetauscht werden.
Der fachliche Anwendungsfall, an dem du das alles messen kannst, ist der schmalste, der es
durch das ganze System schafft: `POST /api/v1/katas/{id}/attempts` — ein erfasster Versuch,
persistiert, ueber die Outbox verschickt, im Trace sichtbar. Wenn dieser Aufruf ein
Deployment ueberlebt, hat die Kata ihren Zweck erfuellt.

## Aufgaben

1. **Multi-Stage `Dockerfile`**: Build-Stage mit SDK, Runtime-Stage mit
   `runtime-deps`/`aspnet`. Non-Root-User. Ziel: Image unter 120 MB (Alpine oder chiseled).
   Layer-Caching so anordnen, dass ein Codeaenderung kein `restore` ausloest.
2. **`docker-compose.yml`**: API + SQL Server + RabbitMQ + Jaeger, mit `depends_on` und
   echten Healthchecks (nicht `sleep`).
3. **.NET Aspire** als Alternative aufsetzen. Vergleich schriftlich festhalten: Was nimmt
   Aspire ab, was kostet es an Kontrolle?
4. **GitHub Actions**:
   `Build -> Unit-Tests -> Integrationstests (Testcontainers) -> Coverage-Gate ->
   Image bauen -> nach GHCR pushen, getaggt mit Git-SHA`.
   Der Coverage-Gate muss den Build tatsaechlich rot machen koennen.
5. **Kubernetes-Manifeste**: Deployment, Service, ConfigMap, Secret, Liveness- und
   Readiness-Probes (die aus Kata 11_02), Resource Requests und Limits. Lokal auf **kind**
   oder Docker Desktop laufen lassen.
6. **Graceful Shutdown**: `SIGTERM` beendet laufende Requests sauber
   (`HostOptions.ShutdownTimeout`, `IHostApplicationLifetime`).

## Nachweis

Rolling Update **ohne einen einzigen fehlgeschlagenen Request**. Fahre waehrend des
Deployments einen Lasttest (z. B. **k6**) und zeige 0 Fehler. Wenn Fehler auftreten:
Readiness-Probe und Shutdown-Handling pruefen — genau dort liegt es fast immer.

## Beispiele und Testfaelle

Jeder Fall unten ist ein Skript oder ein Pipeline-Schritt, der von selbst gruen oder rot
wird — kein Punkt davon wird "durch Hinsehen" geprueft.

1. **Stack steht von allein.** `docker compose up -d` auf einer Maschine ohne Vorbereitung:
   `GET /health/ready` antwortet **innerhalb von 60 Sekunden** mit `200`, und zwar beim
   ersten Versuch nach dem Healthcheck-Signal — nicht erst nach einem Retry-Skript. Kein
   `sleep` in der Compose-Datei: entfernst du testweise den Healthcheck von SQL Server,
   muss die API erkennbar scheitern statt zufaellig durchzukommen.
2. **Image-Groesse als Gate.** `docker image inspect` liefert eine Groesse **unter 120 MB**.
   Der Wert wird in der Pipeline gegen die Grenze geprueft; ein Anstieg darueber macht den
   Build rot. Fuer den Vergleich einmal die naive Single-Stage-Variante bauen und beide
   Zahlen nebeneinander notieren.
3. **Layer-Caching wirkt.** Aendere eine einzelne Zeile in einer `.cs`-Datei und baue neu:
   die `restore`-Schicht kommt aus dem Cache (`CACHED` im Build-Log), und der Build ist
   deutlich kuerzer als der Erstbau. Aendert sich dagegen die `.csproj`, laeuft `restore`
   wieder — genau so soll es sein.
4. **Kein Root im Container.** `docker exec <container> id -u` gibt einen Wert **ungleich 0**
   zurueck. Ein Schreibversuch in einen Pfad, der dem Prozess nicht gehoert, scheitert mit
   `Permission denied` — der Beleg, dass der Non-Root-User nicht nur deklariert ist.
5. **Fehlende Konfiguration bricht ab.** Startest du das Image ohne die Verbindungszeichenfolge
   (bzw. ohne einen Pflichtwert aus `IOptions`, siehe 09_01), **endet der Container mit einem
   Exit-Code ungleich 0** und einer Meldung, die den fehlenden Schluessel nennt. Er darf nicht
   laufen und dabei `503` liefern. Als Test: Container starten, `docker wait` auswerten.
6. **Roter Test stoppt die Pipeline.** Baue einen Test absichtlich rot ein und pushe: der
   Workflow bricht **vor** dem Image-Push ab, und in GHCR entsteht kein Tag fuer diesen SHA.
   Dasselbe fuer das Coverage-Gate: Coverage einen Prozentpunkt unter die Schwelle druecken —
   der Job muss fehlschlagen, obwohl alle Tests gruen sind.
7. **Reproduzierbares Image.** Zweimal derselbe Commit, zweimal gebaut: das Image traegt den
   Tag mit dem Git-SHA, und beide Builds ergeben **dieselben Layer-Digests** fuer alles, was
   nicht vom Zeitstempel abhaengt (`SOURCE_DATE_EPOCH`, deterministische Assembly-Attribute).
   Wo Reproduzierbarkeit nicht erreichbar ist, notier begruendet, welcher Schritt sie bricht.
8. **Rolling Update ohne Ausfall.** Auf kind: laufender k6-Lasttest gegen
   `POST /api/v1/katas/{id}/attempts`, dazu `kubectl set image` auf den neuen SHA. Ergebnis
   **0 fehlgeschlagene Requests**, und die Anzahl erfasster Versuche in der Datenbank stimmt
   genau mit der Anzahl gesendeter Requests ueberein. Gegenprobe: Readiness-Probe entfernen
   und den Lauf wiederholen — jetzt *muessen* Fehler auftreten. Wenn nicht, testet der
   Lasttest nichts.

## Voraussetzung

Docker Desktop, kind (oder Docker-Desktop-Kubernetes), GitHub-Repository.

## Skills

Docker, Compose, .NET Aspire, GitHub Actions, Kubernetes, Graceful Shutdown, Lasttest

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
