# Kata 14_19 — Zustand (State)

**Verhaltensmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/state)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und **richtig anwenden** — nicht: es ueberall anwenden. State ist
geuebt, wenn ein Objekt sein Verhalten aendert, weil es seinen Zustand wechselt, und wenn danach
kein `switch` ueber ein Status-Enum mehr im Kontext steht. Vor allem ist es geuebt, wenn der
**unmoegliche Uebergang gar nicht mehr formulierbar** ist statt nur "eigentlich nicht erlaubt".
Wer jedes Enum vorsorglich in eine Klassenhierarchie aufloest, hat die Kata nicht bestanden —
auch wenn das Klassendiagramm hinterher lehrbuchmaessig aussieht.

## Woran du das Muster erkennst

- Derselbe **`switch` ueber ein `Status`-Enum** steht in mehreren Methoden derselben Klasse,
  jedes Mal mit fast, aber nicht ganz denselben Faellen.
- **Jede neue Methode braucht wieder denselben `switch`** — und der Autor der fuenften Methode
  hat einen Fall vergessen, was niemandem auffiel, weil `default` schweigt.
- **Unmoegliche Uebergaenge sind nur durch Disziplin verhindert:** dass ein abgeschlossener
  Vorgang nicht wieder startet, steht in keinem Typ, sondern in einem `if` an drei Stellen — und
  an der vierten nicht.
- **Boolesche Flag-Kombinationen**, von denen die Haelfte unmoeglich ist: `IsStarted`,
  `IsPaused`, `IsFinished` erlauben acht Kombinationen, fachlich sinnvoll sind vier.
- Der Zustand wird **von aussen zugewiesen** (`session.Status = Status.Running;`), und die
  Frage "wer darf das eigentlich" hat im Code keine Antwort.

## Aufgabe: Die Uebungssitzung des Kata-Trackers

Der **Kata-Tracker** bekommt die Uebungssitzung: ein Zeitraum, in dem an genau einer Kata
gearbeitet wird. Eine Sitzung wird geplant, gestartet, darf pausiert und fortgesetzt werden und
endet abgeschlossen oder abgebrochen. Die gemessene Dauer zaehlt nur, solange sie laeuft — und
genau daran haengt die fachliche Anforderung:

> **TRK-517:** Nur abgeschlossene Sitzungen fliessen in die Statistik. Eine abgeschlossene
> Sitzung darf nicht erneut gestartet, nicht pausiert und nicht nachtraeglich abgebrochen
> werden. Jeder Versuch ist ein Fehler mit klarer Meldung, keine stille Nichtwirkung.

Der Lebenszyklus umfasst fuenf Zustaende: `Geplant`, `Laufend`, `Pausiert`, `Abgeschlossen`,
`Abgebrochen`. Der heutige Stand kennt sie als Enum:

```csharp
public enum SessionStatus { Geplant, Laufend, Pausiert, Abgeschlossen, Abgebrochen }

public sealed class PracticeSession
{
    public SessionStatus Status { get; set; } = SessionStatus.Geplant;
    public int ElapsedMinutes { get; private set; }

    public void Start()
    {
        switch (Status)                       // switch Nummer eins
        {
            case SessionStatus.Geplant:
                Status = SessionStatus.Laufend;
                break;
            case SessionStatus.Pausiert:
                Status = SessionStatus.Laufend;
                break;
            case SessionStatus.Laufend:
                break;                        // stille Nichtwirkung — Fehler oder Absicht?
            default:
                break;                        // Abgeschlossen und Abgebrochen: schweigt
        }
    }

    public string Describe()
    {
        switch (Status)                       // switch Nummer zwei, dieselben Faelle
        {
            case SessionStatus.Geplant:      return "geplant, noch nicht begonnen";
            case SessionStatus.Laufend:      return $"laeuft seit {ElapsedMinutes} min";
            case SessionStatus.Pausiert:     return $"pausiert bei {ElapsedMinutes} min";
            case SessionStatus.Abgeschlossen: return $"fertig in {ElapsedMinutes} min";
            default:                          return "unbekannt";   // Abgebrochen fehlt
        }
    }
}
```

Der Schmerz ist bewusst klein und typisch: zwei Methoden, zweimal derselbe `switch`, ein
vergessener Fall, ein `default`, der einen Fehler zu einer Beschreibung macht, und ein
oeffentliches `Status`-Setter, das jeden Uebergang erlaubt. Genau diesen Codeblock sollst du in
fremdem Code erkennen.

Erlaubte Uebergaenge — und nur diese:

| Von | `Start()` | `Pause()` | `Finish()` | `Cancel()` |
|---|---|---|---|---|
| `Geplant` | `Laufend` | — | — | `Abgebrochen` |
| `Laufend` | — | `Pausiert` | `Abgeschlossen` | `Abgebrochen` |
| `Pausiert` | `Laufend` | — | — | `Abgebrochen` |
| `Abgeschlossen` | — | — | — | — |
| `Abgebrochen` | — | — | — | — |

`Abgeschlossen` und `Abgebrochen` sind Endzustaende: aus ihnen fuehrt kein Uebergang heraus.

