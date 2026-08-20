# Kata 14_04 — Prototyp (Prototype)

**Erzeugungsmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/prototype)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und dann **richtig anwenden** — nicht: es ueberall anwenden. Prototype
loest genau ein Problem: ein Objekt zu vervielfaeltigen, ohne von aussen zu wissen, woraus es
besteht. Wer das Muster verstanden hat, erkennt auch die Faelle, in denen C# es schon mitbringt.

## Woran du das Muster erkennst

- Kopieren von aussen scheitert, weil ein Teil des Zustands **privat** ist — der Aufrufer kann
  nur die oeffentlichen Properties abschreiben und produziert stillschweigend halbe Kopien.
- Die Initialisierung ist **teuer** (Datenbank, Datei, Validierung, Berechnung), das Ergebnis
  aber immer gleich; eine Kopie des fertigen Objekts ist um Groessenordnungen billiger.
- Ein Objekt soll kopiert werden, **ohne seine konkrete Klasse zu kennen** — der Aufrufer haelt
  nur ein `IPlanElement` in der Hand und braucht davon ein zweites.
- Es existieren "Vorlagen" oder "Standardauspraegungen", die zur Laufzeit variiert werden, und
  irgendwo wachsen `switch`-Bloecke, die diese Vorlagen von Hand nachbauen.
- Der Kopiercode steht an mehreren Stellen und laeuft bei jedem neuen Feld auseinander — die
  Klasse aendert sich, die Kopierstellen wissen es nicht.

## Aufgabe: Trainingsplan-Vorlagen im Kata-Tracker

Der **Kata-Tracker** verwaltet Trainingsplaene. Ein `TrainingPlan` besteht aus Wochen, jede
Woche aus Uebungen; jede Uebung kennt ihren Plan zurueck (fuer die Fortschrittsanzeige). Der
Plan traegt ausserdem eine intern vergebene Pruefsumme und den geladenen Katalogstand — beides
privat, beides teuer zu ermitteln. Fuer jeden neuen Teilnehmer soll aus einer Vorlage
("Einsteiger 12 Wochen", "Zertifizierung") ein eigener, aenderbarer Plan entstehen.

Heute macht das der Aufrufer selbst:

```csharp
public sealed class PlanAssignmentService
{
    public TrainingPlan AssignTo(TrainingPlan template, string participant)
    {
        var copy = new TrainingPlan(template.Title, template.Level);
        foreach (var week in template.Weeks)
        {
            var weekCopy = new PlanWeek(week.Number);
            foreach (var exercise in week.Exercises)
            {
                // Die Uebung zeigt auf ihren alten Plan zurueck — niemand merkt es.
                weekCopy.Exercises.Add(exercise);
            }

            copy.Weeks.Add(weekCopy);
        }

        // _checksum und _catalogSnapshot sind privat: von hier nicht erreichbar.
        // Also bleiben sie leer und werden beim naechsten Speichern neu berechnet.
        copy.Participant = participant;
        return copy;
    }
}
```

Das ist der Zustand, den du erkennen sollst. Der Code kompiliert, laeuft und ist falsch: Die
Uebungen sind **geteilt** statt kopiert, die privaten Felder fehlen, und sobald es einen
`CertificationPlan : TrainingPlan` gibt, liefert die Methode stur einen `TrainingPlan` zurueck.

## Aufgaben

1. Bau den Ausgangszustand nach — Plan, Woche, Uebung, zwei private Felder, die Kopierlogik im
   `PlanAssignmentService` — und schreib einen Test, der die geteilte Uebung **beweist**.
2. Verschieb die Verantwortung ins Objekt: `Clone()` auf `TrainingPlan`, gestuetzt auf einen
   **protected Copy-Konstruktor** `protected TrainingPlan(TrainingPlan other)`. Nur die Klasse
   selbst kommt an ihre privaten Felder.
3. Fuehr `IPrototype<T>` mit `T Clone()` ein, damit der Aufrufer ueber die Abstraktion kopiert
   und die konkrete Klasse nicht mehr kennen muss.
4. Mach `Clone()` `virtual` und ueberschreib es in `CertificationPlan`, das ein zusaetzliches
   Feld mitbringt. Der Aufrufer aendert sich dabei nicht.
5. Entscheide pro Feld **flach gegen tief**: `Weeks` und `Exercises` werden tief kopiert (eigene
   Listen, eigene Elemente), der unveraenderliche Katalogstand wird geteilt. Schreib die
   Entscheidung als Kommentar an das Feld — das ist Teil der Loesung, nicht Beiwerk.
