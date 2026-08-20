# Kata 14_12 — Stellvertreter (Proxy)

**Strukturmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/proxy)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und **richtig anwenden** — nicht: es ueberall anwenden. Proxy ist
geuebt, wenn du im fremden Code die Stelle findest, an der ein teures oder geschuetztes Objekt
unkontrolliert benutzt wird, und wenn du danach Ladezeitpunkt und Zugriffsrecht **hinter
derselben Schnittstelle** regelst — ohne dass ein einziger Aufrufer davon erfaehrt. Wer jede
Abhaengigkeit vorsorglich hinter einen Stellvertreter legt, hat die Kata nicht bestanden, auch
wenn das Klassendiagramm hinterher symmetrisch aussieht.

## Woran du das Muster erkennst

- Ein **teures Objekt wird immer erzeugt**, auch wenn es niemand braucht: Datei geladen, Index
  aufgebaut, Verbindung geoeffnet — schon im Konstruktor, obwohl die haeufigste Anfrage die
  Daten gar nicht anfasst.
- Die **Zugriffspruefung ist im Aufrufer verstreut**: dasselbe `if (rolle != ...)` an vier
  Stellen, an der fuenften vergessen. Wer eine neue Aufrufstelle schreibt, muss die Regel
  kennen und daran denken.
- Ein **entfernter Aufruf soll wie ein lokaler aussehen**: HTTP, gRPC oder Queue stehen mitten
  in der Fachlogik, mitsamt Serialisierung, Timeout und Retry.
- Die **Schnittstelle bleibt gleich, nur der Zugriff wird kontrolliert** — Ladezeitpunkt,
  Recht, Protokollierung, Zwischenspeicher. Wuerdest du die Methoden umbenennen oder ergaenzen
  wollen, ist es kein Proxy.
- **Dasselbe teure Ergebnis wird mehrfach beschafft**, weil niemand einen Ort hat, an dem sich
  ein Zwischenspeicher unterbringen liesse, ohne die Fachklasse anzufassen.
- Tests der Fachregel sind **langsam oder brauchen Infrastruktur**, obwohl die Regel selbst
  nichts damit zu tun hat.

## Aufgabe: Das Loesungsarchiv des Kata-Trackers

Der **Kata-Tracker** soll zu jedem Versuch das eingereichte Loesungsarchiv zeigen: ein ZIP mit
Quellcode, Testlauf und Diff, im Schnitt 30 MB, aus dem beim Laden ein Volltextindex gebaut
wird. Die Uebersichtsseite listet aber nur Datum, Kata und Dauer — sie braucht das Archiv
**nie**. Nur wer auf "Loesung ansehen" klickt, braucht es, und dann auch nur, wenn er darf:
eigene Loesungen immer, fremde nur als Coach. Der heutige Stand sieht so aus:

```csharp
public sealed class SolutionArchive
{
    private readonly byte[] _zipBytes;
    private readonly Dictionary<string, string> _fullTextIndex;

    public SolutionArchive(string attemptId)
    {
        // teuer: 30 MB von der Platte, danach Index bauen — passiert immer, auch fuer die Liste
        _zipBytes = File.ReadAllBytes($"/archive/{attemptId}.zip");
        _fullTextIndex = BuildIndex(_zipBytes);
        LoadCount++;
    }

    public static int LoadCount;

    public string ReadFile(string path) => _fullTextIndex[path];
    public int SizeInBytes => _zipBytes.Length;
}

public sealed class AttemptRow
{
    public AttemptRow(Attempt attempt)
    {
        Attempt = attempt;
        Archive = new SolutionArchive(attempt.Id); // die Liste laedt 200 Archive und zeigt keins
    }

    public Attempt Attempt { get; }
    public SolutionArchive Archive { get; }
}

public static class SolutionController
{
    public static string Show(AttemptRow row, User user)
    {
        // Vorkommen 1 von 4: die Rechteregel steht beim Aufrufer
        if (row.Attempt.OwnerId != user.Id && user.Role != "Coach")
        {
            throw new UnauthorizedAccessException();
        }

        return row.Archive.ReadFile("Program.cs");
    }

    public static int Size(AttemptRow row, User user)
    {
        // Vorkommen 2 von 4 — hier ohne die Coach-Ausnahme, also strenger als gewollt
        if (row.Attempt.OwnerId != user.Id)
        {
            throw new UnauthorizedAccessException();
        }

        return row.Archive.SizeInBytes;
    }

    public static string Export(AttemptRow row, User user)
    {
        // Vorkommen 3 von 4 — und in ExportAll() ist die Pruefung ganz vergessen worden
        if (row.Attempt.OwnerId != user.Id && user.Role != "Coach")
        {
            throw new UnauthorizedAccessException();
        }

        return row.Archive.ReadFile("diff.patch");
    }
}
```

