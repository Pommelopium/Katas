# Kata 11_02 — Observability

**Stufe 4: Verteilte Systeme und Betrieb** · Zeitrahmen: 1 Abend

## Ziel

"Laeuft bei mir" ist keine Betriebsstrategie. Ab hier soll das System von aussen erklaerbar
sein.

## Domaene: Kata-Tracker

Dieselbe Codebase wie bisher: der Kata-Tracker aus 09_01, inzwischen mit Outbox und
RabbitMQ aus 11_01. Beobachtbar gemacht wird der Weg eines erfassten Versuchs —
`POST /api/v1/katas/{id}/attempts` schreibt den Versuch samt Outbox-Eintrag, der Publisher
stellt `AttemptRecorded` in den Broker, der Consumer fortschreibt daraus die Streak-Statistik.
Diese eine Kette ist der Gegenstand der Kata: Logs, Traces und Metriken muessen sie von
aussen erklaeren, ohne dass jemand den Code liest.

## Aufgaben

1. **Strukturiertes Logging** mit Serilog. Keine String-Interpolation in Logmeldungen —
   Message Templates mit benannten Properties. Correlation-ID (`TraceId`) haengt an jedem
   Logeintrag eines Requests, auch im Outbox-Consumer.
2. **OpenTelemetry**: durchgehende Traces ueber
   `API-Request -> Handler -> EF-Core-Query -> Outbox -> RabbitMQ -> Consumer`.
   Export nach **Jaeger** (Docker). Trace-Kontext muss ueber die Message-Grenze hinweg
   propagiert werden — das ist der interessante Teil.
3. **Eigene Metriken** ueber `System.Diagnostics.Metrics.Meter`:
   - Counter: Attempts pro Minute
   - Gauge: Outbox-Lag (aelteste unverarbeitete Nachricht in Sekunden)
   - Histogram: Handler-Dauer pro Command-Typ
4. **Health Checks**: `/health/live` (Prozess lebt) und `/health/ready` (DB und Broker
   getrennt geprueft). Unterschied verstehen und begruenden — Kubernetes nutzt beide
   unterschiedlich.

## Beispiele und Testfaelle

Jeder Fall ist automatisiert pruefbar: Traces und Spans ueber einen In-Memory-Exporter bzw.
`ActivityListener`, Metriken ueber `MeterListener` oder `MetricCollector`, Logs ueber eine
Test-Sink. Jaeger ist zum Anschauen da, nicht zum Assertieren.

1. **Eine Trace, erwartete Verschachtelung.** Ein `POST /api/v1/katas/{id}/attempts` erzeugt
   genau **eine** TraceId. Darunter liegen die Spans
   `POST /api/v1/katas/{id}/attempts` -> `RecordAttempt` -> EF-Core-`INSERT` ->
   `Outbox.Publish` -> `AttemptRecorded receive` -> `UpdateStreak`. Geprueft wird die
   Eltern-Kind-Beziehung, nicht nur die Anzahl: jeder Span traegt die ParentSpanId seines
   Vorgaengers, `RecordAttempt` haengt am Request-Span und nicht flach daneben.
2. **Kontext ueber die Message-Grenze.** Der Consumer-Span hat dieselbe TraceId wie der
   Request-Span — und liegt in einer eigenen `Activity` mit dem Publisher-Span als Parent.
   Loescht man die Propagation-Header aus der Nachricht, faellt genau dieser Test um: der
   Consumer bekommt eine neue TraceId. Das ist der Test, der die Kata tatsaechlich absichert.
3. **TraceId in Log und Trace.** Jeder Logeintrag des Requests traegt die Property `TraceId`
   mit demselben Wert wie die Trace — inklusive der Eintraege aus dem Outbox-Consumer, die
   erst Sekunden spaeter entstehen. Umgekehrt: aus einem beliebigen Logeintrag laesst sich per
   TraceId die passende Trace finden. Alle Eintraege liegen als Message Template mit
   benannten Properties vor, keiner enthaelt interpolierten Text.
4. **Fehlerfall.** Wirft der Handler (z. B. unbekannte Kata -> `404`, oder eine erzwungene
   Broker-Stoerung), steht der zugehoerige Span auf Status `Error`, traegt den Exception-Typ
   als Ereignis — und der Eltern-Span ist ebenfalls `Error`. Ein erfolgreicher Request
   hinterlaesst keinen Span mit Status `Error`.
5. **Counter zaehlt.** Zwei erfasste Versuche -> der Counter `katas.attempts.recorded` steht
   auf `2`. Ein fehlgeschlagener Versuch (`400`/`404`) erhoeht ihn **nicht**. Der Gauge
   `katas.outbox.lag_seconds` ist `0`, wenn die Outbox leer ist, und > 0, solange eine
   Nachricht ungesendet liegt.
6. **Histogram-Buckets.** Das Histogram `katas.handler.duration` traegt das Tag
   `command=RecordAttempt` und hat nach drei Aufrufen `Count == 3`. Ein kuenstlich um 300 ms
   verzoegerter Handler landet in einem hoeheren Bucket als der unverzoegerte — geprueft wird
   die Bucket-Verteilung, nicht ein exakter Millisekundenwert.
7. **Kein PII in Attributen.** Weder Span-Attribute noch Log-Properties noch Metrik-Tags
   enthalten Klartext-Benutzerdaten (E-Mail, Name, Roh-SQL-Parameter). Ein Test faehrt einen
   Request mit erkennbarem Wert (`nutzer@example.com`) und behauptet, dass dieser Wert in
   keinem exportierten Telemetrie-Datensatz auftaucht. Metrik-Tags sind zusaetzlich auf
   Werte mit begrenztem Wertebereich beschraenkt — eine Kata-Id als Tag sprengt die
   Kardinalitaet und ist damit ein Fehlerfall.
8. **Health Checks.** `/health/live` antwortet `200`, solange der Prozess laeuft — auch wenn
   die DB weg ist. `/health/ready` antwortet dann `503` und benennt im Body, welche der
   beiden Abhaengigkeiten (DB oder Broker) ausgefallen ist. Beide Faelle je einmal mit
   gestopptem Container geprueft.

## Uebung zum Abschluss

Baue einen realistischen Fehler ein (eine Query ohne Index, die unter Last langsam wird).
Finde ihn **im Trace**, nicht im Debugger. Schreib auf, welche Span dich hingefuehrt hat.

## Voraussetzung

Docker Desktop (Jaeger).

## Skills

Serilog, OpenTelemetry, Distributed Tracing, Context Propagation, Metriken, Health Checks

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
