# Kata 13_01 — Blazor-Dashboard

**Stufe 5: Differenzierung** · Zeitrahmen: 1–2 Abende

## Ziel

Ein Frontend fuer den Kata-Tracker — vollstaendig in C#, ohne den JavaScript-Stack.

## Aufgabe: das Trainings-Dashboard

Die Oberflaeche zum Kata-Tracker aus Kata 09_01: die Ansicht, in der du morgens siehst, wo
dein Trainingsplan steht, und abends einen geloesten Versuch erfasst.

Drei Ansichten, nicht mehr:

- **Startseite** mit vier Kacheln: *aktuelle Streak* (Tage in Folge, aus
  `/api/v1/stats/streak`), *Versuche diese Woche*, *durchschnittliche Dauer der letzten zehn
  Versuche* und *laengste Pause*. Dazu das Balkendiagramm "Attempts pro Woche". Jede Kachel
  laedt ihre Zahl selbst und zeigt bis dahin einen Ladezustand — eine langsame Kachel darf
  die Seite nicht blockieren.
- **Kata-Liste** als `QuickGrid` mit Spalten Name, Tags, letzter Versuch, Anzahl Versuche.
  Filter nach Tag und Freitext, Pagination serverseitig. Der aktuelle Filter steht in der
  URL (`/katas?tag=tdd&page=2`), damit die Ansicht teilbar und der Zurueck-Knopf brauchbar
  ist. Klick auf eine Zeile fuehrt zur Detailseite.
- **Detailseite einer Kata** mit der Versuchshistorie und dem `EditForm` "Versuch erfassen"
  (Datum, Dauer, Notiz). Nach dem Speichern erscheint der neue Versuch ohne Reload in der
  Historie, und die Streak-Kachel auf der Startseite ist beim naechsten Besuch aktuell.

## Aufgaben

1. **Blazor Web App** mit bewusst gewaehltem Render Mode. Entscheide zwischen
   `InteractiveServer`, `InteractiveWebAssembly` und `InteractiveAuto` und **begruende es
   schriftlich** — das ist die entscheidende Frage bei Blazor.
   (Stichworte: Time-to-Interactive, Offline-Faehigkeit, SignalR-Verbindung pro Nutzer.)
2. **Typisierter API-Client**, generiert aus dem OpenAPI-Dokument aus Kata 09_01
   (`kiota` oder `nswag`). Kein handgeschriebener `HttpClient`-Aufruf in Komponenten.
3. Uebersichtsseite: Kata-Liste mit Filter und Pagination, serverseitig ausgewertet
   (`QuickGrid` mit `ItemsProvider`, nicht alles laden und clientseitig filtern).
4. **`EditForm`** zum Erfassen eines Attempts. Die Validierungsregeln muessen **dieselben**
   sein wie im Backend — teile die Regeln ueber ein gemeinsames Projekt, statt sie zu
   duplizieren.
5. Diagramm: Attempts pro Woche, plus die Streak aus `/api/v1/stats/streak`.
6. Fehlerbehandlung im UI: ProblemDetails aus Kata 09_01 auswerten und feldbezogen anzeigen.
7. Ladezustaende und `CancellationToken` bei Navigation waehrend eines laufenden Requests.

## Tests

Komponententests mit **bUnit**: Rendering, Interaktion, Validierungsfehler. Der API-Client
wird dabei gefaked.

## Beispiele und Testfaelle

Jeder Fall unten ist genau ein bUnit-Test: Komponente mit gefaketem API-Client rendern,
gerenderte Ausgabe pruefen — und pruefen, was der Fake **nicht** zu sehen bekommen hat.

1. **Pflichtfeld leer.** Im `EditForm` "Versuch erfassen" Datum gesetzt, Dauer leer,
   Absenden -> genau **eine** Fehlermeldung im Markup (bei Dauer), und der gefakete Client
   hat **keinen** Aufruf erhalten. Mit gueltiger Dauer -> genau ein Aufruf mit den
   eingegebenen Werten, keine Fehlermeldung.
2. **Gleiche Regel wie im Backend.** Dauer `00:00:00` und eine negative Dauer werden im
   Formular abgelehnt, ohne dass ein Request rausgeht — dieselbe Regel, die Kata 09_01
   serverseitig mit `400` beantwortet. Der Test liegt gegen die **geteilten** Regeln, nicht
   gegen eine Kopie im Frontend.
3. **Ladezustand.** Der Fake haelt die Abfrage der Kata-Liste offen: gerendert ist ein
   Ladeindikator und keine Tabelle. Nach dem Aufloesen der Antwort ist der Indikator
   verschwunden und die Zeilen sind da. Der Test prueft beide Zustaende, nicht nur den
   zweiten.
4. **Leerzustand.** Liefert die API eine leere Liste, zeigt die Ansicht den Text "Noch keine
   Kata erfasst" und **kein** Tabellengeruest mit null Zeilen. Ebenso auf der Detailseite:
   eine Kata ohne Versuche zeigt den Leerzustand der Historie.
5. **API-Fehler.** Der Fake antwortet mit ProblemDetails `500`: das Rendering wirft keine
   Ausnahme, sondern zeigt eine Fehlermeldung mit Wiederholen-Knopf. Bei `400` mit
   `errors["Duration"]` erscheint der Text **am Feld** Dauer, nicht als globaler Banner.
6. **Streak-Kachel.** Vorgegebene Attempts am 2026-03-01, -03-02 und -03-03 -> die Kachel
   zeigt `3`. Mit Luecke (03-01, 03-02, 03-04) zeigt sie `2`, zwei Versuche am selben Tag
   zaehlen einmal, ohne Versuch steht dort `0` und kein leerer Text.
7. **Filter aus der URL.** Navigation auf `/katas?tag=tdd&page=2` rendert die Liste mit
   vorbelegtem Tag-Filter, und der `ItemsProvider` wurde mit `tag=tdd` und der zweiten Seite
   aufgerufen. Umgekehrt: Filter im UI auf `async` gesetzt -> die URL lautet danach
   `/katas?tag=async&page=1`, die Seite ist auf 1 zurueckgesprungen.
8. **Abbruch bei Navigation.** Waehrend eine Abfrage laeuft, wird die Komponente entsorgt:
   der uebergebene `CancellationToken` ist abgebrochen, und es gibt keine Zustandsaenderung
   und keine Fehlermeldung mehr auf der verlassenen Seite.

## Skills

Blazor, Render Modes, `EditForm`, `QuickGrid`, OpenAPI-Client-Generierung, bUnit

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
