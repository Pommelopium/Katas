# Kata 09_03 — CQRS ohne MediatR, dann mit

**Stufe 3: API, Persistenz, Architektur** · Zeitrahmen: 1 Abend · baut auf Kata 09_01/09_02 auf

## Ziel

Das Muster verstehen, nicht nur die Library bedienen. Wer CQRS nur als "MediatR benutzen"
kennt, faellt bei der ersten Nachfrage durch.

## Domaene: Kata-Tracker

Weiter am Kata-Tracker aus Kata 09_01/09_02: erfasst wird, welche Kata wann in welcher Zeit
geloest wurde. Die Endpunkte bleiben fachlich unveraendert — was sich aendert, ist der Weg
dahinter. Jeder Anwendungsfall wird ein eigener Command bzw. eine eigene Query:

- `CreateKataCommand(Name, Tags)` -> `Guid` — legt eine Kata an
- `RecordAttemptCommand(KataId, Duration, SolvedOn)` -> `Guid` — erfasst einen Versuch
- `GetKataByIdQuery(Id)` -> `KataDto?`
- `SearchKatasQuery(Tag, Page, PageSize)` -> `PagedResult<KataDto>`
- `GetStreakQuery()` -> `int` — laengste Serie aufeinanderfolgender Tage mit mindestens
  einem Versuch

Die Trennung ist keine Namenskonvention: Commands schreiben und laufen deshalb durch das
Transaction-Behavior, Queries lesen und tun das nicht.

## Aufgaben

1. Definiere die Vertraege:

```csharp
public interface ICommand<TResult>;
public interface IQuery<TResult>;
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>;
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>;
```

2. Schreibe einen eigenen `Dispatcher`, der den passenden Handler zur Laufzeit per DI
   aufloest. Registrierung ueber **offene Generics**
   (`services.AddScoped(typeof(ICommandHandler<,>), ...)`) und Assembly-Scanning.
3. Baue eine **Pipeline-Behavior-Kette** als Decorator:
   `Logging -> Validation -> Transaction -> Handler`.
   Jedes Behavior ist unabhaengig testbar und die Reihenfolge ist explizit konfiguriert.
4. Portiere die Endpunkte aus Kata 09_01 darauf — der Endpoint ruft nur noch den Dispatcher.
5. Erst **danach**: dasselbe mit **MediatR** nachbauen.

## Beispiele und Testfaelle

- **Aufloesung ueber DI:** `dispatcher.Send(new CreateKataCommand("Bowling", ["oop"]))` liefert
  eine neue Id; im Endpoint steht kein Handler-Typ. Ein Command ohne registrierten Handler
  bricht beim `Send` mit einer aussagekraeftigen Meldung ab (Typname des Commands), nicht mit
  `NullReferenceException`.
- **Validierung schlaegt im Behavior zu, nicht im Handler:**
  `CreateKataCommand("", [])` endet im Validation-Behavior mit einem Validierungsfehler; ein
  Spy-Handler zaehlt dabei **0** Aufrufe.
- **Reihenfolge nachgewiesen:** Fake-Behaviors schreiben ihren Namen vor und nach dem
  `next()`-Aufruf in eine Liste. Erwartet fuer einen gueltigen Command:
  `Logging-vor, Validation-vor, Transaction-vor, Handler, Transaction-nach, Validation-nach,
  Logging-nach`.
- **Abbruch kuerzt die Kette:** derselbe Aufbau mit ungueltiger Eingabe ergibt
  `Logging-vor, Validation-vor, Logging-nach` — das Transaction-Behavior wird nie betreten,
  also auch keine Transaktion geoeffnet.
- **Rollback:** wirft der Handler von `RecordAttemptCommand` nach dem Schreiben, ist danach
  kein Versuch persistiert (Anzahl der Attempts in der Datenbank unveraendert).
- **Query ohne Seiteneffekt:** `GetStreakQuery` zweimal hintereinander liefert dasselbe
  Ergebnis; das Transaction-Behavior zaehlt fuer Queries **0** Durchlaeufe und es wird kein
  `SaveChanges` ausgefuehrt.
- **Fachlicher Durchstich:** Versuche am 01., 02., 03. und 05. — ueber
  `RecordAttemptCommand` erfasst — ergeben `GetStreakQuery() == 3`. Bei 7 Katas, davon 3 mit
  Tag `oop`, liefert `SearchKatasQuery("oop", page: 1, size: 2)` zwei Eintraege bei
  `TotalCount == 3`, Seite 2 einen.
- **Der MediatR-Nachweis:** dieselbe Testsuite laeuft unveraendert gegen beide
  Verdrahtungen, nur die Registrierung wird getauscht. Gelingt das nicht, testen die Tests
  die Library und nicht das Muster.

## Reflexion (schriftlich, in dieser README ergaenzen)

- Was hat MediatR dir abgenommen?
- Was hat es versteckt?
- Wo wuerdest du dich fuer welche Variante entscheiden?

## Skills

offene Generics in DI, Decorator / Chain-of-Responsibility, CQRS, MediatR

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
