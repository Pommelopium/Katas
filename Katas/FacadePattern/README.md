# Kata 14_10 — Fassade (Facade)

**Strukturmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/facade)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und dann **richtig anwenden** — nicht: es ueberall anwenden. Facade
loest genau ein Problem: ein Aufrufer soll einen fachlichen Anwendungsfall ausloesen, ohne die
Bauteile des Subsystems und deren Reihenfolge zu kennen. Wer das Muster verstanden hat, erkennt
auch die Faelle, in denen eine Fassade nur eine Schicht ohne Nutzen waere.

## Woran du das Muster erkennst

- Der Aufrufer kennt **sieben Klassen** und dazu deren Aufrufreihenfolge. Vertauscht man zwei
  Zeilen, bricht es zur Laufzeit — der Compiler sagt dazu nichts.
- Dieselbe Initialisierungssequenz steht **an mehreren Stellen kopiert** (Endpunkt, CLI, Test-
  Setup) und laeuft mit jedem neuen Subsystemschritt weiter auseinander.
- Der Aufrufer haengt an Details, **die ihn nichts angehen**: Verbindungsaufbau, Reihenfolge des
  Aufraeumens, Standardwerte, die immer gleich sind.
- `using`-Direktiven und Konstruktorparameter verraten den ganzen Innenraum des Subsystems: wer
  den Anwendungsfall aufrufen will, muss zuerst sieben Typen instanzieren.
- Eine Aenderung tief im Subsystem zieht Anpassungen quer durch alle Aufrufer nach sich, obwohl
  fachlich niemand etwas anderes will als vorher.

## Aufgabe: eine Uebungssitzung im Kata-Tracker starten

Im **Kata-Tracker** soll ein Teilnehmer eine Uebungssitzung starten: Kata auswaehlen, Arbeits-
kopie bereitstellen, Uhr laufen lassen. Fachlich ist das ein Satz. Technisch haengen daran
sieben Bauteile — Katalog, Teilnehmerprofil, Arbeitsverzeichnis, Repository-Klon, Testlauf-
Waermlauf, Zeitmessung und Telemetrie — die in genau einer Reihenfolge zusammenspielen und im
Fehlerfall in umgekehrter Reihenfolge wieder abgebaut werden muessen.

Heute macht das der Aufrufer selbst:

```csharp
public sealed class StartSessionEndpoint
{
    public SessionHandle Start(string participantId, string kataCode)
    {
        var catalog = new KataCatalog(_catalogPath);
        catalog.Load();                                  // muss vor Resolve passieren
        var kata = catalog.Resolve(kataCode);

        var profiles = new ParticipantProfileStore(_connectionString);
        var profile = profiles.Load(participantId);

        var workspace = new WorkspaceProvisioner(_rootPath);
        var folder = workspace.Create(profile.Id, kata.Code);

        RepositoryCloner? cloner = null;
        TestRunnerWarmup? warmup = null;
        SessionClock? clock = null;
        try
        {
            cloner = new RepositoryCloner(folder);
            cloner.Clone(kata.TemplateUrl);              // erst nach Create, sonst kein Zielpfad

            warmup = new TestRunnerWarmup(folder);
            warmup.Restore();                            // muss nach Clone laufen
            warmup.BuildOnce();

            clock = new SessionClock();
            clock.Start();                               // erst starten, wenn alles steht

            new TelemetrySink().SessionStarted(profile.Id, kata.Code, clock.StartedAt);
            return new SessionHandle(profile.Id, kata.Code, folder, clock);
        }
        catch
        {
            clock?.Abort();                              // Abbau genau in umgekehrter Reihenfolge
            warmup?.Discard();
            cloner?.Delete();
            workspace.Remove(folder);
            new TelemetrySink().SessionStartFailed(participantId, kataCode);
            throw;
        }
    }
}
```

Das ist der Zustand, den du erkennen sollst. Der Code ist nicht falsch — er ist an der falschen
Stelle. Dieselben 30 Zeilen stehen im CLI-Kommando und im Test-Setup noch zweimal, jedes Mal mit
einer kleinen Abweichung im `catch`.

## Aufgaben

1. Bau den Ausgangszustand nach: sieben Subsystemtypen mit ihren Reihenfolgezwaengen und den
   Aufrufer, der sie orchestriert. Schreib Tests, die die **Reihenfolge** und das **Aufraeumen
   im Fehlerfall** festnageln, bevor du etwas verschiebst.
2. Zieh eine `TrainingSessionFacade` ein und verschieb die Sequenz dorthin — unveraendert. Die
   Tests aus Schritt 1 bleiben gruen; das ist der Beweis, dass du nur verschoben hast.
3. Schneide die Fassade **fachlich**, nicht technisch: `StartSession`, `FinishSession`,
   `AbortSession`. Kein `GetCatalog()`, kein `GetClock()` — Methoden nach Anwendungsfaellen, nicht
   nach Subsystembauteilen. Eine Fassade mit einer Methode pro Subsystemklasse ist keine Fassade.
4. Nimm dem Aufrufer die Subsystemtypen weg: er bekommt nur noch die Fassade und ein
   `SessionHandle`. Die Rueckgabetypen der Fassade duerfen keine Subsystemtypen durchreichen,
   sonst wandert die Abhaengigkeit durch die Hintertuer zurueck.