6. Sicher die Rueckwaertsreferenz `Exercise.Plan` ab: die Kopie muss auf die **Kopie** zeigen.
   Fuehr dafuer eine Kopier-Map (`Dictionary<object, object>`) durch den Kopiervorgang mit, die
   jedes bereits kopierte Objekt wiederverwendet.
7. Bau eine **PrototypeRegistry**: `Register(string key, IPrototype<TrainingPlan> prototype)` und
   `Create(string key)`. Sie gibt Kopien der Vorlagen aus und kennt keine der konkreten Klassen.
8. Optional: Miss die Erzeugung ueber die teure Initialisierung gegen `Create("einsteiger")` und
   notier den Faktor. Ohne Messung ist "Prototype ist schneller" nur eine Behauptung.

## Beispiele und Testfaelle

| Fall | Erwartetes Ergebnis |
|---|---|
| `plan.Clone()`, dann `copy.Weeks[0].Exercises.Add(...)` | `plan.Weeks[0].Exercises.Count` **unveraendert** — der entscheidende Test |
| `copy.Weeks[0].Exercises[0].Minutes = 90` | Original bleibt bei seinem Wert; die Uebung ist ein eigenes Objekt |
| `ReferenceEquals(plan.Weeks, copy.Weeks)` | `false`; ebenso fuer jedes Element darin |
| Privates Feld `_checksum` | in der Kopie identisch zum Original — nicht leer, nicht neu berechnet |
| `Exercise.Plan` in der Kopie | zeigt auf `copy`, nicht auf `plan`; die zyklische Referenz laeuft nicht in eine Endlosrekursion |
| `IPrototype<TrainingPlan> p = new CertificationPlan(...); p.Clone()` | Ergebnis ist ein `CertificationPlan` (`is` prueft es) samt kopiertem Zusatzfeld |
| Geteilter unveraenderlicher Katalogstand | `ReferenceEquals` ist bewusst `true` — dokumentiert und getestet, nicht zufaellig |
| `registry.Create("einsteiger")` zweimal | zwei verschiedene Instanzen, fachlich gleich; eine Aenderung an der einen laesst die andere und die Vorlage unberuehrt |

## Abgrenzung

- **Factory Method / Abstract Factory** erzeugen Objekte aus Parametern und Typentscheidungen;
  Prototype erzeugt sie aus einem **vorhandenen Objekt**. Sobald du keine Vorlageninstanz mehr
  brauchst, war es eine Factory.
- **Memento** kopiert Zustand ebenfalls, aber zum Zurueckspringen: der Schnappschuss ist opak
  und wird nie als zweites gleichwertiges Objekt benutzt. Prototype liefert ein vollwertiges,
  weiter benutzbares Objekt.
- **`record` mit `with`** und der handgeschriebene **Copy-Konstruktor** sind Kopiermechanismen,
  aber kein Prototype: sie brauchen den **statisch bekannten** Typ. Prototype ist erst dann
  Prototype, wenn der Aufrufer den konkreten Typ nicht kennt.

## Wann nicht

- Der Typ ist ein `record` (oder sonst unveraenderlich) und der Zielzustand statisch bekannt:
  `plan with { Participant = "Anna" }` reicht. Aber Achtung — `with` kopiert flach, geteilte
  Listen bleiben geteilt. Genau dieselbe Falle wie im Ausgangscode, nur kuerzer geschrieben.
- Das Objekt hat nur oeffentliche Wertfelder und keine Vererbung: `MemberwiseClone()` oder ein
  Copy-Konstruktor sind ehrlicher als eine Prototype-Hierarchie. `MemberwiseClone` ist flach und
  ignoriert deine Entscheidung aus Aufgabe 5 — als Prototype-Ersatz reicht es nur im flachen Fall.
- Ein **Serialisierungs-Roundtrip** (`JsonSerializer` hin und zurueck) als Universalkopie: er
  verliert alles, was nicht im Vertrag steht — private Felder ohne Attribut, den Laufzeittyp bei
  polymorphen Referenzen, Delegates und offene Ressourcen; zyklische Referenzen brechen ihn ganz.
  Wer trotzdem so kopiert, testet Fall 4, 5 und 6 von oben und sieht es sofort.

## Skills

Erzeugungsmuster erkennen, Copy-Konstruktor, tiefe gegen flache Kopie, Kovarianz beim Klonen,
zyklische Objektgraphen, Prototype Registry, Grenzen von `record`-`with` und Serialisierung

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
