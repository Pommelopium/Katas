# Kata 11_09 — Resilienz und Chaos

**Stufe 4: Verteilte Systeme und Betrieb** · Zeitrahmen: 1–2 Abende

## Ziel

Kata 08_01 uebt Retry. Retry allein macht ein System unter Last aber **schlimmer**, nicht
besser — ein ueberlasteter Dienst, den alle Aufrufer beharrlich wiederholen, kommt nie
wieder hoch. Diese Kata behandelt die Muster, die das verhindern, und den Nachweis, dass
sie greifen.

## Domaene: Kata-Tracker

Derselbe Kata-Tracker wie bisher, jetzt mit zwei fremden Abhaengigkeiten, die du selbst
schreibst und deren Latenz und Fehlerquote du pro Test einstellst:

- **`KataCatalog`** — liefert zu einer Kata die Stammdaten (Beschreibung, Stufe, empfohlener
  Zeitrahmen). Wird beim Lesen gebraucht: `GET /api/v1/katas/{id}` reichert damit an. Reines
  Lesen, idempotent, und fachlich **entbehrlich** — der Tracker kann eine Kata auch ohne
  Katalogdaten anzeigen. Das ist der Kandidat fuer Fallback und Degradation.
- **`TrainingLog`** — der externe Trainingsplan-Dienst, an den ein erfasster Versuch
  gemeldet wird: `POST /api/v1/katas/{id}/attempts` schickt den Versuch weiter. Schreibend,
  **ohne Idempotenzschluessel nicht wiederholbar** — jeder Retry erzeugt dort einen zweiten
  Eintrag. Das ist der Kandidat fuer Punkt 9.

Beide Aufrufe gehen ueber je eine eigene Resilience Pipeline. Der Versuch selbst wird immer
zuerst in der eigenen Datenbank gespeichert (Outbox aus 11_01, falls vorhanden); wenn der
`TrainingLog` nicht erreichbar ist, darf der Tracker nicht behaupten, der Versuch sei
gemeldet.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Ein Aufrufer und ein absichtlich unzuverlaessiger
Zieldienst genuegen — den Zieldienst schreibst du selbst, mit einstellbarer Latenz und
Fehlerquote.
**Empfohlen, nicht erforderlich:** Kata 08_01 (Retry, Polly), Kata 11_02 (Metriken).

## Minimalpfad

Punkte 2, 3, 5 und 8.

## Aufgaben

1. **Zeig zuerst den Schaden.** Zieldienst auf 80 % Fehlerquote, Aufrufer mit Retry ohne
   Begrenzung. Miss die Anzahl der Requests am Ziel. Das ist der *Retry Storm* — die Zahl
   ist das Argument fuer alles Folgende.
2. **Timeout vor allem anderen.** Ein Aufruf ohne Timeout kann nicht scheitern, nur
   haengen. Setz Gesamt- und Einzelversuch-Timeout und begruende die Beziehung zwischen
   beiden (Timeout x Versuche darf das aeussere Budget nicht sprengen).
3. **Circuit Breaker**: nach N Fehlern oeffnen, nach einer Pause halb oeffnen, bei Erfolg
   schliessen. Zeig alle drei Zustandswechsel im Log. Und beantworte die Frage, die dazu
   immer kommt: was gibst du dem Aufrufer zurueck, waehrend der Kreis offen ist?
4. **Bulkhead**: begrenzte Nebenlaeufigkeit pro Abhaengigkeit. Zwei Abhaengigkeiten, eine
   davon haengt — die andere muss weiter bedient werden. Ohne Bulkhead ist der ganze
   Thread-Vorrat weg (Bezug zu Kata 11_08, Fehler 2).
5. **Fallback und Degradation**: definiere pro Aufruf, was bei endgueltigem Fehlschlag
   passiert — veralteter Cache-Wert, Teilergebnis, leere Liste oder harter Fehler. Schreib
   auf, welcher Fall **nicht** stillschweigend degradieren darf (Geld, Rechte, Loeschen).
6. Alles zusammen als **Resilience Pipeline** (`Microsoft.Extensions.Resilience` / Polly v8)
   in der richtigen Reihenfolge. Vertausch zwei Strategien absichtlich und zeig, dass die
   Reihenfolge das Verhalten aendert — z. B. Retry aussen vs. innen um den Breaker.
7. **Jitter** ist kein Detail: 100 Aufrufer mit identischem Backoff erzeugen einen
   synchronisierten zweiten Ansturm. Miss die Verteilung der Retry-Zeitpunkte mit und ohne
   Jitter.
8. **Chaos-Injektion** als Test: baue Latenz, Fehler und Abbrueche gezielt ein
   (Polly Chaos Strategies oder ein eigener `DelegatingHandler`) und lass die Testsuite
   beweisen, dass die Anwendung dabei **degradiert und nicht faellt**. Ein Test pro Muster
   aus Punkt 2–5.
9. Die unbequeme Frage: welche deiner Operationen darf ueberhaupt wiederholt werden?
   Markiere jede als idempotent oder nicht. Ein Retry um eine nicht-idempotente Operation
   ist ein Bug, kein Schutz.
10. Metriken: Rate offener Kreise, abgelehnte Bulkhead-Anfragen, Retry-Anzahl pro Aufruf.
    Ohne diese drei Zahlen merkt im Betrieb niemand, dass die Resilienz arbeitet.

## Beispiele und Testfaelle

