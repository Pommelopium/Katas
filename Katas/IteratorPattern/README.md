# Kata 14_15 — Iterator (Iterator)

**Verhaltensmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/iterator)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen**, es einmal **von Hand bauen** — und danach die Sprachvariante nutzen und
**den Unterschied benennen koennen**. Iterator loest genau ein Problem: eine Sammlung
durchlaufen, ohne dass der Aufrufer weiss, wie sie innen aufgebaut ist. In C# ist das Muster in
die Sprache eingebaut (`IEnumerable<T>`, `yield return`, `foreach`), und genau das macht die Kata
lehrreich: Erst schreibst du `MoveNext` selbst, um zu verstehen, was der Compiler dir sonst
abnimmt. Dann ersetzt du es durch `yield return`. Zum Schluss liest du am generierten Code ab,
dass beides dasselbe ist — nur dass die eine Variante 60 Zeilen braucht und die andere sechs.

## Woran du das Muster erkennst

- Der Aufrufer kennt die **interne Struktur** der Sammlung: er greift auf `_items`, `_buckets`
  oder `Root.Children` zu, weil die Sammlung ihm nichts anderes anbietet.
- **Indexschleifen ueber Fremdstrukturen**: `for (var i = 0; i < plan.Items.Count; i++)` steht in
  jedem Aufrufer, und beim Wechsel von `List<T>` auf `Dictionary<K,V>` oder auf einen Baum muss
  jede dieser Schleifen angefasst werden.
- Es gibt **mehrere sinnvolle Traversierungsarten** ueber derselben Struktur (Tiefe zuerst,
  Breite zuerst, nur Blaetter, rueckwaerts) — und sie werden ueber Flag-Parameter oder eine
  Kopie der Schleife unterschieden.
- Die Sammlung **soll ihre Interna nicht offenlegen**, tut es aber trotzdem: eine oeffentliche
  `List<T>` als Property ist eine Einladung, von aussen einzufuegen und zu sortieren.
- Zwei Durchlaeufe zur gleichen Zeit gehen schief, weil der Positionszeiger in der **Sammlung**
  liegt statt im Iterator: der zweite Durchlauf setzt den ersten zurueck.

## Aufgabe: der Trainingsplan-Durchlauf im Kata-Tracker

Der **Kata-Tracker** haelt den Trainingsplan als Baum: eine **Stufe** enthaelt **Ordner**, ein
Ordner enthaelt weitere Ordner oder einzelne **Katas**. Die Oberflaeche braucht denselben Baum in
drei Reihenfolgen: als eingerueckte Liste (Tiefe zuerst), als stufenweise Uebersicht "was kommt
als naechstes" (Breite zuerst) und als reine Aufgabenliste (nur Blaetter). Dazu kommt der
Endlos-Modus des Wiederholungstrainers, der Katas zyklisch vorschlaegt, bis der Benutzer aufhoert.

Heute macht das der Aufrufer selbst:

```csharp
public sealed class PlanPrinter
{
    public List<string> Titel(TrainingsplanOrdner ordner)
    {
        var ergebnis = new List<string>();

        // Der Aufrufer kennt die interne Liste und laeuft sie per Index ab.
        for (var i = 0; i < ordner.Kinder.Count; i++)
        {
            var kind = ordner.Kinder[i];
            ergebnis.Add(kind.Titel);

            if (kind is TrainingsplanOrdner unterordner)
            {
                // Handrekursion — dieselbe Schleife steht noch in ZaehleKatas und in Naechste.
                ergebnis.AddRange(Titel(unterordner));
            }
        }

        return ergebnis;
    }
}
```

Das ist der Zustand, den du erkennen sollst. `Kinder` ist eine oeffentliche `List<>`, die
Reihenfolge ist im Aufrufer festgelegt, und eine zweite Reihenfolge bedeutet eine zweite Kopie
dieser Methode — oder einen `bool breiteZuerst`-Parameter, was noch schlechter ist.

## Aufgaben

1. Bau den Ausgangszustand nach — `KataKnoten` als Blatt, `TrainingsplanOrdner` mit oeffentlicher
   `Kinder`-Liste, den `PlanPrinter` mit Indexschleife und Handrekursion — und schreib die Tests
   fuer die Beispiele unten gegen diesen Stand. Sie bleiben dein Netz fuer alles Folgende.
2. Schreib den Iterator **von Hand**: eine Klasse `TiefeZuerstIterator : IEnumerator<KnotenInfo>`
   mit `MoveNext()`, `Current`, `Reset()` und `Dispose()`. Der Zustand (der explizite Stack, die
   Position) gehoert in den Iterator, nicht in die Sammlung. Zaehl die Zeilen, wenn du fertig bist.
3. Ersetz genau diesen Iterator durch eine Methode mit `yield return`. Gleiche Tests, gleiche
   Reihenfolge, deutlich weniger Code. Notier in zwei Saetzen, welche Arbeit der Compiler dir
   abgenommen hat — Zustandsmaschine, `Current`, `Dispose`, `IEnumerable`-Doppelrolle.
