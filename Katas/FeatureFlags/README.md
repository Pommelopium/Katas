# Kata 11_10 — Feature Flags und Progressive Delivery

**Stufe 4: Verteilte Systeme und Betrieb** · Zeitrahmen: 1 Abend

## Ziel

Deployment und Release trennen. Wer das kann, liefert taeglich statt monatlich — und muss
im Fehlerfall nicht deployen, um zurueckzurollen. Der zweite, unbequemere Teil der Kata:
Feature Flags sind technische Schuld mit Verfallsdatum.

## Domaene: Kata-Tracker

Dieselbe Codebase wie in den uebrigen Katas dieser Reihe: die Kata-Tracker-API aus 09_01, die
Katas anlegt, Versuche erfasst und unter `GET /api/v1/stats/streak` die laengste Serie
aufeinanderfolgender Trainingstage ausrechnet. Gegenstand der Kata ist nicht neue Fachlichkeit,
sondern dieselbe Fachlichkeit in zwei gleichzeitig ausgelieferten Varianten. Konkret bekommt
jeder der vier Flag-Typen aus Punkt 2 einen festen Platz im Tracker:

- **Release Toggle** `NeueStreakBerechnung`: die Streak wird neu implementiert — der alte Pfad
  laedt alle Versuche und zaehlt in C#, der neue laesst die Datenbank gruppieren. Fachlich
  muessen beide **identisch** antworten; das Flag schaltet nur die Implementierung um und wird
  nach dem Rollout geloescht.
- **Experiment / A-B** `VorschlagsStrategie`: der Endpunkt "welche Kata als naechste" schlaegt
  in Variante A die am laengsten nicht geuebte Kata vor, in Variante B eine aus dem
  schwaechsten Themengebiet. Hier ist der Unterschied im Verhalten gewollt und wird ausgewertet.
- **Ops Toggle** `AufwaendigeStatistik`: die teure Jahres-Heatmap ueber alle Versuche laesst
  sich unter Last abschalten, ohne den Rest der API zu beruehren.
- **Permission Toggle** `TeamAuswertung`: die Auswertung ueber mehrere Trainierende gibt es nur
  fuer bestimmte Mandanten — dauerhaft, nicht als Rollout.

Wer 09_01 nicht gebaut hat, nimmt eine eigene Anwendung mit zwei Verhaltensvarianten; die
Aufgaben bleiben dieselben.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Eine Anwendung mit zwei Verhaltensvarianten
genuegt.
**Empfohlen, nicht erforderlich:** Kata 09_01 (Endpunkte zum Schalten), Kata 11_02 (Metriken fuer
Punkt 5), Kata 11_03 (Pipeline).

## Minimalpfad

Punkte 1, 3, 5 und 8.

## Aufgaben

1. `Microsoft.FeatureManagement` mit `IFeatureManager`: ein Flag, das ein Verhalten
   umschaltet, ohne Neustart. Zeig den Unterschied zwischen einem beim Start gelesenen
   `IOptions`-Wert und einem pro Anfrage ausgewerteten Flag.
2. **Vier Arten von Flags** unterscheiden und je eine bauen, weil sie unterschiedliche
   Lebensdauer haben:
   - Release Toggle (kurzlebig, wird geloescht)
   - Experiment / A-B (mittel, wird ausgewertet)
   - Ops Toggle (langlebig, z. B. teure Funktion unter Last abschalten)
   - Permission Toggle (dauerhaft, kundenspezifisch)
   Die Vermischung dieser vier ist der haeufigste Fehler.
3. **Zielgruppen-Filter**: 10 % der Nutzer, eine bestimmte Nutzergruppe, ein Zeitfenster.
   Wichtig: **stabile** Zuordnung — derselbe Nutzer muss bei jedem Request dieselbe Variante
   sehen (Hash auf eine stabile Kennung, nicht Zufall pro Request). Beweise es mit einem
   Test.
4. **Trunk-based**: baue ein unfertiges Feature hinter einem ausgeschalteten Flag ein und
   liefere es aus. Der Code ist in `main`, das Verhalten nicht sichtbar. Genau das ist der
   Sinn.
5. **Progressive Delivery mit Abbruchkriterium**: schalte von 1 % auf 10 % auf 100 %, und
   definiere **vorher**, welche Metrik den Rollout stoppt (Fehlerrate, Latenz-Perzentil).
   Ein Rollout ohne vorher definiertes Abbruchkriterium ist Hoffnung, kein Verfahren.
6. **Kill Switch**: das Flag muss ohne Deployment umschaltbar sein — Konfigurationsquelle,
   die zur Laufzeit nachlaedt (`IConfiguration`-Reload oder ein Anbieter). Miss, wie lange
   es dauert, bis die Umschaltung in allen Instanzen wirkt.
7. **Testen mit Flags** ist die Falle: N Flags bedeuten 2^N Kombinationen. Entscheide, welche
   Kombinationen du testest, und schreib die Regel auf. Tests muessen Flags explizit setzen,
   niemals von der Umgebungskonfiguration abhaengen.
8. **Aufraeumen erzwingen**: jedes Flag bekommt Anlagedatum, Besitzer und Ablaufdatum. Baue
   einen Test oder Analyzer, der bei einem abgelaufenen Release Toggle **rot** wird. Dann
   entferne ein Flag komplett — inklusive des toten Zweigs — und zeig, dass nichts
   uebrigbleibt.
9. Flag-Zustand in Logs und Traces mitschreiben, damit ein Fehlerbericht die Variante
   nennt. Ohne das ist ein A-B-Fehler nicht reproduzierbar.
