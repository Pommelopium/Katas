# Kata 14_02 — Abstrakte Fabrik (Abstract Factory)

**Erzeugungsmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/abstract-factory)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und **richtig anwenden** — nicht: es ueberall anwenden. Abstract
Factory ist die Antwort auf genau eine Frage: mehrere Objekte gehoeren zu einer *Familie*
und duerfen nur gemeinsam auftreten. Wer das Muster fuer ein einzelnes Produkt zieht, hat
Factory Method ueberkompliziert. Am Ende dieser Kata sollst du beides unterscheiden koennen
und im eigenen Ausgangscode die Stelle benennen, an der die Familie sichtbar wird.

## Woran du das Muster erkennst

- Es gibt **mehrere zusammengehoerige Produkte** (Formatierer, Regelwerk, Layout), und jede
  Variante liefert von jedem Produkt eine eigene Ausfuehrung.
- Dieselbe Fallunterscheidung (`switch (land)`, `if (mandant == ...)`) taucht an **drei oder
  mehr Stellen** auf — einmal pro Produkt, statt einmal insgesamt.
- **Mischungen sind fachlich falsch, aber technisch moeglich:** Produkt A aus Familie 1 mit
  Produkt B aus Familie 2 kompiliert und geht erst zur Laufzeit oder in der Ausgabe kaputt.
- Eine neue Variante hinzufuegen heisst heute: alle Fallunterscheidungen finden und je einen
  Zweig ergaenzen. Vergisst man einen, faellt das erst spaeter auf.
- Der Aufrufer kennt das Landeskennzeichen (bzw. Mandanten, Zielsystem) nur, um es an die
  Erzeugung weiterzureichen — fachlich braucht er es nicht.

## Aufgabe: die Rechnungsdokumente des Kata-Trackers in mehreren Laendern

Der **Kata-Tracker** verkauft Trainingsplaene und stellt dafuer Rechnungen. Jedes Land
bringt drei Dinge mit, die nur zusammen stimmen: eine **Steuerregel**, eine
**Betragsformatierung** und ein **Rechnungslayout** mit landesspezifischen Pflichtangaben.
Deutschland rechnet mit 19 % Umsatzsteuer, formatiert `1.234,56 EUR` und weist die
USt-IdNr. aus; Frankreich rechnet mit 20 % TVA, formatiert `1 234,56 EUR` (schmales
Leerzeichen als Tausendertrenner) und braucht den Vermerk `TVA intracommunautaire`.

Der Ausgangszustand loest das mit derselben Fallunterscheidung an drei Stellen:

```csharp
public string CreateInvoice(string country, Invoice invoice)
{
    decimal tax = country switch
    {
        "DE" => invoice.Net * 0.19m,
        "FR" => invoice.Net * 0.20m,
        _    => throw new NotSupportedException(country)
    };

    string amount = country == "DE"
        ? (invoice.Net + tax).ToString("N2", new CultureInfo("de-DE")) + " EUR"
        : (invoice.Net + tax).ToString("N2", new CultureInfo("fr-FR")) + " EUR";

    if (country == "DE")
        return $"Rechnung {invoice.Number}\nUSt-IdNr.: {invoice.VatId}\nGesamt: {amount}";

    // und hier faengt das Copy-Paste an, das beim naechsten Land noch einmal kopiert wird
    return $"Facture {invoice.Number}\nTVA intracommunautaire: {invoice.VatId}\nTotal: {amount}";
}
```

Der Schmerz: drei Fallunterscheidungen ueber dasselbe Kriterium, und nichts verhindert
`country == "DE"` in der Steuerregel bei gleichzeitig franzoesischer Formatierung. Genau
dieses Bild ist das, was du als Abstract Factory erkennen sollst.

## Aufgaben

1. Schreibe den Ausgangscode oben ab (oder nimm eine vergleichbare Stelle aus einem eigenen
   Projekt) und markiere jede Fallunterscheidung ueber `country`. Halte die Anzahl fest —
   sie ist deine Messgroesse.
2. Ziehe die drei Produkte als eigene Schnittstellen heraus: `ITaxRule` mit
   `decimal CalculateTax(decimal net)`, `IAmountFormatter` mit `string Format(decimal value)`
   und `IInvoiceLayout` mit `string Render(Invoice invoice, string amount)`. Noch ohne Fabrik.
3. Implementiere die konkreten Produkte je Familie: `GermanTaxRule`, `GermanAmountFormatter`,
   `GermanInvoiceLayout` — und dasselbe fuer Frankreich. Die Klassennamen tragen die Familie,
   nicht mehr der Ablauf.
4. Fuehre die abstrakte Fabrik ein: `IInvoiceKitFactory` mit `CreateTaxRule()`,
   `CreateAmountFormatter()` und `CreateInvoiceLayout()`. Dazu
   `GermanInvoiceKitFactory` und `FrenchInvoiceKitFactory`.
5. Baue `InvoiceService` so um, dass er ausschliesslich gegen `IInvoiceKitFactory` und die
   Produkt-Schnittstellen arbeitet. Danach darf im Ablauf **kein** Landeskennzeichen und kein
   `switch` mehr vorkommen — die Auswahl passiert genau einmal, am Rand der Anwendung.
6. Ergaenze eine **komplette neue Produktfamilie Schweiz**: 8,1 % MWST, Format
   `1'234.56 CHF`, Layout mit dem Vermerk `MWST-Nr.` — als `SwissInvoiceKitFactory` plus drei
   neue Produktklassen. **Keine bestehende Klasse und keine bestehende Datei wird angefasst**;
   der Diff besteht nur aus neuen Dateien und der einen Registrierungszeile am Rand.