Jeder Fall ist ein automatisierter Test gegen den einstellbaren Fake-Dienst: Zeit ueber
`TimeProvider` bzw. `FakeTimeProvider` gesteuert, gezaehlt wird mit einem Aufrufzaehler im
Fake-Dienst. Kein Test wartet auf echte Sekunden, kein Test schlaeft.

1. **Retry ist begrenzt und zaehlbar.** Konfiguration: 3 Wiederholungen, exponentieller
   Backoff mit Jitter. Der `KataCatalog` antwortet dreimal mit `503` und beim vierten Mal
   mit `200` -> der Zaehler im Fake steht auf genau **4** Aufrufen, der Endpunkt liefert
   `200` mit Katalogdaten. Antwortet er viermal mit `503`, bleibt es bei genau **4**
   Aufrufen — nie 5 — und der Fallback aus Fall 6 greift. Zum Vergleich der Storm aus
   Punkt 1: 100 Client-Requests gegen 80 % Fehlerquote erzeugen unbegrenzt wiederholt mehr
   als 1000 Aufrufe am Ziel, mit Begrenzung hoechstens 400.
2. **Timeout bricht ab, statt zu warten.** Der `KataCatalog` haengt 10 s. Einzelversuch-
   Timeout 2 s, Gesamtbudget 5 s -> der Request ist nach **unter 2,5 s** beantwortet (nicht
   nach 10 s), und im Budget passen hoechstens **2** Versuche (2 s + 2 s), nicht 4. Der Test
   prueft beide Zahlen: verstrichene Zeit und Aufrufzaehler.
3. **Circuit Breaker oeffnet und laesst nichts mehr durch.** Konfiguration: Stichprobe 30 s,
   Mindestdurchsatz 10, Fehlerquote 0,5. Nach **5** Fehlern in 10 Aufrufen ist der Kreis
   offen. Die naechsten **20** Aufrufe erreichen den Fake-Dienst **null** Mal, der Zaehler
   bleibt bei 10, und jeder Aufrufer bekommt sofort (< 50 ms) die definierte Antwort aus
   Fall 6 — nicht `TimeoutException`, nicht 10 s Warten.
4. **Half-Open laesst genau einen Probeaufruf zu.** Pausendauer 30 s, per
   `FakeTimeProvider` vorgespult. Danach **9 parallele** Aufrufe -> genau **1** erreicht den
   Zieldienst, 8 werden abgelehnt. Antwortet der Probeaufruf mit `200`, ist der Kreis
   geschlossen und alle folgenden Aufrufe gehen durch; antwortet er mit `503`, ist der Kreis
   wieder offen und die naechsten 29 s erreicht kein einziger Aufruf das Ziel.
5. **Bulkhead schuetzt die andere Abhaengigkeit.** Grenze 4 gleichzeitige Aufrufe je
   Abhaengigkeit. Der `KataCatalog` haengt, 10 Aufrufe gleichzeitig -> **4** stehen im
   Zieldienst, **6** werden sofort abgelehnt. Zur gleichen Zeit laufen 10 Aufrufe an den
   `TrainingLog`, und **alle 10** glueckt. Ohne Bulkhead scheitern im gleichen Szenario auch
   die `TrainingLog`-Aufrufe.
6. **Fallback liefert eine definierte Ersatzantwort.** Bei endgueltigem Fehlschlag des
   `KataCatalog` antwortet `GET /api/v1/katas/{id}` mit `200`, den eigenen Daten (Name,
   Versuche, Streak) und `catalog: null` plus `degraded: true` — kein `500`, keine leere
   Antwort. Beim `TrainingLog` gilt das Gegenteil: dessen Ausfall wird **nicht**
   stillschweigend degradiert, `POST .../attempts` liefert `201` fuer den lokal
   gespeicherten Versuch, aber `trainingLogSynced: false`, und der Versuch bleibt zur
   spaeteren Meldung vorgemerkt.
7. **Nicht wiederholbare Fehler werden nicht wiederholt.** Der Zieldienst antwortet mit
   `400` -> Zaehler steht auf genau **1**, kein Retry, und der Breaker zaehlt diesen Fehler
   **nicht** als Fehler (100 Aufrufe mit `400` oeffnen den Kreis nie). `503` und `429`
   werden wiederholt, `429` mit `Retry-After: 1` fruehestens nach 1 s. Und der nicht
   idempotente `TrainingLog`-Aufruf ohne Idempotenzschluessel wird gar nicht wiederholt: bei
   `503` genau **1** Aufruf und **1** Eintrag beim Ziel, nicht 4 Eintraege.
8. **Jitter streut die Wiederholungen.** 100 Aufrufer, alle scheitern gleichzeitig, Backoff
   2 s. Ohne Jitter liegen **>= 90** der Wiederholungen in einem 20-ms-Fenster; mit Jitter
   liegt in keinem 100-ms-Fenster mehr als **30 %** der Wiederholungen. Der Test prueft das
   Histogramm der Retry-Zeitpunkte, nicht das Log.

## Nachweise

Eine Tabelle **Stoerung | Verhalten ohne Muster | Verhalten mit Muster | Requests am Ziel.**
Der Retry-Storm aus Punkt 1 muss darin messbar verschwunden sein.

## Skills

Timeout-Budgets, Circuit Breaker, Bulkhead, Fallback, `Microsoft.Extensions.Resilience`,
Jitter, Chaos Engineering, Idempotenz als Voraussetzung fuer Retry

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
