# Kata 07_04 — Legacy-Code unter Test bringen

**Stufe 1: Modernes C# und Testbarkeit** · Zeitrahmen: 1–2 Abende

## Ziel

Die Faehigkeit, die im Alltag am haeufigsten gebraucht und am seltensten geuebt wird:
Code aendern, den man nicht geschrieben hat, der keine Tests hat und den man nicht
versteht. Greenfield-Katas trainieren das Gegenteil.

## Aufgabe: der Rettungseinsatz im CSV Viewer

Damit die Kata nicht im Abstrakten bleibt, gibt es einen Auftrag. Der `CsvViewer` aus
Kata 06_01 laeuft "seit Jahren in Produktion": rekursives CSV-Parsing, Namen wie
`iFirstLineOfLastPage`, Lesen, Formatieren und Konsolen-UI in einer Datei, keine Zeile Test.
Der Autor ist nicht mehr im Haus. Und jetzt liegt ein Ticket auf dem Tisch:

> **CSV-1147:** Spaltenbreiten sollen auf 30 Zeichen begrenzt werden, laengere Werte
> abgeschnitten mit `...`. Die Zeilen pro Seite sollen konfigurierbar sein statt fest
> verdrahtet.

Das ist der ganze Auftrag — und trotzdem die kleinste Aenderung, die man in solchem Code
nicht "einfach mal" macht. Der Weg dorthin ist die Kata: erst ein Netz um das *heutige*
Verhalten (inklusive der Fehler), dann Seams, dann das Ticket. Mit `Katas/Hexdump`
funktioniert es genauso; dort lautet das Ticket "Bytes pro Zeile von der festen 16 auf einen
Parameter umstellen".

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Diese Aufgabe steht ausdruecklich fuer sich.
**Material:** nimm `Katas/Hexdump` oder `Katas/CsvViewer` als Ausgangscode — oder eine
echte Klasse aus einem alten Projekt. Je haesslicher, desto besser.

## Die Regel dieser Kata

**Du darfst das Verhalten nicht verstehen muessen, um es abzusichern.** Erst ein Netz,
dann Aenderungen. Wer zuerst "aufraeumt", hat die Kata nicht bestanden — auch wenn es
hinterher schoener aussieht.

## Minimalpfad

Punkte 1, 2, 3 und 7.

## Aufgaben

1. **Charakterisierungstests** (Golden Master): fahre den Ausgangscode mit vielen Eingaben,
   friere die Ausgabe ein und mach sie zum Soll. Nicht "was soll es tun", sondern "was tut
   es heute". Auch die Bugs werden eingefroren — genau darum geht es.
2. Werkzeug dafuer: **Approval Tests** (`Verify` o.ae.) oder ein selbstgebauter Vergleich
   gegen eine `.approved.txt`. Ein bewusster Verhaltenswechsel muss den Test rot machen und
   per Diff sichtbar sein.
3. **Coverage als Landkarte, nicht als Note:** miss, welche Zweige dein Netz *nicht*
   abdeckt, und erweitere die Eingaben gezielt dorthin. Notier die Zweige, die du nicht
   erreichen konntest — das sind die gefaehrlichen.
4. **Seams einziehen**, ohne Verhalten zu aendern. Je einmal:
   - Parameter statt Feld (Dependency als Argument durchreichen)
   - `static`/`DateTime.Now`/`new HttpClient()` hinter eine Abstraktion
     (`TimeProvider`, Interface) — die klassischen Testbarkeitsbremsen
   - Extract Method und Extract Class, jeweils mit gruenem Netz nach jedem Schritt
5. **Jetzt** erst der Bugfix: such einen der eingefrorenen Fehler, aendere ihn bewusst, und
   zeig, dass genau ein Approval-File sich aendert. Ein Fix, der zehn Files anfasst, war
   kein Fix, sondern ein Umbau.
