# Kata 07_05 — Architektur- und Testqualitaet automatisch pruefen

**Stufe 1: Modernes C# und Testbarkeit** · Zeitrahmen: 1 Abend

## Ziel

Zwei Dinge, die man behauptet, ohne sie zu belegen: "die Architektur ist eingehalten" und
"wir haben Tests". Diese Kata macht aus beiden Behauptungen einen roten Build.

## Aufgabe: der Kata-Tracker als Pruefling

Geuebt wird an der Kata-Tracker-Loesung: `KataTracker.Domain` (Kata, Attempt, Streak-Regeln),
`KataTracker.Application` (Handler, Validatoren), `KataTracker.Infrastructure` (EF Core,
Zeit, Dateisystem) und `KataTracker.Web` (Endpunkte). Die gewollte Abhaengigkeitsrichtung
ist `Web -> Application -> Domain`, `Infrastructure` haengt an `Application` und `Domain` und
wird nur in `Web` verdrahtet. Deine Aufgabe: diese Saetze aufhoeren zu behaupten und sie als
Tests schreiben — und danach mit Stryker.NET zeigen, wie viel die vorhandenen Tests zur
Streak-Berechnung wirklich absichern.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Du brauchst eine Loesung mit mindestens zwei
Projekten und ein paar Tests — jede halbwegs gewachsene Kata reicht.
**Empfohlen, nicht erforderlich:** Kata 09_03/09_04 (dort gibt es Schichten, die sich verletzen
lassen), Kata 11_03 (Einbau in die Pipeline).

## Minimalpfad

Punkte 1, 2 und 5.

## Teil 1 — Architekturregeln als Test

1. **Schichtregeln** mit NetArchTest oder ArchUnitNET, je als eigener Testfall:
   - Domain darf **nichts** referenzieren (kein EF Core, kein ASP.NET, kein Newtonsoft)
   - Infrastructure darf nicht von Web abhaengen
   - keine Zyklen zwischen Namespaces
   - Handler sind `internal` und `sealed`, wo sie es sein sollen
2. **Beweise, dass die Regel greift**: baue jede Verletzung einmal absichtlich ein und zeig
   den roten Test. Eine Architekturregel, die nie rot war, ist nicht verifiziert.
3. Konventionen, die sonst im Review verhandelt werden, hier festschreiben: Endungen
   (`*Handler`, `*Validator`), keine `public` Setter in Entitaeten, keine
   `DateTime.Now`-Nutzung ausserhalb einer Zeitabstraktion, kein `Console.WriteLine`
   in Bibliotheksprojekten.
4. **Roslyn-Analyzer und `.editorconfig`** als erste Verteidigungslinie:
   `TreatWarningsAsErrors`, Nullable-Warnungen als Fehler, `dotnet format --verify-no-changes`
   im Build. Danach ein eigener kleiner Analyzer **oder** eine
   `BannedApiAnalyzers`-Liste fuer eine API, die in deinem Projekt verboten sein soll.

## Teil 2 — Testqualitaet statt Testmenge

5. **Mutation Testing** mit Stryker.NET: lass es Mutanten in deinen Code setzen und zaehle,
   wie viele deine Tests **nicht** bemerken. Der Mutation Score ist die ehrlichere Zahl als
   Coverage — such dir bewusst eine Datei mit hoher Coverage und schlechtem Score.
6. Repariere die drei aussagekraeftigsten ueberlebenden Mutanten. Schreib auf, welche
   Art von Test dafuer gefehlt hat (Grenzwert, negativer Fall, Reihenfolge).
7. Zeig den Fall, der jeden Coverage-Fetisch beendet: ein Test **ohne jede Assertion**
   erzeugt 100 % Coverage fuer die durchlaufene Methode und faengt nichts.
8. Coverage-Gate mit Ausnahmen: generierter Code, Migrations und `Program.cs` gehoeren nicht
   in die Zahl. Konfiguriere die Ausschluesse und begruende jeden.

## Teil 3 — Einbau

9. Alles in eine Pipeline-Stufe, die den Build **rot machen kann**: Format, Analyzer,
   Architekturtests, Coverage-Gate, Mutation-Score-Schwelle. Danach eine Verletzung pushen
   und den roten Lauf zeigen.
10. Die Gegenprobe, damit das Ganze nicht zur Buerokratie wird: notier fuer jede Regel, was
    sie in der Praxis verhindert. Regeln ohne diesen Satz werden geloescht.

## Beispiele und Testfaelle

- Ein `using Microsoft.EntityFrameworkCore;` in `KataTracker.Domain/Kata.cs` macht **genau**
  den Schichtentest rot ("Domain darf nichts referenzieren"); alle anderen Architekturtests
  bleiben gruen.
- Eine Projektreferenz von `KataTracker.Infrastructure` auf `KataTracker.Web` macht den
  Abhaengigkeitstest rot — auch dann, wenn keine einzige Klasse daraus benutzt wird.
- Eine Klasse `CreateKataHandler` mit `public class` statt `internal sealed` macht den
  Konventionstest rot und nennt in der Fehlermeldung den Typnamen, nicht nur die Anzahl.
- `DateTime.Now` in `StreakCalculator` macht den Zeitabstraktions-Test rot; derselbe Aufruf
  in einem Testprojekt oder in `Program.cs` darf ihn **nicht** rot machen.
- Eine falsch eingerueckte Datei laesst `dotnet format --verify-no-changes` mit Exit-Code
  ungleich 0 abbrechen, bevor ueberhaupt ein Test laeuft.
- Stryker mutiert in `StreakCalculator` das `>` zu `>=` beim Tagesabstand: dieser Mutant
  **ueberlebt**, solange kein Test die Grenze "genau ein Tag Pause" prueft — nach dem
  Grenzwerttest **stirbt** er.
- Stryker ersetzt den Rueckgabewert von `Kata.Tags` durch eine leere Liste: der Mutant
  ueberlebt einen Test ohne Assertion und stirbt erst mit einer Zusicherung auf den Inhalt.
- Ein Test, der `TotalDuration()` nur aufruft und nichts prueft, ergibt 100 % Coverage fuer
  die Methode und einen Mutation Score von 0 % — beide Zahlen im Bericht nebeneinander
  zeigen.

## Nachweise

Ein Screenshot oder Log je Verletzung mit rotem Test, der Mutation Score vorher/nachher,
und ein roter Pipeline-Lauf.

## Skills

NetArchTest/ArchUnitNET, Fitness Functions, Roslyn-Analyzer, `.editorconfig`,
`BannedApiAnalyzers`, Stryker.NET, Mutation Score, Coverage-Grenzen

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
