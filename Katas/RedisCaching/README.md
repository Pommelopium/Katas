# Kata 11_06 — Verteilter Cache mit Redis

**Stufe 4: Verteilte Systeme und Betrieb** · Zeitrahmen: 1–2 Abende · baut auf Kata 10_02 auf

## Ziel

Der letzte Schritt der Persistenzkette: die Query, die du in Kata 10_02 optimiert hast, gar
nicht mehr zu stellen. Redis ist dabei der einfache Teil — die interessanten Fragen sind
Invalidierung und Konsistenz, nicht `GetString`.

## Domaene: Kata-Tracker

Dieselbe Codebase wie ab Kata 09_01, dasselbe Schema wie in Kata 09_02/10_02 — `Katas`,
`Attempts`, `TrainingPlans`. Gecacht werden genau zwei Lesepfade, damit die Faelle unten
benennbar bleiben:

- **Kata-Detailsicht** `GET /api/v1/katas/{id}` mit den Versuchen der Kata. Das ist die
  teuerste Query aus Kata 10_02, weil eine Lieblingskata Hunderttausende `Attempts` hat.
  Schluessel: `kata:v2:{id}:attempts`.
- **Streak-Bericht** `GET /api/v1/stats/streak`, der ueber alle Versuche laeuft.
  Schluessel: `stats:v2:streak`.

Geschrieben wird ueber `POST /api/v1/katas/{id}/attempts`. Ein erfasster Versuch macht die
Eintraege **genau dieser einen** Kata ungueltig — und den Streak, der sich dadurch aendern
kann. Die Eintraege aller anderen Katas muessen unberuehrt bleiben; das ist die Grenze, an
der eine zu grob geschnittene Invalidierung auffaellt.

Die Redis-Verwendungen jenseits des Caches haengen an derselben Fachlichkeit: der
Rate-Limit-Zaehler `rl:{nutzer}:attempts:{minute}` begrenzt das Erfassen von Versuchen, der
Idempotenz-Store `idem:{key}` verhindert den doppelt gebuchten Versuch aus Kata 09_01, und
das verteilte Lock `lock:plan:{planId}:recalc` schuetzt die Neuberechnung eines
Trainingsplans davor, auf zwei Instanzen gleichzeitig zu laufen.

## Aufgaben

1. `IDistributedCache` mit Redis im Container. **Erst messen, dann cachen**: nimm die
   teuerste Query aus Kata 10_02 und beziffere, was der Cache bringt.
2. **Cache-Aside** sauber implementieren: Miss, Load, Set — mit
   `AbsoluteExpirationRelativeToNow` **und** `SlidingExpiration` bewusst gewaehlt und
   begruendet.
3. **Cache Stampede**: bei Ablauf duerfen nicht 200 parallele Requests dieselbe Query
   feuern. Provozier es mit einem Lasttest, dann loese es (`SemaphoreSlim` pro Key
   oder `HybridCache`, das genau dafuer da ist). Zeig die Zahl vorher/nachher.
4. **Invalidierung**: ein Schreibvorgang muss den betroffenen Eintrag ungueltig machen.
   Entscheide begruendet zwischen Loeschen und Neuschreiben, und behandle den Fall
   "Schreiben in der DB erfolgreich, Cache-Invalidierung fehlgeschlagen". Das ist die
   eigentliche Schwierigkeit.
5. **Keyschema und Versionierung**: `kata:v2:{id}:attempts`. Erklaere, wie du bei einer
   Formataenderung ohne Downtime umstellst.
6. `HybridCache` (L1 In-Memory + L2 Redis): zeig das Problem, das L1 schafft — zwei
   Instanzen, eine invalidiert, die andere haelt den Wert weiter. Loese es mit dem
   Backplane-/Tag-Mechanismus.
7. Serialisierung bewusst waehlen (`System.Text.Json` mit Source Generator), und einen
   **Kompatibilitaetstest**: ein alter Cache-Eintrag darf die neue Anwendung nicht
   umbringen.
8. Redis jenseits von Cache — je einmal:
   verteiltes **Rate Limiting** (Counter mit TTL, an die Policy aus Kata 08_01 gehaengt),
   ein **Idempotenz-Key-Store** fuer die API aus Kata 09_01,
   ein **verteiltes Lock** mit TTL (und die Notiz, warum das kein sicheres Lock ist).
9. Betrieb: Cache-Hit-Rate als Metrik aus Kata 11_02. **Und der wichtigste Test:**
   stopp Redis im laufenden Betrieb. Die Anwendung muss weiterlaufen — degradiert, nicht
   defekt. Ein Cache-Ausfall darf kein Ausfall sein.

## Beispiele und Testfaelle

Jeder Fall unten ist ein automatisierter Test gegen ein Redis im Container. Beweismittel ist
nie die Wanduhr, sondern ein **Zaehler auf dem Datenzugriff** — ein Repository-Doppel oder
ein `DbCommandInterceptor`, der mitzaehlt, wie oft die Zielquery wirklich lief.

1. **Erster Aufruf trifft die Datenbank, zweiter den Cache.** Zaehler auf 0 setzen, dann
   `GET /api/v1/katas/{id}` zweimal aufrufen: beide Antworten sind inhaltlich gleich, der
   Zaehler steht danach auf **1**. Nach dem ersten Aufruf existiert
   `kata:v2:{id}:attempts` in Redis, vorher nicht.