10. Abgrenzung schriftlich: Feature Flag vs. Konfiguration vs. Berechtigung vs.
    Verzweigung im Code. Wann ist ein Flag das *falsche* Werkzeug?

## Nachweise

Der Test aus Punkt 3 (stabile Zuordnung), der rote Test aus Punkt 8 (abgelaufenes Flag),
und die Rollout-Kette 1 % → 10 % → 100 % mit dem vorher notierten Abbruchkriterium.

## Beispiele und Testfaelle

Jeder Fall unten ist ein automatisierter Test, der von selbst gruen oder rot wird — kein Punkt
davon wird an der Konsolenausgabe abgelesen. Die Tests setzen das Flag immer explizit
(Punkt 7), nie ueber die Umgebungskonfiguration.

1. **Flag aus fuehrt ueber den alten Pfad.** Versuche an 2026-03-01, -03-02 und -03-03 erfasst,
   `NeueStreakBerechnung` auf `false`: `GET /api/v1/stats/streak` liefert `3`, **und** der Test
   belegt, dass die alte Implementierung lief — etwa ueber einen Zaehler an der
   In-Memory-Zaehlung oder eine mitgeschriebene Kennung des Pfades. Ein Test, der nur `3`
   prueft, wuerde beide Varianten durchlassen und beweist deshalb nichts.
2. **Flag an fuehrt ueber den neuen Pfad.** Dieselben Daten, dasselbe Request, Flag auf `true`:
   wieder `3`, aber die Datenbank-Gruppierung wurde ausgefuehrt und die alte Zaehlung **nicht**.
3. **Paritaet beider Pfade.** Fuer 200 erzeugte Versuchsmengen (leer, ein Tag, Luecken, zwei
   Versuche am selben Tag, Monats- und Jahresgrenze, Zeitzonenwechsel) laeuft dieselbe Anfrage
   einmal mit Flag aus und einmal mit Flag an; die Ergebnisse muessen **byteweise gleich** sein.
   Gegenprobe: schleife absichtlich einen Fehler in den neuen Pfad ein (Luecke wird uebersehen) —
   der Test *muss* rot werden. Erst dann ist er ein Paritaetstest.
4. **Prozentualer Rollout ist pro Nutzer stabil.** `VorschlagsStrategie` auf 10 %: derselbe
   Nutzer fragt 100-mal an und bekommt **100-mal dieselbe** Variante — auch nach einem Neustart
   des Prozesses und ueber zwei parallel laufende Instanzen hinweg. Ueber 10.000 verschiedene
   Nutzerkennungen liegt der Anteil bei 10 % ± 1 Prozentpunkt. Gegenprobe: ersetz den Hash durch
   `Random` — der Stabilitaetstest muss sofort fallen, der Verteilungstest bleibt gruen. Genau
   deshalb braucht es beide.
5. **Umschalten zur Laufzeit ohne Neustart.** Die App laeuft, die Konfigurationsquelle wird von
   `false` auf `true` geaendert: die naechste Anfrage nach dem Reload zeigt das neue Verhalten,
   waehrend Prozess-Id und Startzeitpunkt **unveraendert** sind. Zum Vergleich derselbe Wert als
   beim Start gelesenes `IOptions` (Punkt 1) — dort bleibt das Verhalten alt, bis neu gestartet
   wird. Der Kill Switch (Punkt 6) wird genauso geprueft: `AufwaendigeStatistik` auf `false`,
   und die Heatmap antwortet ab der naechsten Anfrage mit der definierten Absage statt zu
   rechnen.
6. **Unbekanntes Flag faellt auf den Standard.** `IsEnabledAsync("GibtsNichtV3")` liefert
   `false` und wirft **nicht** — abgeschaltet ist der sichere Standard. Zusaetzlich ein Test,
   der alle im Code verwendeten Flag-Namen gegen die deklarierte Liste haelt: ein Tippfehler
   macht ihn rot, statt still den alten Pfad zu liefern.
7. **Flag-Dienst nicht erreichbar.** Der entfernte Anbieter antwortet nicht (Timeout) oder mit
   `500`: die Anfrage endet trotzdem mit `200` innerhalb des Zeitbudgets und benutzt den zuletzt
   bekannten Wert, sonst den Standard aus Punkt 6 — kein `500`, kein Haenger, keine Ausnahme aus
   dem Handler. Der Rueckfall wird einmal als Warnung protokolliert, nicht pro Anfrage. Startet
   die App, waehrend der Dienst schon weg ist, gilt der Standard.
8. **Beide Flag-Zustaende in der Testsuite und Ablauf erzwungen.** Der fachliche Kernfall
   (Versuch erfassen, Streak abrufen) laeuft als Matrix ueber `NeueStreakBerechnung` = aus/an und
   ist in **beiden** Zustaenden gruen; die gewaehlte Kombinationsregel aus Punkt 7 steht als
   Kommentar am Test. Dazu der Hygiene-Test aus Punkt 8: ein Release Toggle mit Ablaufdatum in
   der Vergangenheit macht die Suite rot, und nach dem Entfernen des Flags findet eine Suche
   ueber Code und Konfiguration **keinen** Treffer auf den Namen mehr — auch nicht im toten
   Zweig.

## Skills

`Microsoft.FeatureManagement`, Flag-Typen und Lebensdauer, Zielgruppen und stabile
Zuordnung, Trunk-based Development, Progressive Delivery, Kill Switch, Flag-Hygiene

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