7. Haerte die Familiengrenze: die konkreten Produkte werden `internal` bzw. `private` in der
   Fabrik, sodass ein Aufrufer sie nicht mehr einzeln zusammenstecken kann. Notiere, welche
   Mischung vorher moeglich war und jetzt nicht mehr kompiliert.
8. Optional: ersetze die Fabrik durch einen DI-Container mit benannten Registrierungen und
   schreibe drei Saetze dazu, was dabei verloren geht (die Familie wird zur Konvention statt
   zum Typ) und was gewonnen wird (weniger Klassen).

## Beispiele und Testfaelle

Netto 1000,00 in allen Faellen; `Invoice.Number = "R-2026-0042"`.

| Fall | Erwartetes Ergebnis |
|---|---|
| `GermanInvoiceKitFactory` -> Steuer auf 1000,00 | `190.00` |
| `FrenchInvoiceKitFactory` -> Steuer auf 1000,00 | `200.00` |
| deutsche Familie, Gesamtbetrag formatiert | `1.190,00 EUR` |
| franzoesische Familie, Gesamtbetrag formatiert | `1 200,00 EUR` (schmales Leerzeichen, kein Punkt) |
| deutsches Layout | enthaelt `USt-IdNr.`, nicht `TVA` |
| franzoesisches Layout | enthaelt `TVA intracommunautaire`, nicht `USt-IdNr.` |
| schweizerische Familie (nach Aufgabe 6) | Steuer `81.00`, Gesamtbetrag `1'081.00 CHF` |

Dazu drei Faelle, die keine Tabellenzeile sind:

- **Keine Mischung mehr moeglich:** ein Test, der `GermanTaxRule` mit `FrenchAmountFormatter`
  und `FrenchInvoiceLayout` kombiniert, darf nach Aufgabe 7 **nicht kompilieren**. Dieser Fall
  gehoert als auskommentierter Block mit einer Zeile Begruendung in die Testklasse, nicht als
  Test. Gegenprobe *vor* Aufgabe 7: dieselbe Mischung laeuft durch und liefert
  `1 190,00 EUR` in franzoesischem Layout — deutsche Steuer, franzoesische Aufmachung. Genau
  dieser gruene Test wird durch die Familiengrenze unmoeglich, und das ist der Fortschritt.
- **Erweiterung ohne Aenderung bestehender Klassen:** nach Aufgabe 6 sind alle Tests der
  deutschen und franzoesischen Familie **unveraendert** gruen, kein bestehender Test wurde
  angepasst, und `git diff` zeigt ausser der Registrierungszeile am Rand nur neue Dateien.
- **Parametrisierter Test ueber alle Fabriken:** eine Testquelle liefert alle
  `IInvoiceKitFactory`-Implementierungen; fuer jede muss `Render(...)` einen nicht-leeren Text
  ergeben, der den zur Familie passenden Steuerbetrag enthaelt. Eine vierte Familie laeuft
  damit automatisch mit — vergisst sie ein Produkt, wird der Test rot.

## Abgrenzung

- **Factory Method** erzeugt **ein** Produkt und variiert per Unterklasse des Creators
  (Kata 14_01). Faustregel: eine Create-Methode = Factory Method, mehrere Create-Methoden,
  die zueinander passen muessen = Abstract Factory. Wenn du beim Aufschreiben nur eine
  Methode findest, nimm 14_01.
- **Builder** variiert nicht die Familie, sondern den **Bauablauf** eines einzelnen,
  komplizierten Objekts, oft schrittweise und mit optionalen Teilen. Unterschied: Builder
  hat eine Reihenfolge und ein Endprodukt, Abstract Factory hat n gleichrangige Produkte.
- **Prototype** erzeugt durch **Kopieren** eines bestehenden Exemplars statt durch Aufruf
  eines Konstruktors. Kein Familienbegriff, keine Varianten-Auswahl — nur `Clone()`.
- Erkennungsmerkmal fuer Abstract Factory: entfernst du ein Produkt aus der Familie, bleibt
  die Fabrik sinnvoll; entfernst du **alle bis auf eins**, war es nie eine Abstract Factory.

## Wann nicht

- **Es gibt nur eine Familie.** Dann sind Fabrik plus Schnittstellen reine Zeremonie. Warte,
  bis die zweite Variante wirklich anliegt — nicht, bis sie jemand vermutet.
- **C#-Alternative:** ein **DI-Container mit benannten Registrierungen** (`AddKeyedScoped`,
  Keyed Services) liefert dieselbe Auswahl ohne eigene Fabrikklassen; **Generics** oder
  ein `Func<T>`-Delegat ersetzen die Fabrik, wenn die Produkte keine gemeinsame Regel
  brauchen. Der Preis: die Familienzugehoerigkeit ist dann Konvention und Konfiguration,
  nicht mehr vom Compiler geprueft.
- **Die Produkte gehoeren gar nicht zusammen.** Wenn Mischungen fachlich erlaubt sind, ist die
  Fabrik eine kuenstliche Kopplung — dann drei unabhaengige Strategien statt einer Familie.

## Skills

Abstract Factory, Interface-Segregation, Erweiterung ohne Aenderung (Open-Closed),
Unterscheidung Erzeugungsmuster, Dependency Injection, parametrisierte Tests

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