## Aufgaben

1. Schreib den Ausgangscode oben ab und sichere das heutige Verhalten mit Tests ab, inklusive
   der stillen Nichtwirkungen und des vergessenen `Abgebrochen`-Falls. Ohne gruenes Netz ist
   alles Folgende Umbau auf Verdacht.
2. Bring die Uebergangstabelle als **parametrisierten Test** an den Anfang, gegen den heutigen
   Code. Die Faelle, die jetzt rot sind, sind die Fehler, die das Muster beseitigt — nicht
   nachtraeglich behaupten, sondern vorher messen.
3. **Ein Typ pro Zustand:** `ISessionState` mit `Start`, `Pause`, `Finish`, `Cancel` und
   `Describe()`, dazu `PlannedState`, `RunningState`, `PausedState`, `FinishedState`,
   `CancelledState`. Nach diesem Schritt enthaelt `PracticeSession` **keinen `switch`** mehr,
   sondern delegiert an das aktuelle Zustandsobjekt.
4. **Uebergaenge als Rueckgabewert des Zustands:** jede Operation liefert den Folgezustand
   (`ISessionState Start()`), der Kontext ersetzt sein Feld damit. Niemand ausserhalb setzt den
   Zustand — der `Status`-Setter verschwindet. Der Zustand kennt seinen Nachfolger, der Klient
   nicht.
5. Der **unerlaubte Uebergang scheitert an genau einer Stelle**: eine gemeinsame Basisklasse
   bzw. ein Default-Verhalten wirft `InvalidStateTransitionException` (oder liefert ein
   `Result`, Kata [07_02](../ResultPattern/README.md)) mit Ausgangszustand und versuchter
   Operation in der Meldung. Es gibt genau einen `throw`, nicht zwoelf.
6. Entscheide, **was der Zustand ueber den Kontext wissen darf**. Nur `Laufend` darf Zeit
   addieren; wandert die Zeitmessung in den Zustand oder bleibt sie im Kontext und der Zustand
   gibt nur Auskunft? Halte die Entscheidung und ihren Grund schriftlich fest.
7. **Neuer Zustand `Abgelaufen`:** eine Sitzung, die laenger als 8 Stunden laeuft, wird beim
   naechsten Zugriff `Abgelaufen` und zaehlt nicht in die Statistik. Fuege ihn hinzu, ohne die
   Zustaende `Geplant`, `Pausiert`, `Abgeschlossen` und `Abgebrochen` anzufassen — genau **eine**
   bestehende Klasse (`RunningState`) und **eine** neue Datei. Vergleiche den Aufwand mit dem am
   Enum: dort waeren es alle `switch`-Bloecke.
8. Klaer die **Persistenz**: welcher Zustand steht in der Datenbank? Zustandsobjekte sind kein
   Datenbanktyp — persistiert wird eine stabile Kennung (String oder Enum), aus der beim Laden
   das Zustandsobjekt rekonstruiert wird. Bau die Abbildung in **eine** Fabrikmethode, und
   beantworte: was passiert beim Laden einer Kennung, die dein Code nicht kennt (alter Zustand
   aus einer aelteren Version), und darf ein Zustandsobjekt Instanzfelder haben, wenn es geteilt
   wird?

## Beispiele und Testfaelle

Ausgangssitzung `S0` = neue Sitzung fuer Kata `14_19`, Zustand `Geplant`, `ElapsedMinutes = 0`.

| Eingabe | Erwartetes Ergebnis |
|---|---|
| **Vollstaendige Uebergangstabelle als parametrisierter Test:** alle 5 Zustaende x 4 Operationen = 20 Faelle, jeder mit Erwartung *erlaubt und Zielzustand* oder *verboten* | genau die 7 erlaubten Uebergaenge aus der Tabelle gehen durch, die anderen 13 scheitern. Der Test ist **datengetrieben**: die Tabelle steht einmal da, nicht als 20 Testmethoden |
| Happy Path als Zustandsfolge: `Start()`, `Pause()`, `Start()`, `Finish()` | `Geplant -> Laufend -> Pausiert -> Laufend -> Abgeschlossen`. Geprueft wird die **Folge** der Zustaende, nicht nur der Endzustand |
| Abbruch mitten im Lauf: `Start()`, `Cancel()` | `Geplant -> Laufend -> Abgebrochen`; die Sitzung erscheint **nicht** in der Statistik der abgeschlossenen Sitzungen |
| `Finish()` auf `S0` (noch nicht gestartet) | definierter Fehler mit Ausgangszustand *und* Operation in der Meldung. Zustand danach **unveraendert** `Geplant`, `ElapsedMinutes` unveraendert — kein halber Uebergang |
| `Start()` auf einer abgeschlossenen Sitzung, zweimal aufgerufen | beide Male derselbe Fehler, kein stiller Durchlauf. Gegenprobe zum Ausgangscode: dort passiert nichts, und niemand erfaehrt es |
| `Pause()` auf `Laufend`, danach `Pause()` erneut | erster Aufruf: `Pausiert`. Zweiter Aufruf: Fehler — "schon pausiert" ist kein Erfolg |
| Zeitmessung: 30 min laufen, `Pause()`, 60 min warten, `Start()`, 15 min laufen, `Finish()` | `ElapsedMinutes == 45`. Die Pausenzeit zaehlt nicht — nur `Laufend` addiert |
| Neuer Zustand `Abgelaufen`: `Start()`, dann 8 h 1 min vergangen (Zeit ueber `TimeProvider` gesteuert) | Zustand `Abgelaufen`, nicht in der Statistik. **Nachweis der Offenheit:** der Diff dieser Aufgabe beruehrt nur `RunningState` und die neue Klasse; kein Test der anderen vier Zustaende wird angefasst |
| Persistenz-Rundlauf: Sitzung in jedem der sechs Zustaende speichern, neu laden, dann eine Operation ausfuehren | gespeichert wird eine Kennung; nach dem Laden verhaelt sich die Sitzung **identisch** zur nie gespeicherten. Unbekannte Kennung beim Laden -> definierter Fehler, kein stilles `Geplant` |
| Suche nach `switch` und `if` auf den Zustand im Kontext | `PracticeSession` enthaelt **keine** Verzweigung ueber den Zustand mehr, und `InvalidStateTransitionException` wird an genau **einer** Stelle geworfen (per Test oder Codereview belegt) |

