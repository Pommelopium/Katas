# Kata 14_17 — Memento (Memento)

**Verhaltensmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/memento)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und **richtig anwenden** — nicht: es ueberall anwenden. Memento
beantwortet genau eine Frage: ein Objekt soll sich auf einen frueheren Zustand zuruecksetzen
lassen, **ohne** dass jemand von aussen seine Interna kennt. Der Kern ist die **Kapselung**,
nicht der Stapel: der Zustand wird gesichert, ohne die Felder nach aussen zu geben. Wer statt
dessen Setter oeffnet, "damit man zurueckschreiben kann", hat das Objekt geoeffnet und den
Verlauf nur obendrauf gelegt. Am Ende dieser Kata sollst du im eigenen Code die Stelle benennen
koennen, an der ein Feld nur wegen des Speicherns oeffentlich ist — und die Stelle, an der ein
`record` mit `with` schon reicht.

## Woran du das Muster erkennst

- **Zustand soll auf einen frueheren Punkt zurueckgesetzt werden:** Abbrechen, Undo, Rollback
  nach einem fehlgeschlagenen Schritt, "zurueck zum letzten Speicherpunkt".
- **Der Aufrufer liest alle Felder aus, um sie spaeter zurueckzuschreiben.** Irgendwo steht ein
  Block aus fuenf `var alt... = objekt.X;` und der passende Block aus fuenf Zuweisungen zurueck.
- **Felder werden nur fuer das Sichern oeffentlich gemacht** — `internal set`, ein zweiter
  Konstruktor "nur fuer Undo", ein `CopyFrom`. Genau dieser Bruch ist das Symptom, nicht der
  fehlende Verlauf.
- Ein **neues Feld bricht das Zuruecksetzen still**: es wird gesichert, wo es gebraucht wird,
  aber niemand denkt an die Kopierstelle. Der Fehler zeigt sich erst beim Undo.
- Der Verlauf liegt bei einer Klasse, die mit dem Inhalt **nichts zu tun hat** (UI, Editor,
  Kommando-Stapel) und ihn trotzdem im Detail kennen muss.

## Aufgabe: der Uebungsplan-Editor des Kata-Trackers

Im **Kata-Tracker** stellt sich der Nutzer seinen Uebungsplan zusammen: `PracticePlan` haelt
einen Titel, die geordnete Liste der geplanten Katas, die aktuelle Position im Plan
(`CurrentIndex`), eine freie Notiz und ein `IsLocked`, das den Plan nach dem Start einer Session
gegen Umsortieren sperrt. Der Editor bietet `Rueckgaengig` an — und macht es heute so:

```csharp
public sealed class PracticePlan
{
    // nur wegen des Sicherns oeffentlich geworden:
    public string Title { get; set; } = "";
    public List<string> Katas { get; set; } = new();
    public int CurrentIndex { get; set; }
    public string Note { get; set; } = "";
    public bool IsLocked { get; set; }

    public void Rename(string title) { /* ... */ }
    public void Insert(int position, string kataId) { /* ... */ }
    public void Advance() { /* ... */ }
}

public sealed class PlanEditor
{
    private string _savedTitle = "";
    private List<string> _savedKatas = new();
    private int _savedIndex;
    private bool _savedLocked;
    // _savedNote fehlt — und niemand merkt es, bis jemand eine Notiz tippt

    public void Backup(PracticePlan plan)
    {
        _savedTitle = plan.Title;
        _savedKatas = plan.Katas;        // dieselbe Liste, keine Kopie
        _savedIndex = plan.CurrentIndex;
        _savedLocked = plan.IsLocked;
    }

    public void Undo(PracticePlan plan)
    {
        plan.Title = _savedTitle;
        plan.Katas = _savedKatas;
        plan.CurrentIndex = _savedIndex;
        plan.IsLocked = _savedLocked;
    }
}
```

Zwei Fehler stecken darin, und beide sind typisch: die **Notiz** wird nie gesichert, und die
**Liste** wird als Referenz uebernommen, sodass `Insert` den angeblichen Sicherungsstand
mitveraendert. Dazu der eigentliche Schaden: fuenf Felder sind oeffentlich schreibbar, damit ein
fremdes Objekt sie zurueckschreiben kann. Genau dieses Bild ist das, was du als Memento erkennen
sollst.

