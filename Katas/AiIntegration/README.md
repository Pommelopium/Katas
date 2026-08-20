# Kata 13_02 — AI-Integration

**Stufe 5: Differenzierung** · Zeitrahmen: ein Wochenende

## Ziel

LLM-Integration sauber und testbar bauen — nicht als Prompt-Bastelei, sondern als normale
Software, mit Abhaengigkeiten hinter Interfaces, Kostenkontrolle und deterministischen
Tests.

## Aufgabe: der Kata-Coach

Ein Assistent, der Fragen zu deinen erfassten Attempts beantwortet
("Woran habe ich zuletzt am laengsten gesessen?", "Was sollte ich als naechstes ueben?").

Der Coach muss vier Arten von Fragen beantworten koennen:

- **Faktenfragen** zu erfassten Attempts ("Wie viele Katas habe ich im Juli geschafft?") --
  aus `GetAttempts`.
- **Fragen zur Serie** ("Wie lange ist meine aktuelle Streak?") -- aus `GetStreak`.
- **Empfehlungsfragen** ("Was sollte ich als naechstes ueben?") -- aus `SuggestNextKata`.
- **Inhaltsfragen** zu Katas ("Welche Kata trainiert Async?") -- aus der Vektorsuche ueber die
  Kata-Beschreibungen, nicht aus dem Weltwissen des Modells.

Die Antwortform ist immer dieselbe: **fliessender Text, tokenweise gestreamt** — kein JSON und
kein Formular. Nennt die Antwort eine Zahl oder eine Kata, muss sie aus einem Tool-Ergebnis
oder einem Suchtreffer stammen; fehlt die Datenlage, sagt der Coach das statt zu raten.

## Aufgaben

1. Chat-Endpoint ueber **Semantic Kernel** oder ein LLM-SDK direkt
   (`Microsoft.Extensions.AI` als Abstraktion ist die aktuell interessanteste Variante).
2. **Tool Calling**: Das Modell darf `GetAttempts`, `GetStreak` und `SuggestNextKata`
   aufrufen — deine **echten** Domaenen-Services aus Kata 09_03, keine Fake-Daten. Die Tools
   laufen durch dieselbe CQRS-Pipeline inklusive Validierung.
3. **Streaming** der Antwort ueber Server-Sent Events an das Blazor-Frontend aus Kata 13_01.
   `CancellationToken` bricht den laufenden LLM-Call ab, wenn der Nutzer wegnavigiert.
4. **RAG**: Embeddings der Kata-Beschreibungen in einer Vektorsuche (pgvector oder
   Azure AI Search), damit "welche Kata trainiert Async?" semantisch funktioniert.
5. **Kosten- und Token-Metriken** ueber die `Meter` aus Kata 11_02: Tokens pro Request,
   Kosten pro Nutzer, Latenz als Histogram. Ein Limit, das bei Ueberschreitung abbricht.
6. Rate Limiting und ein Timeout pro Anfrage.

## Testbarkeit (der eigentlich schwierige Teil)

Der LLM-Aufruf liegt hinter einem Interface und wird in Tests durch einen Fake ersetzt.
**Kein Test darf echtes Geld kosten und kein Test darf nichtdeterministisch sein.**
Teste die Tool-Calling-Logik, indem du die Tool-Call-Antwort des Modells fest vorgibst.

Der Fake ist dabei kein Platzhalter, der irgendetwas zurueckgibt, sondern ein **Skript**: eine
festgelegte Folge von Antworten, die er in dieser Reihenfolge liefert, plus ein Protokoll der
Anfragen, die er bekommen hat. Damit sind beide Richtungen pruefbar -- was das System aus einer
Modellantwort macht (Tool-Aufruf mit welchen Argumenten, welche Text-Chunks am Client) und was
das System an das Modell schickt (wie viele Anfragen, mit welchem Verlauf, mit welchem
Token-Budget). Auch die Vektorsuche und die Uhr liegen hinter Interfaces, damit Embeddings
und Zeitfenster im Test feststehen.