## Abgrenzung

- **Strategy:** strukturell fast identisch — Komposition plus Delegation an ein austauschbares
  Objekt. Der Unterschied liegt beim Wechsel: bei Strategy **waehlt der Klient** die Variante und
  die Varianten kennen einander nicht; bei State **waehlt der Zustand selbst seinen Nachfolger**,
  die Zustaende kennen sich also und bilden zusammen einen Lebenszyklus. Frag: tausche ich einen
  Algorithmus aus (Strategy) oder gehe ich einen Schritt weiter (State)?
- **Bridge:** trennt eine Abstraktion von ihrer Implementierung, damit beide unabhaengig wachsen
  koennen — beide Seiten sind zur Laufzeit stabil. State tauscht das Objekt hinter der Referenz
  **dauernd** aus; genau das ist der Zweck. Gleiche Zeichnung, anderer Grund.
- **Command (Kata [14_14](../CommandPattern/README.md)):** macht die **Aktion** zum Objekt,
  aufschiebbar und umkehrbar. State macht die **Situation** zum Objekt, in der eine Aktion
  erlaubt oder verboten ist. Beides kombiniert sich: ein Command fragt den Zustand, ob es
  ausgefuehrt werden darf.
- **Ausgewachsene State Machine bzw. Workflow-Engine** (Stateless, Elsa, Temporal, BPMN): dort
  ist die Uebergangstabelle **Daten** — konfigurierbar, visualisierbar, mit Guards, Hooks,
  Nebenlaeufigkeit, Persistenz und Wiederaufnahme. State ist ein Muster mit fuenf Klassen und
  ohne Laufzeit. Die Grenze ist praktisch: sobald Fachbereich oder Betrieb den Ablauf **ohne
  Deployment** aendern oder anschauen wollen, oder sobald Uebergaenge Tage dauern und Neustarts
  ueberleben muessen, ist die Engine richtig und das Muster zu klein.

## Wann nicht

- **Zwei Zustaende und ein `switch`.** `Offen`/`Geschlossen` mit einer verhaltensabhaengigen
  Methode braucht kein Interface und keine zwei Klassen — das ist Ueberbau, der die Fachregel
  ueber fuenf Dateien verteilt. Der Nutzen entsteht mit dem dritten Zustand oder der dritten
  zustandsabhaengigen Methode.
- **Die C#-Alternative zuerst pruefen:** ein `switch`-Ausdruck ueber eine **abgeschlossene
  Typhierarchie** (`abstract record` mit `sealed record`-Faellen, also eine Union) gibt dir
  Vollstaendigkeitspruefung, Pattern Matching und Zielzustaende als Rueckgabewert ohne
  Klassenhierarchie mit Verhalten. Eine reine Uebergangsfunktion
  `static State Next(State current, Trigger t) => (current, t) switch { ... }` ist an einer
  Stelle lesbar und testbar — solange das Verhalten *pro Zustand* klein bleibt. Erst wenn jeder
  Zustand eigene Daten und mehrere eigene Methoden hat, gewinnt State.
- **Der Zustand aendert nur die Anzeige, nicht das Verhalten.** Wenn sich je Zustand allein ein
  Text, ein Icon oder eine Farbe unterscheidet, reicht eine Abbildung (Dictionary, Attribut,
  Ressource). Muster loesen Verhaltensprobleme, keine Beschriftungsprobleme.

## Skills

State, Zustandsuebergaenge als Rueckgabewert, Enum-`switch` aufloesen, unmoegliche Zustaende
untypisierbar machen, parametrisierte Uebergangstests, Open-Closed beim Hinzufuegen von
Zustaenden, Persistenz von Zustaenden, Abgrenzung zu Strategy und zur Workflow-Engine

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
