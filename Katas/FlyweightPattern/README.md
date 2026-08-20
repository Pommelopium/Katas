# Kata 14_11 — Fliegengewicht (Flyweight)

**Strukturmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/flyweight)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

Flyweight ist das einzige Muster im Katalog, das kein Entwurfsproblem loest, sondern ein
**Ressourcenproblem**. Es ist eine Optimierung — und Optimierungen ohne Messung sind
Aberglaube. Diese Kata beginnt deshalb mit einer Zahl und endet mit einer zweiten. Wer die
Umsetzung ohne beide Zahlen abgibt, hat die Kata **nicht** bestanden, auch wenn der Code
lehrbuchmaessig aussieht.

## Ziel

Das Muster **erkennen**, es **korrekt anwenden** — geteilter unveraenderlicher Zustand,
Factory mit Cache, extrinsischer Zustand von aussen — und vor allem: es **nur dann**
anwenden, wenn eine Messung es rechtfertigt. Das dritte Ziel ist das schwerste, weil es
verlangt, ein fertig verstandenes Muster wieder wegzulegen.

## Woran du das Muster erkennst

- Es existieren **sehr viele gleichartige Objekte** gleichzeitig — nicht hundert, sondern
  Hunderttausende bis Millionen, und alle leben im selben Moment im Speicher.
- Dieselben Werte liegen **tausendfach** im Speicher: derselbe Kategoriename, dieselbe
  Farbe, dieselbe Formatvorlage, einmal pro Instanz kopiert.
- **Der Speicher ist die knappe Ressource**, nicht die CPU. Das Programm wird langsam durch
  Allokationen, GC-Druck und Cache-Misses, nicht durch Rechenarbeit.
- Der Zustand laesst sich sauber in zwei Haelften schneiden: **intrinsisch** (gehoert zum
  Wert, gilt fuer viele Objekte gleich, aendert sich nie) und **extrinsisch** (gehoert zur
  konkreten Verwendungsstelle, ist pro Objekt anders).
- Die intrinsische Haelfte hat **wenige verschiedene Auspraegungen** — ein paar Dutzend
  Kategorien fuer eine Million Zeilen. Genau dieses Verhaeltnis ist der Hebel.

## Aufgabe: die Importvorschau des Kata-Trackers

Der **Kata-Tracker** importiert die Uebungsprotokolle eines ganzen Teams: eine Datei mit
**einer Million Zeilen**. Jede Zeile ist ein Versuch — Kata-Kuerzel, Datum, Dauer — und wird
in der Vorschau dargestellt: mit dem Titel der Kata, ihrer Stufe, ihrer Kategorie und der
Anzeigefarbe der Kategorie. Von diesen Darstellungsmerkmalen gibt es genau **40**
verschiedene Kombinationen, eine pro Kata im Trainingsplan.

Heute haelt jede Zeile alles selbst:

```csharp
public sealed class AttemptRow
{
    public string KataCode { get; init; } = "";       // extrinsisch: "07_02"
    public DateOnly Date { get; init; }               // extrinsisch
    public int DurationMinutes { get; init; }         // extrinsisch

    // Ab hier: 40 verschiedene Werte, eine Million Mal abgelegt.
    public string KataTitle { get; init; } = "";      // "Result-Pattern statt Exceptions"
    public string LevelName { get; init; } = "";      // "Stufe 1: Modernes C# und Testbarkeit"
    public string CategoryName { get; init; } = "";   // "Fehlerbehandlung"
    public string ColorHex { get; init; } = "";       // "#3B7DD8"
    public string IconGlyph { get; init; } = "";      // "shield"
    public int SortWeight { get; init; }              // 120
}
```

Eine Million dieser Objekte im Speicher — das ist der Ausgangszustand, den du messen sollst,
**bevor** du eine Zeile Muster schreibst.

## Aufgaben

