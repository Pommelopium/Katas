# Kata 09_04 — Domaenenmodell mit DDD-Bausteinen

**Stufe 3: API, Persistenz, Architektur** · Zeitrahmen: 1–2 Abende

## Ziel

Fachlogik gehoert in die Domaene, nicht in den Controller und nicht in einen Validator.

## Neuer Bounded Context: Trainingsplan

Regeln, die das **Modell** erzwingen muss — nicht die API-Schicht:

1. Ein Plan hat 1 bis 7 Slots pro Woche, jeder Slot genau eine Kata.
2. Eine Kata darf pro Woche hoechstens zweimal vorkommen.
3. Ein abgeschlossener Plan ist unveraenderlich.
4. Ein Slot kann nicht in der Vergangenheit liegen, wenn er neu angelegt wird.

Zum Vokabular: Ein Plan gehoert zu genau einer Kalenderwoche (`WeekNumber`), ein Slot sitzt
auf einer Position innerhalb dieser Woche (`SlotPosition`, 1 bis 7) und ist dort eindeutig.
"Abgeschlossen" ist ein Plan, nachdem `Complete()` erfolgreich war; danach lehnt jede
aendernde Methode ab. Ob die Gegenwart aus `DateTimeOffset.UtcNow` oder aus einer
injizierten Zeitquelle kommt, entscheidest du — Regel 4 muss testbar bleiben, ohne die
Systemuhr zu stellen.

## Umsetzung

- **Aggregate Root** `TrainingPlan` mit privatem `List<Slot>` und `IReadOnlyList<Slot>`
  nach aussen. Kein `ICollection`, kein Setter.
- Private Setter, **keine** oeffentlichen Konstruktoren — Erzeugung nur ueber
  `TrainingPlan.Create(...)`, das ein `Result<TrainingPlan>` liefert (Kata 07_02).
- **Value Objects** mit Wertgleichheit (`WeekNumber`, `SlotPosition`), als `record` oder
  `readonly record struct`.
- Invarianten werden in den Methoden des Aggregats geprueft. Ein Aggregat kann niemals in
  einem ungueltigen Zustand existieren.
- **Domain Events**: `PlanCompleted`, `SlotAdded`. Werden im Aggregat gesammelt und erst
  **nach** `SaveChanges` dispatcht.
- Anbindung an EF Core (Kata 09_02) ohne das Modell zu verbiegen: Backing Fields
  (`UsePropertyAccessMode.Field`).

## Beispiele und Testfaelle

- **Leerer Plan wird abgewiesen:** `TrainingPlan.Create(week, [])` liefert ein
  fehlgeschlagenes `Result` (Regel 1) — und kein Objekt, das man hinterher pruefen muesste.
  Genauso bei 8 Slots.
- **Kata dreimal in einer Woche:** Ein Plan mit den Katas `[Bowling, Bowling, Bowling]`
  scheitert bereits in `Create`; `[Bowling, Bowling, Taxi]` ist gueltig (Regel 2). Derselbe
  Test noch einmal ueber `AddSlot`: zwei Mal `Bowling` geht, das dritte Mal liefert einen
  Fehler und die Slot-Liste ist danach **unveraendert lang**.
- **Slot in der Vergangenheit:** `AddSlot` mit einer Position, die vor "jetzt" liegt,
  schlaegt fehl (Regel 4). Ein Slot in der laufenden oder einer kuenftigen Woche geht durch.
- **Abgeschlossener Plan ist dicht:** Nach `Complete()` liefern `AddSlot`, `RemoveSlot` und
  ein zweites `Complete()` jeweils einen Fehler, und `Slots` hat unveraendert denselben
  Inhalt (Regel 3).
- **Wertgleichheit der Value Objects:** `new WeekNumber(2026, 34) == new WeekNumber(2026, 34)`
  ist `true`, ebenso `Equals` und identische `GetHashCode`. Zwei Slots derselben Position
  in verschiedenen Wochen sind **nicht** gleich. `new WeekNumber(2026, 54)` ist ungueltig.
- **Domain Events sammeln, nicht feuern:** Ein erfolgreiches `AddSlot` legt genau **ein**
  `SlotAdded` in die Event-Liste des Aggregats; ein fehlgeschlagenes `AddSlot` legt
  **keines** dorthin. Nach `Complete()` steht dort zusaetzlich genau ein `PlanCompleted`.
  Der Test prueft die Liste am Aggregat — zu diesem Zeitpunkt darf noch kein Handler
  gelaufen sein.
- **Dispatch erst nach dem Speichern:** Im Test mit In-Memory- oder SQLite-Kontext laeuft
  der Handler erst nach `SaveChanges`. Wirft `SaveChanges`, wurde kein Event dispatcht.
- **Vom Compiler verhindert:** `plan.Slots.Add(slot)` und `plan.Slots = ...` duerfen sich
  nicht uebersetzen lassen, ebenso `new TrainingPlan(...)` von aussen. Diese Faelle
  gehoeren nicht in eine Testmethode, sondern als auskommentierte Zeile mit Begruendung
  daneben — dass der Compiler meckert, ist hier das Testergebnis.

## Fertig, wenn

Die kompletten Domaenentests **ohne jedes Mocking und ohne Datenbank** laufen und trotzdem
alle vier Regeln abdecken. Wenn du fuer einen Domaenentest ein Mock brauchst, ist die
Abhaengigkeit an der falschen Stelle.

## Skills

DDD, Aggregates, Value Objects, Domain Events, Kapselung, EF-Core-Mapping ohne Modellbruch

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