## Beispiele und Testfaelle

- **Vorgegebener Tool-Call fuehrt zum Aufruf des echten Service.** Frage: "Wie lange ist meine
  aktuelle Streak?" Das Fake-Modell antwortet mit genau einem Tool-Call
  `GetStreak(userId: "u1", asOf: "2026-07-15")`. Erwartet: der Handler ruft `GetStreak` genau
  einmal mit exakt diesen Argumenten auf, das Ergebnis geht als Tool-Ergebnis in die zweite
  Anfrage, und die zweite (ebenfalls fest vorgegebene) Modellantwort wird gestreamt.
- **Antwort ohne Tool-Call wird direkt gestreamt.** Frage: "Was ist eine Kata?" Das Fake-Modell
  liefert die Chunks `["Eine ", "Uebung."]` und keinen Tool-Call. Erwartet: kein
  Domaenen-Service wird beruehrt (Aufrufzaehler aller drei Tools = 0), und der Client empfaengt
  zwei SSE-Ereignisse mit genau diesem Text in dieser Reihenfolge.
- **Ungueltige Tool-Argumente laufen in die Validierung.** Das Fake-Modell ruft
  `GetStreak(userId: "")` auf. Erwartet: die CQRS-Validierung lehnt ab, der Fehler geht als
  Tool-Ergebnis zurueck ans Modell (kein Absturz, kein 500), und die Anzahl der
  Modellanfragen bleibt bei zwei.
- **Abbruch des Clients bricht den LLM-Call ab.** Der Client trennt die SSE-Verbindung nach dem
  ersten Chunk. Erwartet: der `CancellationToken`, den der Fake beim Streamen beobachtet, ist
  gesetzt, das Streamen endet nach dem ersten Chunk, und es wird keine weitere Anfrage an das
  Modell gestellt.
- **Token-Limit fuehrt zum definierten Abbruch.** Limit 1000 Tokens pro Anfrage; das
  Fake-Modell meldet nach dem ersten Aufruf 900 verbrauchte Tokens und fordert einen weiteren
  Tool-Call an. Erwartet: die zweite Anfrage wird **nicht** gestellt, der Aufrufzaehler des
  Fakes bleibt bei eins, der Client bekommt eine erkennbare Abbruchmeldung, und der
  Token-Zaehler der `Meter` steht auf 900.
- **Rate Limit greift.** Bei einem Limit von 3 Anfragen pro Minute und Nutzer: die vierte
  Anfrage desselben Nutzers innerhalb der Minute wird abgelehnt (`429`), ohne dass das
  Fake-Modell ein viertes Mal aufgerufen wird. Ein anderer Nutzer kommt im selben Fenster
  weiterhin durch.
- **RAG findet die semantisch passende Beschreibung.** Bei indizierten Kata-Beschreibungen
  liefert die Suche zu "welche Kata trainiert Async?" die Beschreibung der Async-Kata als
  Treffer -- obwohl das Wort "Async" in der Frageformulierung "gleichzeitig laufende
  Aufgaben" gar nicht vorkommt. Der Treffer landet im Prompt, den der Fake protokolliert.
- **Kein Test ruft ein echtes Modell auf.** Die Testsuite laeuft ohne API-Schluessel und ohne
  Netzwerk gruen und ist zweimal hintereinander bit-identisch im Ergebnis. Ein Test, der
  belegt, dass es keinen zweiten Weg gibt: der einzige echte Client wird ausschliesslich in
  der DI-Registrierung der Produktion aufgeloest.

## Skills

Semantic Kernel / `Microsoft.Extensions.AI`, Tool Calling, SSE-Streaming, Embeddings, RAG,
Testbarkeit nichtdeterministischer Abhaengigkeiten, Kostenkontrolle

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