1. **Bau den Ausgangszustand und miss ihn.** Erzeuge die Million `AttemptRow`-Objekte aus 40
   Kata-Definitionen und ermittle den Speicherbedarf: `GC.GetTotalAllocatedBytes` bzw.
   `GC.GetTotalMemory(true)` vor und nach dem Aufbau. Rechne daraus **Bytes pro Objekt**
   aus und schreib beide Zahlen — Gesamtbedarf und Pro-Objekt-Wert — in die README oder ein
   Protokoll. Diese Zahl ist deine Basislinie; ohne sie ist der Rest wertlos.
2. **Schneide den Zustand.** Ordne jedes Feld schriftlich als intrinsisch oder extrinsisch
   ein. Faustregel zur Pruefung: intrinsisch ist, was fuer zwei Zeilen derselben Kata immer
   gleich ist. Alles andere bleibt an der Zeile.
3. **Zieh das Flyweight heraus:** `KataStyle` traegt Titel, Stufe, Kategorie, Farbe, Glyph
   und Gewicht. `AttemptRow` behaelt Datum und Dauer und haelt **eine Referenz** auf den
   Style statt sechs Kopien seiner Felder.
4. **Erzwinge Unveraenderlichkeit.** Der geteilte Typ wird `sealed` mit ausschliesslich
   `get`-Eigenschaften (oder `readonly record`), ohne Setter, ohne veraenderbare Collection
   nach aussen. Geteilter Zustand, den einer aendern kann, ist ein Fehler, der eine Million
   Zeilen gleichzeitig trifft. Schreib den Test, der das Aendern **nicht kompilieren** laesst,
   als Kommentar in die Testklasse.
5. **Factory mit Cache:** `KataStyleFactory.Get(string kataCode)` legt jede Auspraegung genau
   einmal an und gibt sie danach wieder aus. Der Konstruktor des Flyweights wird `private`
   oder `internal` — niemand darf ihn umgehen. Zaehl die tatsaechlich erzeugten Instanzen mit
   und pruef die Zahl im Test.
6. **Mach den Cache thread-sicher.** Der Import laeuft parallel: `ConcurrentDictionary` mit
   `GetOrAdd`, und beachte, dass die Factory-Lambda dabei mehrfach laufen kann — das Ergebnis
   muss trotzdem **eine** Instanz pro Schluessel sein. Zeig es mit einem Test, der aus 8
   Threads gleichzeitig denselben Schluessel anfragt.
7. **Gib extrinsischen Zustand herein statt ihn zu speichern:** `style.Render(row.Date,
   row.DurationMinutes)`. Das Flyweight darf keinen Zustand der Aufrufstelle behalten — auch
   nicht "nur zum Zwischenspeichern".
8. **Miss erneut und stell die Zahlen gegenueber.** Dieselbe Messung wie in Aufgabe 1, dieselbe
   Million Zeilen. Notiere eine Tabelle mit vorher, nachher und dem Faktor, und rechne nach,
   ob der Faktor zur Feldgroesse passt. Weicht er stark ab, hast du entweder falsch gemessen
   oder etwas anderes gebaut als gedacht — beides ist ein Ergebnis.

## Beispiele und Testfaelle