4. Bau die Traversierungen als **benannte Properties** auf der Sammlung, nicht als
   Flag-Parameter: `plan.TiefeZuerst`, `plan.BreiteZuerst`, `plan.NurKatas`. Jede liefert ein
   eigenes `IEnumerable<KnotenInfo>`. Ein `bool`- oder `enum`-Parameter an *einer* Methode ist die
   Loesung, die du hier bewusst verwirfst — begruend schriftlich, warum.
5. Mach die Sammlung `foreach`-faehig: `TrainingsplanOrdner` implementiert `IEnumerable<KnotenInfo>`
   mit der Standardreihenfolge (Tiefe zuerst). Ab hier ist `Kinder` **nicht mehr oeffentlich** —
   das ist das Abnahmekriterium. Kein Aufrufer darf noch einen Index sehen.
6. Leg das Verhalten bei **Aenderung der Sammlung waehrend der Iteration** fest: fuehr einen
   Versionszaehler ein, der bei `Fuege...Hinzu` und `Entferne` hochgezaehlt wird, und lass
   `MoveNext` eine `InvalidOperationException` werfen, wenn sich die Version seit dem Start des
   Durchlaufs geaendert hat. Schreib dazu, welche Alternative du verworfen hast (Snapshot beim
   Start, oder undefiniertes Verhalten) und was sie gekostet haette.
7. Bau einen **unendlichen** Iterator: `plan.Wiederholungsschleife` liefert die Katas zyklisch,
   ohne Ende. Beweise die **verzoegerte Auswertung** — ein Zaehler im Iterator zeigt, dass bei
   `.Take(3)` genau drei Elemente erzeugt wurden. Beweise ausserdem, dass der Iterator vor dem
   ersten `MoveNext` **gar nichts** tut: der Aufruf von `plan.Wiederholungsschleife` allein
   erhoeht den Zaehler nicht.
8. Optional: bau einen zweiten Sammlungstyp mit voellig anderer Innenstruktur (Ringpuffer aus
   `Katas/Ringpuffer` oder ein `Dictionary`) und lass dieselbe Auswertungsfunktion darauf laufen —
   ohne eine Zeile in der Auswertung zu aendern. Das ist der eigentliche Gewinn des Musters.

## Beispiele und Testfaelle

Referenzbaum `Stufe 1`: `Grundlagen` (Katas `FizzBuzz`, `Bowling`) und `Testbarkeit`
(Kata `LegacyRescue` sowie der Ordner `Vertiefung` mit Kata `SpanCsvParser`) — acht Knoten,
Reihenfolge des Einfuegens.

- **Leere Struktur:** `new TrainingsplanOrdner("Reserve")` -> alle drei Traversierungen liefern
  eine leere Folge. Kein `null`, keine Exception, und `foreach` laeuft null Mal durch. Auch
  `NurKatas.Any()` ist `false`.
- **Ein Element:** Ordner `Grundlagen` mit nur `FizzBuzz` -> `TiefeZuerst` = `[Grundlagen,
  FizzBuzz]`, `NurKatas` = `[FizzBuzz]`. Der Wurzelknoten selbst zaehlt mit, das Blatt nicht als
  Ordner — leg diese Entscheidung im Test fest, sonst verschiebt sie sich staendig.
- **Tiefe zuerst gegen Breite zuerst:** auf `Stufe 1` sind die Reihenfolgen unterschiedlich, und
  genau das ist der Nachweis:
  - `TiefeZuerst` = `[Stufe 1, Grundlagen, FizzBuzz, Bowling, Testbarkeit, LegacyRescue,
    Vertiefung, SpanCsvParser]`
  - `BreiteZuerst` = `[Stufe 1, Grundlagen, Testbarkeit, FizzBuzz, Bowling, LegacyRescue,
    Vertiefung, SpanCsvParser]`
  - `NurKatas` = `[FizzBuzz, Bowling, LegacyRescue, SpanCsvParser]`
  - alle drei enthalten dieselbe **Menge** an Katas (4) — nur die Reihenfolge unterscheidet sich.
- **Zwei gleichzeitig laufende Iteratoren stoeren sich nicht:** hol zwei Enumeratoren auf
  derselben Sammlung, ruf auf dem ersten dreimal `MoveNext`, auf dem zweiten einmal ->
  `[erster.Current, zweiter.Current]` = `[FizzBuzz, Stufe 1]`. Danach den ersten weiterlaufen
  lassen: er macht bei `Bowling` weiter, unbeeindruckt vom zweiten. Wenn der Positionszeiger in
  der Sammlung liegt, faellt dieser Test um.
- **Verschachtelter `foreach` ueber derselben Sammlung** liefert 8 x 8 = 64 Paare. Derselbe
  Nachweis wie oben, nur ohne Handarbeit am Enumerator.