## Aufgaben

1. Schreibe den Ausgangscode oben ab (oder nimm eine vergleichbare Klasse aus einem eigenen
   Projekt) und halte fest: wie viele Felder sind **nur** wegen des Sicherns oeffentlich, und
   welche fehlen in `Backup`. Das ist deine Messgroesse.
2. Fuehre den **Schnappschuss als undurchsichtigen Typ** ein: `IPlanSnapshot` ohne eine einzige
   Eigenschaft. Die Implementierung ist eine **verschachtelte** Klasse des `PracticePlan` und
   traegt die Daten privat — der Caretaker darf hineinsehen **nicht**, und das soll nicht
   Vereinbarung, sondern Sprachmittel sein (`private sealed class`, `private`-Felder, allenfalls
   ein `internal` mit begruendetem Grund).
3. Mach alle Setter des `PracticePlan` wieder **privat**. Der Zustand veraendert sich nur noch
   ueber die fachlichen Methoden (`Rename`, `Insert`, `Advance`, `WriteNote`, `Lock`) — sonst hat
   das Muster nichts gebracht.
4. Der **Originator** bekommt genau zwei Methoden: `IPlanSnapshot Save()` und
   `void Restore(IPlanSnapshot snapshot)`. Nur der Plan selbst legt seinen Zustand hinein und
   holt ihn heraus; wer welchen Schnappschuss aufbewahrt, ist ihm gleichgueltig.
5. Baue den **Caretaker** `PlanHistory` mit einem Verlauf: `Backup()`, `Undo()`, optional
   `Redo()`, dazu eine Obergrenze (z. B. 20 Schritte, aeltester faellt heraus). Er kennt nur
   `IPlanSnapshot` und den Plan — kein Titel, keine Liste, kein Index.
6. **Tiefe Objektgraphen:** die Kata-Liste (und, wenn du magst, pro Eintrag ein
   `PlanEntry`-Objekt mit Zielzeit) muss beim Sichern **tief kopiert** werden. Entscheide
   bewusst zwischen tiefer Kopie und unveraenderlichen Werttypen im Schnappschuss und schreib
   die Entscheidung in einem Satz auf.
7. **Fremde Schnappschuesse abweisen:** `Restore` mit einem Memento eines *anderen*
   `PracticePlan` oder einer fremden `IPlanSnapshot`-Implementierung wirft
   (`ArgumentException`/`InvalidOperationException`) statt halb wiederherzustellen. Gib jedem
   Schnappschuss dafuer die Herkunft mit.
8. **Kombination mit Command (Kata 14_14):** lass jedes Kommando vor dem Ausfuehren einen
   Schnappschuss nehmen und sein `Undo()` daraus wiederherstellen, statt eine Gegenaktion zu
   berechnen. Vergleiche in drei Saetzen: Gegenaktion ist sparsam und fehleranfaellig,
   Schnappschuss ist stumpf und immer korrekt.

## Beispiele und Testfaelle

Alle Faelle gegen den Plan mit Startzustand `Titel "Woche 34"`, Katas
`["07_02", "14_09"]`, `CurrentIndex 0`, Notiz `""`, `IsLocked false`.

| Fall | Erwartetes Ergebnis |
|---|---|
| `Backup()`, dann `Rename("Woche 35")`, dann `Undo()` | Titel wieder `"Woche 34"` |
| `Backup()`, dann `Insert(1, "14_17")` + `Advance()`, dann `Undo()` | Liste wieder genau `["07_02", "14_09"]`, `CurrentIndex` wieder `0` |
| `Backup()`, dann `WriteNote("erst Tests")`, dann `Undo()` | Notiz wieder `""` — **der Fall, den der naive Weg vergisst** |
| `Backup()`, alle fuenf Felder aendern, `Undo()` | **alle** fuenf sind wie vorher; der Test vergleicht Feld fuer Feld, nicht nur den Titel |
| drei Aenderungen, dreimal `Backup()`, zweimal `Undo()` | Zustand von nach der ersten Aenderung; ein drittes `Undo()` fuehrt auf den Startzustand und nicht darueber hinaus |
| `Backup()`, dann `Insert(...)`, dann Vergleich des Schnappschusses | der Schnappschuss ist **unveraendert** — kein geteiltes Listenobjekt |
| `Restore(fremderSchnappschuss)` (Memento eines zweiten Plans) | wird abgewiesen, der eigene Zustand bleibt vollstaendig unangetastet |
| leerer Verlauf, `Undo()` | kein Absturz und keine stille Aenderung: definiertes Verhalten (no-op oder Exception), getestet |

