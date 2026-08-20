# Kata 14_20 — Strategie (Strategy)

**Verhaltensmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/strategy)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und dann **richtig anwenden** — nicht, es ueberall anzuwenden. Strategy
ist das am haeufigsten uebertriebene Muster: wer es einmal verstanden hat, ersetzt danach
jedes `switch` durch drei Interfaces und eine Registry. Die Kata trainiert beides, das
Erkennen der echten Stelle und das bewusste Stehenlassen der harmlosen.

## Woran du das Muster erkennst

- Mehrere Varianten **desselben** Algorithmus stehen in einem `switch` oder einer `if`-Kette:
  gleiche Eingaben, gleiches Ergebnis-Format, nur ein anderer Rechenweg.
- Die Auswahl haengt an Konfiguration, Mandant, Feature-Flag oder einer Nutzerentscheidung —
  sie ist erst zur Laufzeit bekannt, nicht beim Kompilieren.
- Eine neue Variante bedeutet die Aenderung einer **bestehenden** Klasse. Das ist der
  Open-Closed-Verstoss, den du im Code-Review riechen sollst.
- Die Klasse hat mehrere Gruende zur Aenderung: sie aendert sich, wenn ein Verfahren angepasst
  wird, *und* wenn ein Verfahren hinzukommt, *und* wenn die Auswahllogik sich verschiebt.
- Die Tests der Klasse muessen fuer jeden Zweig durch dieselbe Fassade — die Testnamen fangen
  alle mit demselben Methodennamen an und unterscheiden sich nur im Verfahren.

## Aufgabe: die Versandkosten im Kata-Shop

Der **Kata-Shop** berechnet die Versandkosten eines Warenkorbs. Es gibt drei austauschbare
Versandverfahren, ausgewaehlt vom Kunden im Checkout und vorbelegt aus der Konfiguration:

- **Standard**: 4,95 EUR pauschal, ab einem Warenwert von 50,00 EUR versandkostenfrei.
- **Express**: 9,90 EUR Grundpreis, zusaetzlich 1,50 EUR je angefangenes Kilogramm oberhalb
  von 5 kg. Kein Freibetrag, egal wie hoch der Warenwert ist.
- **Abholung**: immer 0,00 EUR.

Der Ausgangszustand ist die Stelle, die du erkennen sollst:

```csharp
public decimal CalculateShippingCost(string method, decimal orderValue, decimal weightKg)
{
    switch (method)
    {
        case "standard":
            return orderValue >= 50m ? 0m : 4.95m;
        case "express":
            var extraKg = Math.Max(0m, weightKg - 5m);
            return 9.90m + Math.Ceiling(extraKg) * 1.50m;
        case "abholung":
            return 0m;
        default:
            return 4.95m; // "im Zweifel Standard" — genau das ist der Fehler
    }
}
```

## Aufgaben

1. Beschreibe in einem Satz, warum diese Methode Strategy braucht — mit Bezug auf eine der
   Erkennungsregeln oben. Ohne diesen Satz ist der Rest Bastelei.
2. Zieh das Strategie-Interface heraus. Ein Aufruf, ein Parameterobjekt statt drei losen
   Argumenten:

```csharp
public readonly record struct ShippingRequest(decimal OrderValue, decimal WeightKg);

public interface IShippingCostStrategy
{
    string Key { get; }
    decimal Calculate(ShippingRequest request);
}
```

3. Implementiere die drei Verfahren als eigene Klassen. Jede Klasse kennt **nur** ihre eigene
   Rechnung, keine `if`-Abfrage auf einen Verfahrensnamen mehr.
4. Baue den Kontext (`ShippingCalculator` oder direkt der Checkout-Dienst). Er kennt das
   Interface, nie eine konkrete Klasse und nie den Schluessel-`switch`.
5. Auswahl per DI: registriere alle Strategien und loese sie ueber den Schluessel auf
   (`IReadOnlyDictionary<string, IShippingCostStrategy>` aus den registrierten Diensten, oder
   ein `IShippingCostStrategySelector`). Der Standardschluessel kommt aus der Konfiguration.
6. Unbekannter Schluessel: definierter Fehler, kein stiller Rueckfall auf Standard. Entweder
   eine `UnknownShippingMethodException` mit dem Schluessel im Text oder ein
   `Result<decimal>`-Failure mit Code `shipping.unknown_method`.
7. Vierte Strategie **Spedition** (29,00 EUR pauschal plus 0,80 EUR je Kilogramm): fuege sie
   hinzu, **ohne** eine bestehende Klasse anzufassen. Erlaubt ist genau eine neue Datei plus
   eine Zeile Registrierung im Startup. Wenn du mehr brauchst, ist Schritt 5 nicht fertig.