Der Schmerz ist bewusst klein und typisch: das Archiv wird **eifrig** im Konstruktor geladen,
also 200 Mal fuer eine Liste, die es nicht anzeigt. Die Berechtigungspruefung steht **beim
Aufrufer**, viermal, einmal falsch und einmal gar nicht. Und selbst wenn die Pruefung greift,
liegen die 30 MB schon im Speicher — abgelehnt wurde erst *nach* dem Laden. Genau dieser
Codeblock ist das, was du in fremdem Code erkennen sollst.

## Aufgaben

1. Schreib den Ausgangscode oben ab (`SolutionArchive` mit dem statischen `LoadCount` als
   Messpunkt, Laden per Fake statt echter Platte) und sichere das heutige Verhalten mit Tests
   ab, **inklusive** eines Tests, der die 200 unnoetigen Ladevorgaenge der Liste als Zahl
   festnagelt. Ohne diese Zahl gibt es hinterher keinen Beweis.
2. Zieh die gemeinsame Schnittstelle heraus: `ISolutionArchive` mit `string ReadFile(string
   path)` und `int SizeInBytes`. Original und alle Stellvertreter implementieren sie
   **unveraendert** — keine zusaetzliche Methode, kein `Initialize()`, kein `IsLoaded`. Sobald
   der Aufrufer merken kann, welche Variante er in der Hand haelt, ist das Muster verfehlt.
3. **Virtual Proxy (Lazy Loading):** `LazySolutionArchive : ISolutionArchive` erzeugt das
   echte `SolutionArchive` erst beim ersten Methodenaufruf und danach nie wieder. Der
   Stellvertreter **verwaltet die Lebensdauer** des Ziels selbst: er bekommt keine fertige
   Instanz, sondern die Information, wie sie zu erzeugen waere. `AttemptRow` haengt ab jetzt am
   Stellvertreter, die Liste laedt nichts mehr.
4. **Protection Proxy (Berechtigung):** `AuthorizingSolutionArchive : ISolutionArchive` traegt
   die Regel aus den vier Aufrufstellen **genau einmal**: eigene Loesung immer, fremde nur als
   Coach, sonst Abweisung. Loesch die vier Kopien, statt sie "vorerst" zu behalten, und leg
   fest, was Abweisung heisst (`UnauthorizedAccessException` oder ein `Failure`-Result aus Kata
   07_02 — eine Form, nicht beides).
5. **Die Reihenfolge ist die eigentliche Aufgabe:** stapel Protection *vor* Virtual, damit ein
   unberechtigter Zugriff abgewiesen wird, **ohne** das Ziel zu erzeugen. Dreh den Stapel
   probeweise um und zeig mit dem `LoadCount`, dass die falsche Reihenfolge 30 MB fuer einen
   abgelehnten Aufruf laedt. Halt in zwei Saetzen fest, warum das nicht Kosmetik ist.
6. **Logging- oder Caching-Proxy** als dritte Art: entweder protokolliert er jeden Zugriff mit
   Aufrufer, Pfad und Dauer, oder er merkt sich Ergebnisse von `ReadFile` pro Pfad. Beim
   Caching-Proxy gehoert die Frage dazu, wann der Eintrag ungueltig wird — eine Antwort, nicht
   ein TODO.
7. Setz den vollstaendigen Stapel im DI-Container zusammen, sodass der Aufrufer nur
   `ISolutionArchive` bekommt und die Registrierung der einzige Ort ist, an dem die drei
   Stellvertreter ueberhaupt namentlich vorkommen.
8. **Wie .NET das schon tut** — bau je ein kleines Gegenbeispiel und halt in je zwei Saetzen
   fest, was es dir abnimmt und was nicht:
   - `Lazy<T>` bzw. `Lazy<T>(LazyThreadSafetyMode.ExecutionAndPublication)` als eingebauter
     Virtual Proxy. Ersetz die Handarbeit aus Aufgabe 3 dadurch und behalt den Test.
   - Ein **gRPC-Client** (oder ein `HttpClient`-Typed-Client): der generierte Stub ist ein
     Remote Proxy — dieselbe Signatur, entfernter Aufruf. Zeig, wo Timeout und Fehlerform
     hingehoeren.
   - **EF-Core-Lazy-Loading-Proxies** (`UseLazyLoadingProxies`, `virtual`-Navigationen): das
     Framework erzeugt den Stellvertreter zur Laufzeit. Provozier bewusst ein
     N+1-Query-Problem und erklaer, warum genau dieser Komfort es verursacht.

## Beispiele und Testfaelle

Versuche `A1` (Eigentuemer `lasse`) und `A2` (Eigentuemer `mara`); Benutzer `lasse` (Rolle
`Trainee`), `mara` (`Trainee`) und `chris` (`Coach`). `SolutionArchive.LoadCount` wird vor jedem
Fall auf 0 gesetzt.

