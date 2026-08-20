# Kata 14_03 — Erbauer (Builder)

**Erzeugungsmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/builder)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und **richtig anwenden** — nicht: es ueberall anwenden. Der Builder
loest ein sehr schmales Problem: ein Objekt entsteht in mehreren Schritten, und derselbe
Ablauf soll unterschiedliche Ergebnisse liefern koennen. Wer das trifft, gewinnt viel; wer
jede Klasse mit einem Builder versieht, hat nur Zeilen produziert. Der zweite Teil des Ziels
ist darum die Gegenprobe: erkennen, wann C# das Problem schon selbst loest.

## Woran du das Muster erkennst

- **Konstruktor-Teleskop:** ein Konstruktor mit 8 Parametern, daneben vier Ueberladungen mit
  6, 5 und 3 Parametern, die sich gegenseitig aufrufen. Am Aufrufort steht
  `new Bericht(a, b, null, null, true, false, null, 3)` und niemand weiss, was `false` war.
- **Halb initialisierte Objekte:** das Objekt existiert nach `new` in einem Zustand, in dem
  es nicht benutzt werden darf, und wird durch eine Folge von Settern nachtraeglich
  fertiggestellt. Vergisst der Aufrufer einen, faellt es erst spaeter und woanders auf.
- **Viele optionale Teile:** Kopf, Abschnitte, Tabellen, Fussnoten — die meisten Kombinationen
  sind erlaubt, aber nicht alle. Diese Regeln haben in einem Konstruktor keinen Platz.
- **Derselbe Bauablauf, unterschiedliche Ergebnisse:** die Reihenfolge der Schritte ist immer
  gleich, nur das Zielformat wechselt. Wird der Ablauf pro Format kopiert, driften die Kopien.
- **Kombinatorische Ueberladungen:** jede neue Option verdoppelt die Zahl der Konstruktoren.

## Aufgabe: der Wochenbericht des Kata-Trackers

Der **Kata-Tracker** (Kata 07_01/07_02) soll einen Wochenbericht ausgeben: eine Kopfzeile mit
Zeitraum und Trainierendem, eine Tabelle der Versuche (Kata-Kuerzel, Datum, Dauer), optional
eine Zusammenfassung mit Gesamtdauer und Streak, optional eine Liste offener Katas und
optional eine Fussnote. Derselbe Bericht wird an drei Stellen gebraucht: als Markdown fuer das
Repository, als schmale Konsolenausgabe fuer das CLI und — neu — als CSV fuer die Auswertung
in der Tabellenkalkulation.

Heute sieht das so aus, und genau das ist der Zustand, den du erkennen sollst:

```csharp
public sealed class Wochenbericht
{
    public Wochenbericht(
        string trainierender, DateOnly von, DateOnly bis, IReadOnlyList<Attempt> versuche,
        bool mitZusammenfassung, bool mitOffenenKatas, string? fussnote, int spaltenbreite)
    { /* ... */ }

    // ... und weil das am Aufrufort unlesbar war, kam die Setter-Variante dazu:
    public Wochenbericht() { }
    public string? Trainierender { get; set; }
    public DateOnly Von { get; set; }
    public DateOnly Bis { get; set; }
    public List<Attempt>? Versuche { get; set; }
    public bool MitZusammenfassung { get; set; }
    public string? Fussnote { get; set; }
    public string Rendern(string format) { /* if (format == "md") ... else if ... */ }
}

// Aufrufort:
var bericht = new Wochenbericht("Lasse", von, bis, versuche, true, false, null, 80);
var markdown = bericht.Rendern("md");

var zweiter = new Wochenbericht { Trainierender = "Lasse", Von = von };
var konsole = zweiter.Rendern("console"); // Versuche ist null — knallt erst hier
```

Beide Zutaten sind da: das Teleskop **und** die Setter-Orgie, dazu ein `Rendern`, das mit
jedem neuen Format waechst.

## Aufgaben

1. Bau den Ausgangszustand oben lauffaehig nach — klein, aber echt. Notier in einem Satz je
   Geruch, was daran konkret weh tut. Ohne diesen Schritt uebst du das Erkennen nicht.
2. Trenne **Produkt** und **Bau**: `MarkdownBericht`, `KonsolenBericht` sind eigene Produkte
   und muessen sich nicht gleichen. Kein gemeinsames Interface erzwingen, wenn keins passt.
3. Definiere die Builder-Schnittstelle als **Bauschritte**, nicht als Property-Setter:
   `Reset()`, `SetzeKopf(...)`, `FuegeVersuchsTabelleHinzu(...)`,
   `FuegeZusammenfassungHinzu(...)`, `FuegeOffeneKatasHinzu(...)`, `SetzeFussnote(...)`.
   `Build()` gehoert an die konkreten Builder, weil die Rueckgabetypen verschieden sind.
4. Implementiere `MarkdownBerichtBuilder` und `KonsolenBerichtBuilder`. `Build()` **validiert
   und liefert** das fertige Produkt und ruft danach `Reset()` — ein Builder ist
   wiederverwendbar, ohne Reste des Vorgaengers.
5. Schiebe die Regeln in `Build()`: Pflichtfelder (Trainierender, Zeitraum, mindestens eine
   Tabelle) und Konsistenz (Zeitraum nicht verdreht). Fehlt etwas, scheitert `Build()` —
   nicht das Rendern und nicht der Aufrufer drei Schichten weiter oben.
6. Baue den **Director** `WochenberichtDirector` mit `BaueWochenrueckblick(IBerichtBuilder)`
   und `BaueKurzfassung(IBerichtBuilder)`. Der Ablauf steht genau einmal im Director; die
   beiden Builder werden hindurchgeschickt und liefern zwei verschiedene Ergebnisse.
