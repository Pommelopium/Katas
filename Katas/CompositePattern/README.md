# Kata 14_08 — Kompositum (Composite)

**Strukturmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/composite)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und dann **richtig anwenden** — nicht: es ueberall anwenden. Composite
loest genau ein Problem: einen Baum so behandeln, dass der Aufrufer nicht wissen muss, ob er
gerade ein einzelnes Element oder eine ganze Gruppe in der Hand haelt. Wer das Muster verstanden
hat, erkennt auch die Faelle, in denen eine flache Liste die ehrlichere Antwort ist.

## Woran du das Muster erkennst

- Die Daten sind fachlich ein **Baum** (Ordner in Ordnern, Stueckliste, Menue, Organigramm,
  Ausdrucksbaum) — und der Code behandelt ihn als Sonderfall statt als Struktur.
- Beim Aufrufer steht `if (element ist Gruppe) { ... } else { ... }` — dieselbe Verzweigung
  taucht in jeder Auswertung wieder auf, weil jede Auswertung sie selbst treffen muss.
- Die **Rekursion ist von Hand** geschrieben, und zwar an mehreren Stellen: Summe, Anzahl,
  Ausgabe, Suche. Bei jedem neuen Auswertungswunsch wird sie kopiert.
- Blatt und Knoten werden unterschiedlich behandelt, obwohl der Aufrufer den Unterschied
  fachlich gar nicht braucht — er will nur eine Summe.
- Ein neuer Knotentyp zwingt dich, **alle** Aufrufer anzufassen: jedes `switch` und jede
  Typpruefung ist eine Stelle, die man vergessen kann.

## Aufgabe: der Lehrplanbaum im Kata-Tracker

Der **Kata-Tracker** zeigt den Lehrplan als Baum: eine **Stufe** enthaelt **Module**, ein Modul
enthaelt weitere Module oder einzelne **Katas**. Nur die Kata selbst hat eine Dauer in Minuten;
alles darueber ist Struktur. Die Oberflaeche braucht zu jedem Knoten die Gesamtdauer, die Anzahl
der enthaltenen Katas und eine eingerueckte Darstellung.

Heute rechnet der Aufrufer das aus:

```csharp
public sealed class CurriculumCalculator
{
    public int TotalMinutes(object node)
    {
        if (node is KataItem kata)
        {
            return kata.Minutes;
        }

        if (node is CurriculumFolder folder)
        {
            var sum = 0;
            foreach (var child in folder.Children)
            {
                // Handrekursion — dieselbe Schleife steht noch in CountKatas und in Render.
                sum += TotalMinutes(child);
            }

            return sum;
        }

        throw new ArgumentException($"Unbekannter Knotentyp: {node.GetType().Name}");
    }
}
```

Das ist der Zustand, den du erkennen sollst. Der Code laeuft, aber die Struktur des Baums lebt
im Aufrufer statt in den Knoten: `object` als Parametertyp, zwei Typpruefungen, ein `throw` fuer
alles Weitere — und mit jedem neuen Knotentyp und jeder neuen Auswertung wird es eine Stelle mehr.

## Aufgaben

1. Bau den Ausgangszustand nach — `KataItem`, `CurriculumFolder`, der `CurriculumCalculator` mit
   Typpruefung und Handrekursion in **drei** Methoden (`TotalMinutes`, `CountKatas`, `Render`) —
   und schreib die Tests fuer die Beispiele unten gegen diesen Stand. Sie bleiben dein Netz.
2. Fuehr die gemeinsame Schnittstelle ein: `ICurriculumNode` mit `string Title`,
   `int TotalMinutes()`, `int CountKatas()` und `string Render(int indent)`. Blatt und Gruppe
   implementieren sie beide — ab hier kennt der Aufrufer nur noch diesen Typ.
3. Verschieb die Rekursion in das Kompositum: `CurriculumFolder.TotalMinutes()` summiert seine
   Kinder ueber die Schnittstelle. Das Blatt gibt seine eigene Zahl zurueck und hoert damit auf.
   Danach enthaelt der Aufrufer **kein** `is`, `as` oder `switch` mehr — das ist das Abnahmekriterium.
4. Entscheide, wo `Add`/`Remove` hingehoert, und **begruende es schriftlich** im Code:
   - **Transparenz:** beide Methoden stehen in `ICurriculumNode`. Der Aufrufer behandelt alles
     gleich, aber `KataItem.Add(...)` muss luegen — meist mit `NotSupportedException`.
   - **Sicherheit:** beide Methoden stehen nur auf `CurriculumFolder`. Der Compiler verhindert
     den Unsinn, aber wer strukturell arbeitet, braucht wieder eine Typpruefung.
   Waehl **Sicherheit** und schreib in zwei Saetzen dazu, was du damit aufgibst: Aufbau und
   Auswertung sind ab jetzt zwei verschiedene Sichten auf denselben Baum. Wer sich anders
   entscheidet, muss den Fall `Add` auf einem Blatt testen.
5. Ergaenze `IEnumerable<ICurriculumNode> Children` als **schreibgeschuetzte** Sicht (beim Blatt
   leer, nicht `null`). Damit funktioniert die Auswertung von aussen, ohne dass jemand die Liste
   von aussen veraendern kann.
6. Fuege einen **neuen Knotentyp** hinzu: `ExamBlock` — eine Gruppe, deren Gesamtdauer die Summe
   der Kinder **plus 15 Minuten Puffer** ist. Kein Aufrufer und keine bestehende Klasse darf sich
   dafuer aendern. Wenn du etwas anfassen musst, war Schritt 3 nicht fertig.