2. **Ein Schreibvorgang invalidiert genau den betroffenen Schluessel.** Detailsicht von
   Kata A und Kata B je einmal lesen (Zaehler 2, zwei Schluessel gesetzt). Dann
   `POST /api/v1/katas/{A}/attempts`. Danach ist `kata:v2:{A}:attempts` weg (oder
   neugeschrieben, je nach begruendeter Entscheidung), `kata:v2:{B}:attempts` unveraendert.
   Erneutes Lesen von A erhoeht den Zaehler auf 3 und enthaelt den neuen Versuch; erneutes
   Lesen von B laesst den Zaehler stehen. Zweiter Test dazu: schlaegt die Invalidierung fehl
   (Redis waehrend des Schreibens gestoppt oder ein Doppel, das wirft), ist der Versuch
   **trotzdem** in der Datenbank und der Fehlerpfad greift — Antwort bleibt erfolgreich,
   der veraltete Eintrag wird nachtraeglich aufgeraeumt oder laeuft spaetestens per TTL ab.
   Was von beidem gilt, entscheidet der Test.
3. **TTL laeuft ab, der naechste Aufruf laedt neu.** Mit `AbsoluteExpirationRelativeToNow`
   von 2 Sekunden: zwei Aufrufe direkt hintereinander ergeben Zaehler 1, nach Ablauf der
   TTL (ohne jede Schreiboperation) ergibt der dritte Aufruf Zaehler **2**. Derselbe Aufbau
   mit `SlidingExpiration` zeigt den Unterschied: ein Zugriff innerhalb des Fensters
   verlaengert die Lebensdauer, beim absoluten Ablauf nicht.
4. **Cache Stampede: 50 parallele Anfragen, eine Datenbankabfrage.** Cache leeren, die
   Zielquery kuenstlich um 200 ms verlangsamen, dann 50 Anfragen gleichzeitig starten
   (`Task.WhenAll`). Ohne Schutz steht der Zaehler bei **etwa 50** — dieser Lauf gehoert als
   Messung dokumentiert. Mit `SemaphoreSlim` pro Key bzw. `HybridCache` steht er auf
   **genau 1**, und alle 50 Antworten sind identisch. Keine Anfrage laeuft in einen Timeout
   und keine bekommt `null`.
5. **Verteiltes Lock verhindert doppelte Ausfuehrung.** Zwei Instanzen (zwei
   `WebApplicationFactory`-Hosts gegen dasselbe Redis) stossen gleichzeitig
   `lock:plan:{planId}:recalc` an: die Neuberechnung laeuft **einmal**, der Verlierer
   wartet oder gibt sofort auf — und zwar nachweislich, indem ein Ausfuehrungszaehler auf 1
   bleibt. Zweiter Fall: haelt der Gewinner das Lock laenger als die TTL, darf der zweite
   ran — genau der Beleg dafuer, dass das kein sicheres Lock ist. Diese Notiz gehoert in die
   Nachweise.
6. **Redis nicht erreichbar — die Anwendung antwortet weiter.** Container im laufenden Test
   stoppen, dann `GET /api/v1/katas/{id}`: Statuscode **200** mit korrekten Daten aus der
   Datenbank, der Zaehler steigt bei jedem Aufruf (kein Cache), es fliegt keine
   `RedisConnectionException` nach aussen und die Health-Probe der App bleibt gesund
   (Redis-Check `Degraded`, nicht `Unhealthy`). Nach dem Neustart des Containers greift der
   Cache ohne Zutun wieder — der Zaehler bleibt beim zweiten Aufruf stehen.
7. **L1 macht Aerger, das Backplane loest ihn.** Zwei Instanzen mit `HybridCache`: beide
   lesen Kata A (beide haben A im L1). Instanz 1 nimmt einen Versuch auf und invalidiert.
   Ohne Backplane liefert Instanz 2 weiter den alten Stand — der Test haelt genau diesen
   Fehlstand fest. Mit Tag-/Backplane-Invalidierung liefert Instanz 2 nach der
   Benachrichtigung den neuen Stand.
8. **Formatwechsel toetet niemanden.** Leg von Hand einen Eintrag im alten Format unter dem
   alten Schluessel (`kata:v1:{id}:attempts`) und einen defekten Eintrag unter dem neuen ab:
   die Anwendung liefert in beiden Faellen eine korrekte Antwort — der v1-Eintrag wird
   ignoriert, weil `v2` ein anderer Schluesselraum ist, der defekte Eintrag wird als Miss
   behandelt und neu geladen. Kein `500`, keine `JsonException` nach aussen. Dazu der
   Rate-Limit-Zaehler als eigener kleiner Fall: 5 erlaubte Versuche pro Minute, der sechste
   Aufruf ergibt `429`, nach Ablauf der Zaehler-TTL ist wieder einer frei.

## Nachweise

Antwortzeit der Zielquery ohne Cache / mit Cache / bei Stampede, Hit-Rate im Normalbetrieb,
und ein gruener Test mit gestopptem Redis.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Eine langsame Abfrage und ein Schreibvorgang
darauf genuegen — die Langsamkeit darfst du auch simulieren.
**Empfohlen, nicht erforderlich:** Kata 10_02 (eine echte teure Query statt einer
simulierten), Kata 11_02 (Metriken fuer die Hit-Rate).
**Werkzeuge:** Docker Desktop (Redis, SQL Server).

## Skills

Redis, `IDistributedCache`, `HybridCache`, Cache-Aside, Stampede-Schutz,
Invalidierungsstrategien, verteiltes Rate Limiting, Graceful Degradation

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
