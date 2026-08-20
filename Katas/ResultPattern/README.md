# Kata 07_02 — Result-Pattern statt Exceptions

**Stufe 1: Modernes C# und Testbarkeit** · Zeitrahmen: 2–4 h

## Ziel

Fehlerbehandlung, wie sie in Clean-Architecture-Codebases erwartet wird: erwartbare Fehler
sind Rueckgabewerte, keine Exceptions. Exceptions bleiben fuer das Unerwartete.

## Aufgabe: die Versuchserfassung des Kata-Trackers

Der **Kata-Tracker** aus Kata 07_01 bekommt seine erste fachliche Regel: das Erfassen eines
Versuchs. Eine Eingabe besteht aus Kata-Kuerzel, Datum und Dauer in Minuten und wird zu
einem `Attempt`, wenn *alle* Regeln halten:

- Das Kata-Kuerzel ist nicht leer und existiert im Trainingsplan (`07_01`, `07_02`, ...).
- Die Dauer liegt zwischen 1 und 480 Minuten.
- Das Datum liegt nicht in der Zukunft.

Alle drei Faelle sind erwartbare Benutzereingaben, keine Ausnahmen — deshalb liefert
`AttemptService.Record(...)` ein `Result<Attempt>` mit sprechendem `Error.Code`
(`attempt.kata_code_empty`, `attempt.kata_unknown`, `attempt.duration_out_of_range`,
`attempt.date_in_future`) und
niemals ein `throw`. Genau dieser Dienst wird ab Kata 09_01 als Endpunkt veroeffentlicht;
die Fehlercodes von hier werden dort zu ProblemDetails.

## Aufgaben

1. Baue den Typ:

```csharp
public readonly record struct Error(string Code, string Message);

public sealed class Result<T>
{
    public static Result<T> Success(T value);
    public static Result<T> Failure(Error error);
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure);
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> next);
}
```

2. Kein oeffentlicher Zugriff auf `Value`, ohne den Fehlerfall zu behandeln — der Typ muss
   den Missbrauch verhindern, nicht nur davon abraten.
3. Variante ohne Wert (`Result`) und `Result.Combine(params Result[])`, das alle Fehler
   sammelt statt beim ersten abzubrechen.
4. Portiere `Katas/RomanNumerals` darauf:
   `Parse("XIVX")` liefert `Result<int>.Failure(new Error("roman.invalid_sequence", ...))`.
5. Implizite Konvertierung `T -> Result<T>` fuer angenehme Aufrufe.

## Beispiele und Testfaelle

Versuchserfassung (`Record(kataId, datum, dauerMinuten)`, heute ist `2026-08-20`):

| Eingabe | Erwartetes Ergebnis |
|---|---|
| `("07_02", 2026-08-19, 95)` | `Success`, `Attempt` mit Dauer 95 |
| `("07_02", 2026-08-19, 0)` | `Failure`, Code `attempt.duration_out_of_range` |
| `("07_02", 2026-08-19, 481)` | `Failure`, Code `attempt.duration_out_of_range` |
| `("07_02", 2026-08-19, 1)` und `(..., 480)` | beide `Success` — die Grenzen sind eingeschlossen |
| `("99_99", 2026-08-19, 30)` | `Failure`, Code `attempt.kata_unknown` |
| `("", 2026-08-19, 30)` | `Failure`, Code `attempt.kata_code_empty` |
| `("07_02", 2026-08-21, 30)` | `Failure`, Code `attempt.date_in_future` |
| `("99_99", 2026-08-21, 0)` — drei Fehler | `Combine` liefert **alle drei** Codes, `Bind` nur den ersten |

Portierte RomanNumerals-Faelle:

| Eingabe | Erwartetes Ergebnis |
|---|---|
| `Parse("MCMXCIV")` | `Success(1994)` |
| `Parse("XIVX")` | `Failure`, Code `roman.invalid_sequence` |
| `Parse("IIII")` | `Failure`, Code `roman.invalid_repetition` |
| `Parse("ABC")` | `Failure`, Code `roman.invalid_symbol` |
| `Parse("")` | `Failure`, Code `roman.empty_input` — kein `Success(0)` |

Verhalten des Typs selbst: Ein Test, der `Value` eines `Failure` liest, darf gar nicht
kompilieren — dieser Fall gehoert als Kommentar in die Testklasse, nicht als Test.
`Match` ist auf beiden Zweigen genau einmal aufrufbar und liefert bei `Failure` den
`onFailure`-Wert. `Success(42).Bind(x => Result<string>.Success(x.ToString()))` ergibt
`Success("42")`; `Failure(e).Bind(...)` fuehrt die Fortsetzung **nicht** aus und
transportiert `e` unveraendert weiter.

## Fertig, wenn

Die Parse-Methode kein einziges `throw` mehr enthaelt und jeder Fehlerfall einen eigenen,
getesteten `Error.Code` hat.

## Skills

Pattern Matching, Generics, funktionale Fehlerbehandlung, `readonly record struct`

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
