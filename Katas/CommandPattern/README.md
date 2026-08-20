# Kata 14_14 — Befehl (Command)

**Verhaltensmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/command)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und **richtig anwenden** — nicht: es ueberall anwenden. Command ist
geuebt, wenn du eine Aktion so als Objekt fasst, dass sie rueckgaengig gemacht, wiederholt,
protokolliert und aufgeschoben werden kann, und wenn danach derselbe Befehl an Button,
Tastenkuerzel und Menue haengt, ohne dreimal dieselbe Logik zu enthalten. Wer jeden
Methodenaufruf vorsorglich in eine Befehlsklasse verpackt, hat die Kata nicht bestanden —
auch wenn das Klassendiagramm hinterher lehrbuchmaessig aussieht.

## Woran du das Muster erkennst

- Es steht die Anforderung im Raum, Aktionen **rueckgaengig zu machen** (Undo/Redo), und der
  heutige Code kann nur vorwaerts: er aendert Zustand, ohne zu wissen, wie er ihn zurueckdreht.
- Aktionen sollen **protokolliert, in eine Warteschlange gelegt oder wiederholt** werden — ein
  Auftrag soll spaeter, ausserhalb der Transaktion oder nach einem Neustart laufen. Ein
  Methodenaufruf laesst sich nicht persistieren, ein Befehlsobjekt schon.
- Die **Oberflaeche kennt die Fachlogik direkt**: im Event-Handler stehen Validierung,
  Berechnung und Speichern. Die Fachregel ist nur ueber die UI testbar.
- **Dieselbe Aktion haengt an Button, Tastenkuerzel und Menue** — und existiert dreimal, jedes
  Mal ein bisschen anders. Der dritte Aufrufer hat die Abfrage auf "darf ich das" vergessen.
- Es gibt Bedarf an **Audit oder Wiedervorlage**: "wer hat wann was geaendert" soll beantwortbar
  sein, und die Antwort steckt heute allenfalls in Logzeilen aus Fliesstext.
- Ein Ablauf soll aus mehreren Aktionen **als eine Einheit** ausgefuehrt werden (Makro, Skript,
  Batch), inklusive gemeinsamem Abbruch.

## Aufgabe: Der Trainingsplan-Editor des Kata-Trackers

Der **Kata-Tracker** bekommt einen Editor fuer den Trainingsplan: eine Liste geplanter Katas,
die man anlegen, umbenennen, verschieben und loeschen kann. Der Plan ist die Domaene
(`TrainingPlan` mit `IReadOnlyList<PlanItem>`), der Editor die Oberflaeche. Die fachliche
Anforderung, die alles treibt, lautet:

> **TRK-482:** Jede Aenderung am Trainingsplan muss rueckgaengig gemacht werden koennen,
> beliebig weit zurueck, und wiederherstellbar sein, solange der Anwender nichts Neues
> geaendert hat. "Woche einplanen" gilt dabei als **eine** Aenderung.

Der heutige Stand kennt kein Zurueck:

```csharp
public sealed partial class PlanEditorForm : Form
{
    private readonly TrainingPlan _plan;

    private void OnAddClick(object? sender, EventArgs e)
    {
        // Fachlogik direkt im Event-Handler — nichts merkt sich, was hier passiert ist
        var kataId = _kataIdBox.Text.Trim();
        if (kataId.Length == 0) return;
        _plan.Add(new PlanItem(kataId, int.Parse(_minutesBox.Text)));
        RefreshList();
    }

    private void OnRenameClick(object? sender, EventArgs e)
    {
        var item = _plan.Items[_list.SelectedIndex];
        _plan.Replace(_list.SelectedIndex, item with { KataId = _kataIdBox.Text.Trim() });
        RefreshList();
        // der alte Name ist ab hier weg — kein Weg zurueck
    }

    private void OnDeleteClick(object? sender, EventArgs e)
    {
        _plan.RemoveAt(_list.SelectedIndex);
        RefreshList();
    }

    private void OnMenuDeleteClick(object? sender, EventArgs e)
    {
        // dritte Kopie derselben Aktion: Menue, Button und Entf-Taste, dreimal fast gleich
        if (_list.SelectedIndex < 0) return;
        _plan.RemoveAt(_list.SelectedIndex);
        RefreshList();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete) _plan.RemoveAt(_list.SelectedIndex);
        RefreshList();
    }
}
```