- **Aenderung waehrend der Iteration:** starte `foreach` ueber `Stufe 1`, fuege beim zweiten
  Element eine Kata in `Grundlagen` ein -> der naechste `MoveNext` wirft
  `InvalidOperationException`. Gegenprobe: eine Aenderung **nach** dem Ende des Durchlaufs ist
  erlaubt, und ein Durchlauf, der komplett vor der Aenderung liegt, bleibt gruen.
- **Verzoegerte Auswertung nachgewiesen:** `plan.Wiederholungsschleife.Take(3).ToList()` liefert
  `[FizzBuzz, Bowling, LegacyRescue]`, und der Erzeugungszaehler im Iterator steht danach auf
  genau `3` — nicht auf 4 und nicht auf `int.MaxValue`. Vor dem ersten `MoveNext` steht er auf
  `0`, obwohl `Wiederholungsschleife` schon aufgerufen wurde. `Take(6)` liefert die vier Katas
  plus `FizzBuzz, Bowling`: der Zyklus beginnt neu.
- **Kein Index mehr im Aufrufer:** ein Test bzw. eine Suche ueber den Aufrufer-Code findet kein
  `[i]`, kein `.Count` und keinen Zugriff auf `Kinder`. Das ist der messbare Abschluss von
  Aufgabe 5, und `Kinder` ist ab da nicht mehr oeffentlich.

## Abgrenzung

- **Composite** (Kata 14_08) baut die **Struktur** und behandelt Blatt und Gruppe einheitlich;
  Iterator liefert die **Traversierung** darueber. Beide zusammen sind der Normalfall: das
  Kompositum haelt den Baum, der Iterator reicht ihn als flache Folge heraus. Wer die
  Typpruefung im Aufrufer loswerden will, braucht Composite — ein Iterator allein reicht dafuer
  nicht.
- **Visitor** legt fest, **welche Operation** auf jedem Knoten passiert; Iterator legt fest, in
  **welcher Reihenfolge** die Knoten kommen. Sobald du dir bei einer Aufgabe unsicher bist:
  Aendert sich die Frage (Summe, Ausgabe, Validierung), ist es Visitor. Aendert sich der Weg
  durch die Struktur, ist es Iterator.
- **`IEnumerable<T>` gegen `IAsyncEnumerable<T>`:** Der synchrone Iterator blockiert bei jedem
  `MoveNext`. Sobald die Elemente aus einer Datenbank, einem HTTP-Stream oder einer Queue kommen,
  brauchst du `IAsyncEnumerable<T>` mit `await foreach` und `yield return` in einer
  `async`-Methode — und mit `CancellationToken`, den die synchrone Variante nicht kennt. Das ist
  Thema der Katas 08_01 und 08_02; hier bleibt es bewusst synchron, damit der Unterschied im
  Zustandsautomaten sichtbar bleibt und nicht in der Asynchronitaet untergeht.
- **Generator gegen Sammlung:** Ein Iterator muss keine Sammlung hinter sich haben (siehe
  Aufgabe 7). Umgekehrt ist nicht jede Sammlung ein guter Iterator-Kandidat — eine
  `IReadOnlyList<T>` mit Index ist fuer Zufallszugriff die ehrlichere Schnittstelle.

## Wann nicht

- **In C# fast immer**: schreib `yield return`, keinen handgeschriebenen `IEnumerator<T>`. Ein
  eigener Enumerator lohnt praktisch nur fuer `struct`-Enumeratoren in heissen Pfaden (kein
  Boxing, keine Allokation je `foreach`) — und dann mit einer Messung als Begruendung, nicht mit
  einem Gefuehl. In dieser Kata baust du ihn genau einmal, um ihn danach nie wieder zu brauchen.
- **LINQ deckt die meisten Faelle ab**: `Where`, `Select`, `SelectMany`, `Take`, `Skip` sind
  fertige, verzoegert auswertende Iteratoren. Eine eigene Traversierung schreibst du nur, wenn
  die Struktur selbst neu ist (Baum, Graph, Ringpuffer) — nicht, um Filtern und Projizieren
  nachzubauen.
- Die Sammlung ist eine **flache Liste**, die der Aufrufer sowieso ganz braucht, und es gibt nur
  eine sinnvolle Reihenfolge. Dann ist `IReadOnlyList<T>` als Rueckgabetyp klarer als ein
  eigener Iterator: die Anzahl ist bekannt, der Zugriff direkt, und niemand muss ueber Mehrfach-
  Enumeration nachdenken.

## Skills

Verhaltensmuster erkennen, `IEnumerator<T>` von Hand implementieren, `yield return` und die
generierte Zustandsmaschine, mehrere benannte Traversierungen statt Flag-Parameter, `foreach` und
Mehrfach-Enumeration, Interna kapseln, definiertes Verhalten bei Aenderung waehrend der
Iteration, verzoegerte Auswertung und unendliche Folgen, Abgrenzung zu Composite, Visitor und
`IAsyncEnumerable<T>`

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