7. Ergaenze eine **dritte Repraesentation** `CsvBerichtBuilder`, ohne Director, Interface oder
   die bestehenden Builder und Produkte anzufassen — nur eine neue Klasse und eine Zeile am
   Aufrufort. Gelingt das nicht, sitzt die Naht falsch: korrigiere sie, statt es hinzunehmen.
8. Gegenprobe: schreib denselben Wochenbericht als `record` mit `init`-Properties und
   benannten Parametern. Entscheide begruendet, welche der beiden Loesungen du behalten
   wuerdest — die Begruendung ist Teil des Ergebnisses.

## Beispiele und Testfaelle

- **Vollstaendiger Bauauftrag:** Kopf, Tabelle mit 3 Versuchen, Zusammenfassung, Fussnote ->
  `MarkdownBerichtBuilder.Build()` liefert einen `MarkdownBericht`, dessen Text die
  Tabellenzeile `| 07_02 | 2026-08-19 | 95 |` und eine Gesamtdauer enthaelt.
- **Unvollstaendiger Bauauftrag scheitert beim `Build()`:** nur `SetzeKopf(...)`, keine
  Tabelle -> `Build()` schlaegt fehl. Der Test prueft ausdruecklich, dass der Fehler **beim
  `Build()`** entsteht und nicht erst beim Ausgeben oder Speichern des Produkts.
- **Pflichtfeld fehlt:** Tabelle gesetzt, aber kein Trainierender -> `Build()` schlaegt fehl
  mit einem Hinweis auf genau dieses Feld. Ein `NullReferenceException` beim Rendern gilt als
  nicht bestanden.
- **Verdrehter Zeitraum:** `von = 2026-08-20`, `bis = 2026-08-13` -> `Build()` schlaegt fehl,
  obwohl beide Werte einzeln gueltig sind. Solche Regeln sind der Grund fuer das Muster.
- **Derselbe Director, zwei Ergebnisse:** `BaueWochenrueckblick` mit dem Markdown- und dem
  Konsolen-Builder auf identischen Eingabedaten -> zwei Produkte mit derselben fachlichen
  Aussage und unterschiedlichem Text; die Konsolenfassung haelt zusaetzlich 80 Zeichen
  Zeilenbreite ein.
- **Zwei Ablaeufe, ein Builder:** `BaueWochenrueckblick` und `BaueKurzfassung` mit demselben
  Builder-Objekt hintereinander -> die Kurzfassung enthaelt keine Zusammenfassung und keine
  Fussnote und **keine** Reste des ersten Berichts (Nachweis fuer `Reset()`).
- **Neue Repraesentation:** `BaueWochenrueckblick(new CsvBerichtBuilder())` liefert 4 Zeilen
  (Kopfzeile + 3 Versuche). Der Nachweis ist der Diff: die Tests der Markdown- und
  Konsolenfassung bleiben unveraendert gruen und keine bestehende Datei wurde angefasst.
- **Leere Woche:** Tabelle mit 0 Versuchen, ausdruecklich hinzugefuegt -> `Build()` gelingt und
  das Produkt enthaelt die Aussage "keine Versuche". Unterschied zu Fall 2: der Schritt wurde
  ausgefuehrt, nur ohne Daten.

## Abgrenzung

- **Abstract Factory** erzeugt in *einem* Aufruf eine Familie zusammenpassender Produkte;
  der Builder erzeugt *ein* Produkt in *mehreren* Schritten. Merkmal: braucht es einen
  Zwischenzustand zwischen den Aufrufen, ist es ein Builder.
- **Factory Method** waehlt den konkreten Typ ueber eine ueberschriebene Methode; es gibt
  keine Bauschritte und keine Teilmengen. Ein Builder ohne mehrere Schritte ist eine Factory
  Method mit mehr Zeilen.
- **Prototype** kommt zum fertigen Objekt durch Kopieren, nicht durch Zusammensetzen — die
  richtige Wahl, wenn ein passendes Objekt schon existiert und nur variiert werden soll.
- **Fluent-API ohne Muster:** `Setze...().Setze...().Build()` allein ist noch kein Builder,
  sondern eine Schreibweise. Es fehlen die austauschbaren Builder mit unterschiedlichen
  Produkten und der Director, der den Ablauf besitzt. Umgekehrt gilt: ein Builder braucht
  kein Method Chaining.

## Wann nicht

- **Das Objekt hat drei bis vier Parameter und keine Teilmengen-Regeln.** Dann ist ein
  `record` mit `init`-Properties plus benannten und optionalen Parametern kuerzer, sicherer
  und ohne halb initialisierten Zwischenzustand:
  `new Bericht(Trainierender: "Lasse", Von: von, Bis: bis) { MitZusammenfassung = true }`.
  Varianten entstehen mit einem `with`-Ausdruck statt mit einem zweiten Builder-Durchlauf.
- **Es gibt nur eine Repraesentation und keine Aussicht auf eine zweite.** Ohne den zweiten
  Builder zahlt man Interface, Director und Testaufwand fuer nichts. Warte auf den zweiten
  Fall — er ist die Voraussetzung des Musters, nicht seine Belohnung.
- **Der Bauablauf variiert pro Aufrufer.** Dann besitzt niemand den Ablauf, der Director wird
  zur leeren Huelse und der Builder degeneriert zu einer umstaendlichen Setter-Sammlung.

## Skills

Erzeugungsmuster erkennen und einordnen, Trennung von Bauablauf und Repraesentation,
Invarianten in `Build()` durchsetzen, Director, Erweiterung ohne Aenderung bestehenden Codes,
Sprachmittel als Alternative zum Muster (`record`, `init`, `with`, benannte Parameter)

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