| Eingabe | Erwartetes Ergebnis |
|---|---|
| Stellvertreter fuer `A1` nur anlegen, keine Methode aufrufen | `LoadCount == 0` — das teure Objekt wird **nicht** erzeugt |
| danach `ReadFile("Program.cs")` als `lasse` | Inhalt kommt, `LoadCount == 1` — genau einmal beim **ersten** echten Zugriff |
| danach `SizeInBytes` und noch zweimal `ReadFile` | `LoadCount` bleibt `1` — der Stellvertreter erzeugt kein zweites Ziel |
| Uebersichtsliste mit 200 Versuchen aufbauen | `LoadCount == 0`; vor dem Umbau waren es 200 — derselbe Test, zwei Zahlen |
| `mara` greift auf `A1` zu (fremd, kein Coach) | Abweisung **und** `LoadCount == 0` — das Zielobjekt wird nicht erzeugt |
| `chris` (Coach) greift auf `A1` zu | erlaubt, `LoadCount == 1` |
| `lasse` fragt `SizeInBytes` von `A1` | erlaubt — der Fall, den Kopie 2 des Ausgangscodes falsch abgewiesen hat |
| Stapel absichtlich umgedreht (Virtual vor Protection), `mara` auf `A1` | Abweisung, aber `LoadCount == 1` — der Test macht die falsche Reihenfolge sichtbar und muss **rot** sein |
| Dieselbe parametrisierte Suite gegen `SolutionArchive` und den vollen Stapel, jeweils als `ISolutionArchive`, mit berechtigtem Aufrufer | beide identisch gruen; die Suite kennt **keinen** der Proxy-Typen |
| Zwei Threads rufen gleichzeitig zum ersten Mal `ReadFile` auf | `LoadCount == 1`, kein zweites Ziel, kein `null` |
| Caching-Proxy: `ReadFile("Program.cs")` zweimal | zweiter Aufruf erreicht das Ziel nicht (Zaehler am Ziel bleibt bei 1), Ergebnis identisch |
| Volltextsuche nach `new SolutionArchive` im Projekt | Treffer nur in der DI-Registrierung, im Virtual Proxy und in seinen Tests — nicht in Controller oder Domaene |

## Abgrenzung

- **Decorator:** **ergaenzt Verhalten** um denselben Vertrag (Retry, Metrik, Formatierung) und
  bekommt das umhuellte Objekt **fertig hereingegeben**. Der Proxy **kontrolliert den Zugriff**
  und verwaltet oft die **Lebensdauer selbst** — er entscheidet, *ob* und *wann* das Ziel
  ueberhaupt existiert. Frag: koennte ich die Klasse mit `null` als Ziel sinnvoll bauen? Beim
  Proxy ja (er erzeugt es), beim Decorator nein. Ein Logging-Proxy und ein Logging-Decorator
  sehen identisch aus — der Unterschied liegt in der Absicht, nicht im Code, und genau darum
  ist die Verwechslung der haeufigste Fehler dieser Kata.
- **Adapter:** **aendert die Schnittstelle**, weil zwei Vertraege nicht zusammenpassen. Der
  Proxy behaelt die Schnittstelle exakt — sobald du eine Signatur anfasst, ist es keiner mehr.
- **Facade:** vereinfacht ein **ganzes Subsystem** zu einem bequemen Einstieg und hat keine
  gemeinsame Schnittstelle mit dem, was sie verbirgt. Ein Proxy ist gegen dasselbe Interface
  austauschbar wie sein Ziel; eine Facade ist es nie.
- **Ein Repository ist kein Proxy.** Es buendelt Abfragen und hat einen eigenen Vertrag; ein
  Stellvertreter steht **eins zu eins** vor genau einem Objekt.

## Wann nicht

- **`Lazy<T>` genuegt.** Fuer reines verzoegertes Erzeugen ohne weitere Kontrolle ist die
  handgeschriebene Proxy-Klasse eine Datei zu viel — `Lazy<T>` ist thread-sicher, getestet und
  in einer Zeile da. Erst wenn *mehrere* Belange an derselben Naht haengen, lohnt der Stapel.
- **Autorisierung gehoert an die Grenze.** In ASP.NET Core erledigen `[Authorize]`, Policies
  oder eine Middleware die Rechtepruefung zentral und deklarativ, bevor die Domaene ueberhaupt
  aufgerufen wird. Ein Protection Proxy pro Objekt ist nur dann richtig, wenn das Recht am
  **einzelnen Datensatz** haengt und der Aufrufweg nicht durch die HTTP-Grenze fuehrt.
- **Das Objekt ist billig.** Ein Stellvertreter vor etwas, das in Mikrosekunden entsteht, kostet
  eine Indirektion, einen Stacktrace-Eintrag und einen Debug-Umweg — und spart nichts. Miss
  zuerst, bau dann.

## Skills

Proxy, Virtual Proxy und Lazy Loading, Protection Proxy, Remote Proxy, Caching- und
Logging-Proxy, Reihenfolge im Stellvertreter-Stapel, Lebensdauerverwaltung, `Lazy<T>`,
EF-Core-Lazy-Loading und N+1, Abgrenzung zu Decorator und Adapter

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