7. Sicher die Struktur ab: `Add` verweigert Selbsteinfuegung und Zyklen (ein Knoten darf keinen
   eigenen Vorfahren aufnehmen) und lehnt ein Kind ab, das schon einen Elternknoten hat.
8. Nimm die Tiefe in den Griff: leg ein Tiefenlimit fest oder ersetz die Rekursion durch eine
   Traversierung mit explizitem Stack. Miss vorher, ab welcher Tiefe die rekursive Variante
   umfaellt, und notier die Zahl — ohne Messung ist "tief genug" nur eine Behauptung.

## Beispiele und Testfaelle

Referenzbaum: `Stufe 1` enthaelt `Grundlagen` (Katas `FizzBuzz` 30 min, `Bowling` 90 min) und
`Testbarkeit` (Kata `LegacyRescue` 120 min sowie den leeren Ordner `Reserve`).

| Fall | Erwartetes Ergebnis |
|---|---|
| `new CurriculumFolder("Reserve").TotalMinutes()` | `0`, `CountKatas() == 0`, keine Exception, `Children` ist leer und nicht `null` |
| `new KataItem("FizzBuzz", 30)` als `ICurriculumNode` | `TotalMinutes() == 30`, `CountKatas() == 1`, `Children` leer — das Blatt ist ein vollwertiger Knoten |
| `stufe1.TotalMinutes()` | `240` (30 + 90 + 120); der leere Ordner traegt `0` bei, ohne den Aufruf zu stoeren |
| `stufe1.CountKatas()` und `grundlagen.TotalMinutes()` | `3` bzw. `120` — jeder Teilbaum ist fuer sich dieselbe Frage |
| `ExamBlock("Abschluss")` mit Kata `BankOCR` 60 min, in `Stufe 1` gehaengt | Block liefert `75` (60 + 15 Puffer), `stufe1.TotalMinutes() == 315`; kein Aufrufer wurde geaendert |
| 1000 ineinander geschachtelte Ordner mit einem Blatt (5 min) | `TotalMinutes() == 5`. Die rekursive Variante muss die Tiefe entweder tragen oder mit `DepthLimitExceeded` bei einem **definierten** Limit abbrechen — ein `StackOverflowException` ist ein Fehlschlag, weil er nicht abfangbar ist |
| `folder.Add(folder)` | `InvalidOperationException`; ebenso `a.Add(b); b.Add(a)` — der Zyklus wird beim zweiten `Add` erkannt, nicht erst beim Rechnen |
| Dasselbe `KataItem` in zwei Ordnern | `Add` lehnt das zweite Einhaengen ab (Elternknoten schon gesetzt); sonst wird `FizzBuzz` doppelt gezaehlt und `CountKatas()` liefert `4` statt `3` |
| `stufe1.Render(0)` | Baum in Einrueckungsstufen von zwei Leerzeichen, Reihenfolge des Einfuegens; Zeilenzahl = Anzahl aller Knoten (hier `7`) |

## Abgrenzung

- **Decorator** hat denselben Aufbau — eigener Typ hinter derselben Schnittstelle — aber **genau
  ein** Kind, und er will Verhalten ergaenzen. Composite hat **viele** Kinder und will Struktur
  abbilden. Wenn deine Gruppe nie mehr als ein Kind hat, baust du einen Decorator.
- **Iterator** loest das **Durchlaufen** eines Baums, Composite das **Aufbauen und einheitliche
  Behandeln**. Beide zusammen sind ueblich: das Kompositum liefert die Struktur, ein Iterator
  reicht sie als flache Folge heraus. Ein Iterator allein macht die Typpruefung nicht weg.
- **Visitor** haengt **Operationen** an eine fertige Baumstruktur, ohne die Knoten zu aendern.
  Composite entscheidet, wie der Baum aussieht. Sobald du drei Auswertungen mehr brauchst und die
  Knotentypen stabil sind, kommt Visitor **auf** das Composite — nicht statt seiner.
- **Chain of Responsibility** reicht eine Anfrage eine Kette entlang, bis jemand zustaendig ist.
  Composite fragt **alle** Kinder und verrechnet die Ergebnisse.

## Wann nicht

- Die Struktur ist **flach**: eine Liste von Katas ohne Verschachtelung. Dann ist `Sum(k =>
  k.Minutes)` die Loesung, und eine Knotenhierarchie ist Ueberbau, der nur Lesezeit kostet.
- Blatt und Knoten koennen fachlich **wirklich Verschiedenes**: hat nur die Gruppe eine
  Reihenfolge, eine Freigabe oder Kinder, erzwingt die gemeinsame Schnittstelle Luegen —
  `NotSupportedException` in `Add`, `throw` in `Reorder`, `null` als Rueckgabe. Ein Interface,
  dessen Haelfte immer wirft, ist kein Muster, sondern eine verschobene Typpruefung.
- Der Baum ist eigentlich ein **Graph** (geteilte Kinder, mehrere Elternknoten, Zyklen). Dann
  gehoeren Besuchsmarkierungen und Kantenlogik dazu, und die naive Summe zaehlt doppelt.

## Skills

Strukturmuster erkennen, rekursive Datenstrukturen, einheitliche Schnittstelle fuer Blatt und
Knoten, Transparenz gegen Typsicherheit abwaegen, Erweiterung ohne Aenderung am Aufrufer,
Zyklen- und Tiefenpruefung, Abgrenzung zu Decorator, Iterator und Visitor

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