Der Schmerz ist bewusst klein und typisch: die Aenderung passiert an Ort und Stelle, der alte
Zustand wird nirgends festgehalten, `Undo` waere nur mit einem kompletten Snapshot der Liste zu
bekommen — und dieselbe Loeschaktion steht dreimal da, einmal ohne Pruefung der Auswahl. Genau
diesen Codeblock sollst du in fremdem Code erkennen.

## Aufgaben

1. Schreib den Ausgangscode oben ab (der Editor darf eine Konsolen- oder Testattrappe sein, es
   geht nicht um WinForms) und sichere das heutige Verhalten mit Tests ab. Ohne gruenes Netz ist
   alles Folgende Umbau auf Verdacht.
2. Definiere den Vertrag: `ICommand` mit `void Execute()` und `void Undo()`. Jeder Befehl haelt
   **seine eigenen Parameter** und alles, was er zum Zurueckdrehen braucht (bei `RenameCommand`
   den alten Namen, bei `DeleteCommand` das entfernte Element **und** seine Position). Der
   Befehl kennt den Empfaenger (`TrainingPlan`), nicht die Oberflaeche.
3. Zieh die drei Aktionen als `AddItemCommand`, `RenameItemCommand`, `RemoveItemCommand`
   heraus. Danach enthaelt kein Event-Handler mehr Fachlogik, sondern nur noch das Erzeugen
   eines Befehls und die Uebergabe an den Invoker.
4. Bau den **Invoker** (`CommandBus` oder `History`) mit Verlauf: `Do(ICommand)`, `Undo()`,
   `Redo()`. Zwei Stacks. Ein `Do` nach einem `Undo` verwirft den Redo-Stack — festhalten,
   warum das die richtige Semantik ist und nicht eine Vereinfachung.
5. Haeng **dieselbe Befehlsinstanz-Erzeugung an drei Ausloeser** (Button, Tastenkuerzel, Menue).
   Die drei Kopien aus dem Ausgangscode werden geloescht, nicht "vorerst behalten".
6. Bau das **Makro**: `MacroCommand(IReadOnlyList<ICommand>)` ist selbst ein `ICommand`,
   fuehrt vorwaerts in Reihenfolge aus und macht **in umgekehrter Reihenfolge** rueckgaengig.
   "Woche einplanen" (fuenf `AddItemCommand`) liegt als **ein** Eintrag im Verlauf.
7. Mach einen Befehl **serialisierbar** und leg ihn in eine Warteschlange: `Add` und `Rename`
   als JSON (Typkennung plus Parameter), aus der Warteschlange gelesen, rekonstruiert und
   spaeter ausgefuehrt. Klaer dabei, was **nicht** serialisierbar ist (Objektreferenzen, Zeit,
   der Empfaenger) und wie der Befehl seinen Empfaenger beim Ausfuehren bekommt.
8. Beantworte die Frage, an der diese Kata haengt: **was passiert mit dem Verlauf, wenn
   `Execute` fehlschlaegt?** Der Befehl darf nicht im Undo-Stack landen, und der Zustand muss
   unveraendert sein — sonst macht ein spaeteres `Undo` etwas rueckgaengig, das nie passiert
   ist. Entscheide bewusst zwischen "wirft" und "liefert ein `Result`" (Kata 07_02) und
   zwischen "Befehl garantiert Atomaritaet selbst" und "Invoker raeumt auf".

## Beispiele und Testfaelle

Ausgangsplan `P0 = [("07_02", 95), ("14_06", 60)]`.