Dazu zwei Faelle, die keine Tabellenzeile sind:

- **Ein spaeter hinzugefuegtes Feld darf den Test rot machen.** Ergaenze am Ende der Kata ein
  sechstes Feld (`TargetMinutes`), aendere es im Testszenario und lass `Undo()` laufen, **ohne**
  den Schnappschuss anzupassen. Der Vergleichstest ueber den *gesamten* Zustand muss rot werden.
  Wird er gruen, prueft er zu wenig — dann ist der Vergleich (etwa ein Gleichheitsvergleich ueber
  alle Felder oder ein Approval-Text des Zustands) das eigentliche Ergebnis dieses Punkts.
- **Das Memento gibt seine Daten nach aussen nicht her.** Ein Testversuch, aus `IPlanSnapshot`
  den Titel oder die Liste zu lesen, darf **gar nicht kompilieren** — dieser Fall gehoert als
  auskommentierter Block mit einem Satz Begruendung in die Testklasse, nicht als Test. Wo die
  Grenze technisch loecherig bleibt (Reflection, Serialisierung, `internal` fuer die
  Testassembly), dokumentiere sie bewusst als Grenze statt sie zu verschweigen.

## Abgrenzung

- **Command** (Kata 14_14) kehrt eine **Aktion** um ("Zeile geloescht -> Zeile wieder
  einfuegen"), Memento sichert einen **Zustand** und rollt ihn zurueck. Faustregel: kennst du
  die exakte Gegenaktion und ist sie billig, nimm Command; ist der Zustand verwoben oder die
  Umkehrung nicht eindeutig, nimm den Schnappschuss. Beides zusammen ist der Normalfall in
  Editoren.
- **Prototype** (Kata 14_04) erzeugt eine **Kopie zur Weiterverwendung** — ein zweites, gleiches
  Objekt, mit dem weitergearbeitet wird. Ein Memento ist kein zweites Objekt, sondern ein
  Schnappschuss **zum Zurueckrollen**, den niemand ausser dem Original benutzen darf.
- **`record` mit `with`** loest den Fall schon auf Sprachebene, wenn der Zustand ohnehin
  unveraenderlich ist: die "alte Version" ist einfach die alte Instanz. Ebenso ist eine
  **Snapshot-Serialisierung** (JSON, `DbContext`-Change-Tracking, Event-Sourcing-Snapshot) eine
  legitime Variante — aber eine, die den Zustand nach aussen sichtbar macht und damit genau die
  Kapselung aufgibt, um die es hier geht.

## Wann nicht

- **Bei kleinen unveraenderlichen Typen genuegt `record` mit `with`.** Fuenf Zeilen Copy-Konstruktor
  gegen eine verschachtelte Memento-Klasse plus Schnittstelle plus Caretaker: das lohnt erst,
  wenn es wirklich privaten Zustand zu schuetzen gibt.
- **Grosse Schnappschuesse kosten Speicher.** Ein Verlauf von 100 Zustaenden eines Objekts mit
  Bildern oder langen Listen ist ein Leak mit Muster-Namen. Dann Obergrenze, inkrementelle
  Schnappschuesse oder Command mit Gegenaktion.
- **Serialisierung als Abkuerzung bricht die Kapselung wieder auf.** "Wir machen einfach
  `JsonSerializer.Serialize(plan)`" verlangt oeffentliche Setter oder Attribute an jedem privaten
  Feld — und schon ist der Zustand ueberall lesbar. Wenn der Weg trotzdem richtig ist, dann als
  bewusste Entscheidung, nicht als Nebenwirkung.

## Skills

Memento, Kapselung, verschachtelte Typen und Sichtbarkeit, tiefe gegen flache Kopie,
Undo/Redo mit Verlauf, Zusammenspiel mit Command, Tests ueber den vollstaendigen Zustand

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