8. Zweite Variante: dieselbe Auswahl mit `Func<ShippingRequest, decimal>` statt Interface —
   ein `Dictionary<string, Func<ShippingRequest, decimal>>`, die Rechnungen als Lambdas.
   Halte schriftlich fest, wann welche Form besser passt: das `Func` gewinnt bei einer
   Methode, kurzen Rechnungen und ohne eigene Abhaengigkeiten; das Interface gewinnt, sobald
   die Strategie Dependencies per Konstruktor braucht, Metadaten wie `Key` oder einen
   sprechenden Typnamen im Stacktrace, oder wenn ein zweiter Aufruf dazukommt
   (`Calculate` **und** `IsAvailableFor`).

## Beispiele und Testfaelle

| Eingabe (`method`, Warenwert, Gewicht) | Erwartetes Ergebnis |
|---|---|
| `("standard", 20.00, 2)` | `4.95` |
| `("standard", 49.99, 2)` und `("standard", 50.00, 2)` | `4.95` bzw. `0.00` — die Grenze ist eingeschlossen |
| `("express", 20.00, 5)` | `9.90` — 5 kg loesen noch keinen Zuschlag aus |
| `("express", 20.00, 5.1)` | `11.40` — angefangenes Kilogramm zaehlt voll |
| `("express", 500.00, 8)` | `14.40` — kein Freibetrag, auch nicht bei hohem Warenwert |
| `("abholung", 0.00, 100)` | `0.00` — Gewicht und Warenwert sind irrelevant |
| `("drohne", 20.00, 2)` | definierter Fehler (`shipping.unknown_method`), **nicht** `4.95` |
| `("spedition", 100.00, 40)` | `61.00` — neue Datei plus eine Registrierungszeile, kein Diff in Standard/Express/Abholung |

Zwei Tests darueber hinaus, die nicht rechnen:

- **Kontext mit Fake-Strategie:** eine Strategie, die konstant `1.23` liefert, in den Checkout
  gegeben — die Gesamtsumme ist `Warenwert + 1.23`. Der Test beweist, dass der Kontext die
  Strategie genau **einmal** aufruft und ihr Ergebnis unveraendert uebernimmt, ohne dass
  irgendeine Versandregel im Test auftaucht.
- **Aequivalenz der beiden Varianten (Aufgabe 8):** Interface- und `Func`-Auswahl liefern fuer
  alle Zeilen der Tabelle oben identische Werte. Derselbe Testfall, zwei Aufbauten.

## Abgrenzung

- **State**: gleiche Struktur, andere Absicht. Bei Strategy waehlt der **Klient** das
  Verfahren und die Strategien wissen nichts voneinander. Bei State waehlt der **Zustand
  selbst** seinen Nachfolger — Zustaende kennen sich und schalten weiter.
- **Template Method**: dieselbe Variabilitaet ueber **Vererbung** statt **Komposition**. Die
  Unterklasse fuellt Luecken in einem festen Ablauf; austauschen kann man sie zur Laufzeit
  nicht. Strategy ist die Wahl, wenn die Variante erst zur Laufzeit bekannt ist.
- **Bridge**: trennt eine Abstraktionshierarchie von ihrer Implementierung — strukturell
  aehnlich, aber es geht um Plattformwechsel, nicht um vertauschbare Rechenwege im Fachcode.
- **Command**: kapselt **einen** Aufruf samt Argumenten zum spaeteren Ausfuehren,
  Protokollieren oder Rueckgaengigmachen. Strategy kapselt das **Wie** eines Aufrufs, der
  sofort passiert.

## Wann nicht

- Bei zwei stabilen Varianten, die sich seit Jahren nicht bewegt haben, reicht ein
  `switch`-Ausdruck. Zwei Interfaces und eine Registry sind dann teurer als die drei Zeilen,
  die sie ersetzen.
- In C# ist ein `Func<T, TResult>` oft die ehrlichere Strategie als ein Interface mit genau
  einer Methode. Wenn die Implementierung keinen Zustand und keine Abhaengigkeiten hat, ist
  die Klasse nur Zeremonie um ein Lambda.
- Wenn die Auswahl beim Kompilieren feststeht, gehoert sie in die Komposition (ein anderer
  registrierter Typ), nicht in eine Laufzeit-Auswahl per Schluessel.

## Skills

Strategy, Open-Closed-Prinzip, Dependency Injection mit mehreren Implementierungen,
Auswahl per Schluessel, `Func<T, TResult>` als Strategie, Test-Doubles ohne Framework

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