| Eingabe | Erwartetes Ergebnis |
|---|---|
| `Add("14_14", 120)`, `Rename(0, "07_03")`, `Remove(1)` — dann dreimal `Undo()` | Plan ist **exakt** `P0`: gleiche Elemente, gleiche Reihenfolge, gleiche Minuten. Verglichen wird der ganze Zustand, nicht nur die Anzahl |
| `Undo()` bei leerem Verlauf | keine Aenderung, keine Exception, Rueckgabe `false` (bzw. definierter "nichts zu tun"-Fall). Zweimal aufgerufen: dasselbe |
| `Add(...)`, `Undo()`, `Redo()` | Element ist wieder da, und zwar an **derselben Position** wie vor dem `Undo` |
| `Add(A)`, `Undo()`, `Add(B)`, dann `Redo()` | `Redo` ist **nicht** moeglich: `A` kommt nicht zurueck, der Plan enthaelt `B` genau einmal |
| `Remove(0)` und danach `Undo()` | das entfernte Element steht wieder **an Index 0**, nicht am Ende — der Beweis, dass der Befehl die Position mitfuehrt |
| Makro "Woche einplanen" mit fuenf `Add`, danach **ein** `Undo()` | alle fuenf sind weg, `Plan == P0`; der Verlauf hat genau **einen** Eintrag verbraucht. Ein Protokoll der Undo-Aufrufe zeigt die Reihenfolge 5, 4, 3, 2, 1 |
| `Rename(0, "")` — ein Befehl, dessen `Execute` fehlschlaegt | Plan unveraendert, Befehl liegt **nicht** im Verlauf. Gegenprobe: ein direkt folgendes `Undo()` macht den *vorherigen* Befehl rueckgaengig, nicht den gescheiterten |
| Makro, dessen dritter Befehl fehlschlaegt | die ersten zwei sind zurueckgedreht, `Plan == P0`, und das Makro steht nicht im Verlauf — Alles-oder-nichts |
| Serialisierter `Add("14_14", 120)` durch die Warteschlange, in einem neuen Invoker rekonstruiert und ausgefuehrt | derselbe Zustand wie beim direkten Aufruf, und der rekonstruierte Befehl ist danach ebenfalls per `Undo()` rueckgaengig machbar |
| Aktion einmal per Button, einmal per Tastenkuerzel, einmal per Menue ausgeloest | dreimal genau dasselbe Ergebnis und drei Eintraege im Verlauf; die Tests kennen **keinen** UI-Typ, nur `ICommand` und den Invoker |

## Abgrenzung

- **Strategy:** kapselt, **wie** etwas getan wird — austauschbare Algorithmen fuer denselben
  Zweck, in der Regel ohne eigenen Zustand und ohne Umkehrung. Command kapselt, **was** getan
  wird: eine konkrete Aktion samt Parametern und Empfaenger, aufschiebbar und umkehrbar. Frag:
  waehle ich zwischen Varianten (Strategy) oder speichere ich einen Auftrag (Command)?
- **Memento:** sichert den **Zustand** vor der Aenderung und stellt ihn wieder her; Command
  kehrt die **Aktion** um. Beides ist kombinierbar und in der Praxis die ueblichste Loesung:
  der Befehl legt fuer nicht invertierbare Aktionen ein Memento an, statt eine fehleranfaellige
  Rueckwaertsrechnung zu versuchen. Wer den ganzen Plan pro Schritt kopiert, hat allerdings
  nur Memento gebaut und Command uebersprungen.
- **Chain of Responsibility:** verteilt **eine** Anfrage auf eine Kette moeglicher Bearbeiter,
  bis einer zustaendig ist. Command hat genau einen bekannten Empfaenger. Die Kette kann
  Commands transportieren — das macht sie nicht zum Muster derselben Frage.
- **CQRS-Command (Kata [09_03](../Cqrs/README.md)):** dort ist ein Command eine reine
  Nachricht (DTO ohne Verhalten), die ein separater Handler ueber einen Dispatcher ausfuehrt,
  und Undo gibt es nicht — rueckgaengig macht dort die Transaktion. Beim GoF-Command tragen
  Aktion, Parameter und Umkehrung **in einem Objekt**. Derselbe Name, zwei verschiedene
  Muster: das eine trennt Schreiben von Lesen, das andere macht eine Aktion zum Objekt.

## Wann nicht

- **Kein Undo, keine Serialisierung, keine Warteschlange.** Dann reicht die Sprache: ein
  `Action` bzw. `Func<T>` als Delegate, ein Lambda oder eine Methodenreferenz ist genau ein
  Befehlsobjekt ohne Zeremonie. Erst wenn `Undo`, Persistenz oder Protokollierung dazukommen,
  braucht es den Typ — ein Delegate kann man nicht umkehren und nicht speichern.
- **Ein einziger Ausloeser, eine einzige Aktion.** Eine Befehlsklasse pro Klick, die nur eine
  Servicemethode weiterruft, ist keine Entkopplung, sondern eine Datei mehr. Der Nutzen
  entsteht mit dem zweiten Ausloeser oder der zweiten Anforderung an die Aktion.
- **Die Aktion ist nicht umkehrbar.** E-Mail versendet, Zahlung ausgefuehrt, Datei geloescht:
  hier taeuscht ein `Undo()` eine Faehigkeit vor, die es nicht gibt. Dann besser eine explizite
  Kompensation als eigener Befehl (Storno) — und im Verlauf sichtbar als solcher.

## Skills

Command, Undo/Redo mit zwei Stacks, MacroCommand, Serialisierung von Befehlen, Invoker und
Empfaenger trennen, UI ohne Fachlogik, Fehlerbehandlung im Verlauf, Abgrenzung der
Verhaltensmuster

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