5. Lass den **direkten Weg offen**: das Subsystem bleibt oeffentlich und benutzbar. Ein
   Sonderfall — etwa ein Wiederherstellen ohne Klon, weil das Verzeichnis schon existiert — geht
   an der Fassade vorbei direkt an `WorkspaceProvisioner`. Baue genau einen solchen Fall und
   notier, warum er kein Fassadenfall ist.
6. Beantworte schriftlich: **Wie verhinderst du, dass die Fassade zum God Object waechst?** Leg
   eine Grenze fest (etwa: nur Anwendungsfaelle eines Kontexts, keine Fachlogik, keine
   Entscheidungen, hoechstens N Methoden) und schreib sie als Kommentar ueber die Klasse. Fuege
   dann einen achten Anwendungsfall hinzu, der die Grenze verletzt, und zeig, wo er stattdessen
   hingehoert — in eine zweite Fassade.
7. Zerleg die Fassade dort, wo es sich lohnt: zwei kleine Fassaden (`SessionLifecycleFacade`,
   `ReportingFacade`) mit klaren Grenzen statt einer grossen. Der Aufrufer haengt nur an der, die
   er braucht.
8. Optional: Ersetz ein Subsystembauteil (anderer `RepositoryCloner`) und zeig, dass **kein**
   Aufrufer angefasst werden muss. Wenn doch, lag die Grenze der Fassade falsch.

## Beispiele und Testfaelle

| Fall | Erwartetes Ergebnis |
|---|---|
| `facade.StartSession("anna", "14_10")` | **ein** Aufruf liefert dasselbe `SessionHandle` wie die sieben Aufrufe vorher — Feld fuer Feld gleich |
| Fehler mitten in der Sequenz (`warmup.BuildOnce()` wirft) | nichts halb Erledigtes bleibt zurueck: Verzeichnis entfernt, Klon geloescht, Uhr nicht laufend, `SessionStartFailed` genau einmal |
| Aufrufer-Projekt | kompiliert **ohne Referenz** auf `KataCatalog`, `RepositoryCloner`, `SessionClock` und die uebrigen Subsystemtypen |
| Reihenfolge | ein Spion protokolliert die Aufrufe: `Load` vor `Resolve`, `Create` vor `Clone`, `Restore` vor `BuildOnce`, `clock.Start` als Letztes |
| Zweimal `StartSession` fuer denselben Teilnehmer | zweiter Aufruf scheitert fachlich (`session.already_running`), ohne das laufende Verzeichnis der ersten Sitzung anzutasten |
| Sonderfall am Subsystem vorbei (Aufgabe 5) | direkter `WorkspaceProvisioner`-Aufruf funktioniert weiter und ist getestet — die Fassade ist kein Zwangsweg |
| Subsystembauteil ausgetauscht (Aufgabe 8) | alle Aufrufer-Tests bleiben gruen, nur die Fassadentests kennen den Unterschied |
| Grenze der Fassade (Aufgabe 6) | der achte Anwendungsfall liegt in einer zweiten Klasse; ein Test haelt fest, dass die erste Fassade ihn **nicht** anbietet |

## Abgrenzung

- **Adapter** passt eine **Schnittstelle** an eine erwartete Form an — meist ein Objekt, kein
  Systemschnitt. Facade **versteckt Komplexitaet** hinter einer neu erfundenen, bequemen
  Schnittstelle. Kurz: Adapter hat die Zielschnittstelle vorgegeben, Facade erfindet sie.
- **Mediator** entkoppelt Kollegen, die sonst voneinander wissen muessten — die Kollegen
  **kennen den Vermittler** und reden nur mit ihm. Beim Facade **kennt das Subsystem die Fassade
  nicht**; der Verkehr laeuft nur in eine Richtung.
- **Proxy** hat dieselbe Schnittstelle wie sein Gegenstueck und tritt an seine Stelle (Lazy
  Loading, Zugriffsschutz, Caching). Facade hat eine **andere, kleinere** Schnittstelle und
  ersetzt kein einzelnes Objekt, sondern buendelt mehrere.
- **Application Service / Use-Case-Klasse** aus der Clean Architecture ist oft genau diese
  Fassade — mit dem Unterschied, dass sie zusaetzlich fachliche Entscheidungen treffen darf. Eine
  Fassade koordiniert nur; sobald Regeln darin stehen, benenn sie um.

## Wann nicht

- Eine Fassade, die **jede Methode 1:1 durchreicht**, ist nur eine zusaetzliche Schicht: sie
  kostet eine Datei, verbirgt nichts und muss bei jeder Subsystemaenderung mitgepflegt werden.
  Wenn du keine Reihenfolge, keinen Standardwert und keinen Aufraeumpfad einsparst, lass sie weg.
- Das Subsystem ist bereits **eine Klasse mit einem sinnvollen Einstiegspunkt**. Dann ist die
  Fassade ein Synonym — und Synonyme verwirren mehr als sie helfen.
- **Die Sammelklasse droht:** eine Fassade ohne festgelegte Grenze zieht jeden neuen
  Anwendungsfall an und wird zum God Object, an dem am Ende die ganze Anwendung haengt. Ohne die
  Antwort aus Aufgabe 6 solltest du keine Fassade einziehen.

## Skills

Strukturmuster erkennen, Subsysteme kapseln, fachlicher Anwendungsfall statt technischer
Delegation, Aufrufreihenfolge und Aufraeumen testen, Abhaengigkeiten schneiden, Grenzen gegen
God Objects, Abgrenzung zu Adapter, Mediator und Proxy

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