6. **Strangler Fig**: setz eine neue Implementierung parallel neben die alte und leite den
   Aufruf ueber einen Schalter um. Beide Wege muessen fuer dieselbe Eingabe dasselbe
   liefern — beweise es, indem du in einem Testlauf **beide** ausfuehrst und vergleichst
   (Parallel Run / Verifikation im Schatten).
7. **Fuehre ein Aenderungsprotokoll**: pro Schritt eine Zeile — was geaendert, welcher Test
   hat abgesichert, wie lange gebraucht. Nach der Kata liest du es und erkennst, welche
   Schritte dich wirklich vorangebracht haben. Das ist das Ergebnis.
8. Optional, aber praxisnah: der Sprung ueber Frameworkgrenzen. Nimm eine Klasse mit
   `System.Web`-, `ConfigurationManager`- oder `WebClient`-Abhaengigkeiten und bring sie
   nach .NET 10 — abgesichert durch das Netz aus Punkt 1. Notier, was `try-convert` bzw.
   der Upgrade-Assistent nicht kann.

## Beispiele und Testfaelle

Bei dieser Kata ist die Sollvorgabe nicht ausgedacht, sondern gemessen: **erwartet ist, was
der Ausgangscode heute tut.** Nachpruefbar sind darum die beobachtbaren Zustaende.

- **Leere Eingabe:** CSV-Datei mit 0 Bytes bzw. nur einer Kopfzeile -> das heutige Ergebnis
  (leere Ausgabe, Absturz oder Kopfzeile allein) landet unveraendert im `.approved.txt`.
  Wenn es eine Exception ist, wird die Exception das Soll.
- **Zeile mit zu wenigen Feldern:** ein Datensatz hat 3 Spalten, die Kopfzeile 5 -> heutiges
  Verhalten einfrieren. Genau hier steckt typischerweise eine `IndexOutOfRangeException`,
  und genau die ist ab jetzt abgesichert.
- **Feld mit Trennzeichen in Anfuehrungszeichen** (`"Mueller, Anna";42`) -> das (mutmasslich
  falsch aufgeteilte) heutige Ergebnis wird approved. Das ist der **eingefrorene Bug** fuer
  Aufgabe 5.
- **Navigation ueber die Grenzen:** `N` auf der letzten Seite, `P` auf der ersten -> Seite
  bleibt stehen, springt um, oder wirft. Was auch immer davon: es ist reproduzierbar
  festgenagelt, bevor irgendetwas angefasst wird.
- **Hexdump-Grenzfaelle:** 0 Bytes -> keine Zeile; 1 Byte -> eine Zeile mit einem Hex-Wert
  und aufgefuellter ASCII-Spalte; 17 Bytes -> zwei Zeilen, die zweite teilbefuellt. Die
  Spaltenausrichtung ist Teil des Solls, nicht Kosmetik.
- **Seam-Nachweis (Aufgabe 4):** nach dem Einziehen der Naht laeuft mindestens ein Test
  **ohne Dateisystem und ohne Console** — Eingabe als `string`, Ausgabe als Rueckgabewert.
  Vorher unmoeglich, nachher gruen: das ist der messbare Fortschritt.
- **Bugfix-Nachweis (Aufgabe 5):** der Fix am Anfuehrungszeichen-Fall macht **genau ein**
  `.approved.txt` rot, und der Diff zeigt genau die betroffenen Zeilen. Aendern sich mehr
  Files, war es kein Fix.
- **Parallel Run (Aufgabe 6):** alte und neue Implementierung ueber mindestens 100 generierte
  Eingaben -> 0 Abweichungen. Gegenprobe: eine absichtlich eingebaute Abweichung (ein Zeichen
  im Format) muss den Vergleichstest rot machen, sonst vergleicht er nichts.

## Fertig, wenn

Du eine fachliche Aenderung am Code vornehmen kannst und dir **die Tests** sagen, ob du
etwas kaputtgemacht hast — nicht dein Gefuehl.

## Skills

Charakterisierungstests, Approval Tests, Seams, Legacy Refactoring in kleinen Schritten,
Strangler Fig, Parallel Run, Coverage als Werkzeug, Framework-Migration

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