| Fall | Erwartetes Ergebnis |
|---|---|
| `ReferenceEquals(factory.Get("07_02"), factory.Get("07_02"))` | `true` — zwei Anfragen mit demselben Schluessel liefern **dasselbe** Objekt, nicht zwei gleiche |
| `ReferenceEquals(factory.Get("07_02"), factory.Get("09_03"))` | `false` — verschiedene Schluessel bleiben getrennt |
| 1.000.000 Zeilen aus 40 Kata-Kuerzeln aufgebaut | `factory.CreatedCount == 40`, nicht 41 und nicht 1.000.000 |
| Messung vorher gegen nachher | Gesamtbedarf sinkt um einen **belegten** Faktor (Erwartung: 3-5x bei 6 ausgelagerten Feldern); beide Zahlen stehen im Protokoll |
| Bytes pro Zeile | vorher rund 80-90 Byte (Header, 5 Referenzen, `int`, `DateOnly`), nachher rund 32-40 Byte — die Rechnung wird im Test als Kommentar hergeleitet |
| Verhalten vor und nach der Optimierung | **derselbe** Testlauf gegen dieselbe Erwartung ist in beiden Varianten gruen; die gerenderte Zeile ist Zeichen fuer Zeichen identisch |
| 8 Threads fragen gleichzeitig `Get("07_02")` | alle 8 Ergebnisse sind referenzgleich, `CreatedCount == 1` |
| Der Style eines Eintrags wird geaendert | geht nicht — Kompilierfehler; ein Reflection-Test darf beweisen, dass alle Felder `readonly` sind |
| Gegenprobe mit 40 Zeilen statt 1.000.000 | die Ersparnis verschwindet, der Cache-Overhead bleibt messbar — genau der Fall aus "Wann nicht" |

## Abgrenzung

- **Singleton** garantiert **eine** Instanz eines Typs, global und meist mit Zustand.
  Flyweight erzeugt **viele** Instanzen — eine pro Auspraegung — und teilt sie. 40 geteilte
  Styles sind kein Singleton, auch wenn jeder einzelne von ihnen nur einmal existiert.
- **Cache** speichert Ergebnisse, um sie **nicht neu berechnen** zu muessen; Eintraege duerfen
  ablaufen, verdraengt werden und neu entstehen. Der Flyweight-Cache spart **Speicher** und
  darf gerade **nicht** verdraengen: verschwindet ein Eintrag, entsteht eine zweite Instanz
  und die Referenzgleichheit ist kaputt. Gleiche Datenstruktur, entgegengesetzte Lebensdauer.
- **Object Pool** gibt Objekte **exklusiv** aus und nimmt sie zurueck — zeitliches
  Wiederverwenden veraenderbarer Objekte. Flyweight gibt Objekte **gleichzeitig** an alle aus
  und nimmt nie etwas zurueck; das funktioniert nur, weil sie unveraenderlich sind.
- **Prototype** (14_04) vervielfaeltigt ein Objekt, damit jeder ein eigenes hat. Flyweight
  tut das genaue Gegenteil: es verhindert Kopien. Wenn du beim Lesen "kopieren" denkst,
  ist es Prototype; wenn du "teilen" denkst, ist es Flyweight.

## Wann nicht

- **Ohne Messung gar nicht.** Kein Flyweight auf Verdacht, keine Factory "fuer spaeter".
  Wenn die Basislinie aus Aufgabe 1 fehlt, ist die richtige Entscheidung: nicht anwenden.
- **Bei wenigen Objekten schadet es nur.** Bei ein paar Tausend Instanzen kostet der Cache
  mehr als er spart: Dictionary-Buckets, ein zusaetzlicher Indirektionssprung pro Zugriff,
  und ein Objektgraph, der beim Debuggen schwerer zu lesen ist.
- **Die Unveraenderlichkeitspflicht ist ein echter Preis.** Ab jetzt ist jede fachliche
  Aenderung am geteilten Typ eine Aenderung an einer Million Stellen. Genau deshalb greifen
  in C# oft die eingebauten Mittel: `string`-Interning bzw. `string.Intern` fuer wiederholte
  Texte, `record struct` fuer kleine Werte ohne Header und Referenz, `ArrayPool<T>` bzw.
  `MemoryPool<T>` fuer grosse Puffer und `ReadOnlyMemory<char>`-Slices auf einen einzigen
  Eingabepuffer statt Millionen Teilstrings. Miss diese Alternativen mit — nicht selten
  schlagen sie das handgebaute Flyweight.

## Skills

Strukturmuster erkennen, Speicherprofiling messen statt raten, intrinsischer gegen
extrinsischer Zustand, Unveraenderlichkeit erzwingen, Factory mit Cache, thread-sicheres
`GetOrAdd`, Referenzgleichheit testen, Grenzen einer Optimierung benennen

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
