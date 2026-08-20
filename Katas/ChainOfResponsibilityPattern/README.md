# Kata 14_13 — Zustaendigkeitskette (Chain of Responsibility)

**Verhaltensmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/chain-of-responsibility)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und dann **richtig anwenden** — nicht: es ueberall anwenden. Die
Uebung besteht aus zwei Haelften. Die erste ist das Lesen: eine Kaskade von Pruefungen
ansehen und entscheiden, ob hier wirklich eine Zustaendigkeitskette steckt oder nur eine
Fallunterscheidung, die man besser in Ruhe laesst. Die zweite ist der Umbau, und der ist
erst fertig, wenn eine neue Regel eingefuegt und die Reihenfolge geaendert werden kann,
ohne eine einzige bestehende Klasse anzufassen.

## Woran du das Muster erkennst

- Eine lange `if` / `else if`-Kaskade aus Pruefungen, die alle dieselbe Form haben:
  Bedingung pruefen, Ergebnis liefern, Rest ueberspringen.
- Die **Reihenfolge** der Pruefungen ist fachlich bedeutsam — wer sie umstellt, aendert das
  Ergebnis. Genau das ist der Unterschied zu einem `switch` ueber disjunkte Faelle.
- Die Pruefungen sollen **konfigurierbar** werden: pro Mandant, pro Umgebung, pro
  Feature-Flag eine andere Zusammenstellung derselben Bausteine.
- Die **erste zutreffende Regel gewinnt**, alle weiteren werden gar nicht mehr ausgewertet.
- Die Kaskade waechst mit jedem Ticket, und jedes Ticket aendert dieselbe Methode. Die
  Merge-Konflikte in genau dieser Methode sind das billigste Erkennungsmerkmal.
- Es gibt einen Rest: einen `else`-Zweig, der "damit kann ich nichts anfangen" bedeutet.

## Aufgabe: die Genehmigungsstrecke fuer Reisekosten

Der **Kata-Tracker** bekommt eine Nebenanwendung: die Reisekostenabrechnung fuer
Konferenzbesuche. Eine Abrechnung (`ExpenseRequest`) hat einen Betrag in Euro, eine
Kategorie (`Reise`, `Bewirtung`, `Sonstiges`) und einen Antragsteller. Wer sie genehmigen
darf, haengt an Betragsgrenzen: Teamleitung bis 500 EUR, Abteilungsleitung bis 5.000 EUR,
Geschaeftsfuehrung bis 50.000 EUR. Darueber genehmigt niemand allein — der Antrag geht an
den Aufsichtsrat, und das ist kein Fehler, sondern ein eigenes fachliches Ergebnis.

Heute steht das so im Code:

```csharp
public string Approve(ExpenseRequest request)
{
    if (request.Category == Category.Bewirtung && request.Amount > 100m)
    {
        return "Compliance";
    }
    else if (request.Amount <= 500m)
    {
        return "Teamleitung";
    }
    else if (request.Amount <= 5_000m)
    {
        return "Abteilungsleitung";
    }
    else if (request.Amount <= 50_000m)
    {
        return "Geschaeftsfuehrung";
    }
    else
    {
        return "Aufsichtsrat";
    }
}
```

Das ist der Ausgangszustand, und er ist das eigentliche Lernobjekt: fuenf Pruefungen, eine
Methode, eine bedeutsame Reihenfolge. Die Compliance-Regel steht **vor** den Betragsgrenzen
und muss dort bleiben — verschiebt man sie nach unten, wird sie von der Teamleitung
verdeckt und feuert nie. Diese Abhaengigkeit ist im Code nirgends benannt. Sie sichtbar
und testbar zu machen ist der Zweck des Umbaus.

## Aufgaben

1. **Erst absichern, dann umbauen.** Schreib die Testfaelle unten gegen die Kaskade, bevor
   du sie anfasst. Alles danach ist Refactoring mit gruenem Netz.
2. **Handler-Interface mit Nachfolger.** `IApprovalHandler` mit `SetNext(IApprovalHandler
   next)` und `ApprovalResult Handle(ExpenseRequest request)`. Eine abstrakte Basisklasse
   uebernimmt das Weiterreichen, sodass ein konkreter Handler nur seine eigene Bedingung
   und den Aufruf `base.Handle(request)` enthaelt.
3. **Ein Handler pro Regel**, jeder mit genau einem Grund zur Aenderung:
   `ComplianceHandler`, `TeamLeadHandler`, `DepartmentHeadHandler`, `ManagementHandler`.
4. **Kette zusammenstecken** an genau einer Stelle — ein `ApprovalChainBuilder` oder die
   DI-Registrierung. Der Aufrufer kennt nur den Kopf der Kette und nichts von deren Laenge.
   Die Reihenfolge ist ab jetzt Konfiguration, nicht Kontrollfluss.
5. **Der Fall "niemand ist zustaendig".** Kein `null`, keine Exception als Normalfall.
   Entweder ein Abschluss-Handler (`BoardHandler`) als letztes Glied oder ein
   `ApprovalResult.NotHandled` — entscheide dich, begruende die Wahl in einem Kommentar und
   nagle das Verhalten mit einem Test fest.
6. **Erweitern ohne anzufassen.** Ergaenze eine Regel "Betraege ueber 20.000 EUR brauchen
   zusaetzlich das Vier-Augen-Prinzip" und stell danach die Reihenfolge um, sodass
   Compliance erst nach der Teamleitung greift. Beide Aenderungen duerfen **keine** Zeile in
   einem bestehenden Handler beruehren — nur neue Klassen und die Zusammenstellung aus
   Punkt 4. Wenn du doch einen alten Handler oeffnen musst, ist die Kette falsch geschnitten.
7. **Kurzschluss messen.** Gib jedem Handler einen Aufrufzaehler und beweise per Test, dass
   nach dem zustaendigen Handler kein weiterer mehr laeuft.
8. **Dasselbe Muster im Framework.** Bau die Strecke ein zweites Mal als
   ASP.NET-Core-Middleware-Pipeline: `RequestDelegate next` ist der Nachfolger,
   `app.Use(...)` ist das Zusammenstecken, ein nicht aufgerufenes `next()` ist der
   Kurzschluss. Halt in drei Saetzen schriftlich fest, welche Rolle im Muster welchem Teil
   der Pipeline entspricht — danach liest sich fremder Middleware-Code anders.

## Beispiele und Testfaelle

Genehmigungsstrecke in der Reihenfolge Compliance -> Teamleitung -> Abteilungsleitung ->
Geschaeftsfuehrung -> Aufsichtsrat:

| Eingabe | Erwartetes Ergebnis |
|---|---|
| `(120 EUR, Reise)` | `Teamleitung` |
| `(500 EUR, Reise)` | `Teamleitung` — die Grenze ist eingeschlossen |
| `(500,01 EUR, Reise)` | `Abteilungsleitung` — ein Cent darueber kippt die Zustaendigkeit |
| `(5.000 EUR, Reise)` | `Abteilungsleitung`; `(5.000,01 EUR, Reise)` -> `Geschaeftsfuehrung` |
| `(50.000 EUR, Reise)` | `Geschaeftsfuehrung`; `(50.000,01 EUR, Reise)` -> `Aufsichtsrat` |
| `(80.000 EUR, Reise)` | niemand ist einzeln zustaendig -> `NotHandled` bzw. `Aufsichtsrat` |
| `(250 EUR, Bewirtung)` | `Compliance` — die Kategorie schlaegt die Betragsgrenze |
| `(90 EUR, Bewirtung)` | `Teamleitung` — unter 100 EUR greift Compliance nicht |

Kurzschluss (Aufgabe 7): `(250 EUR, Bewirtung)` durch die Kette -> `ComplianceHandler`
wurde genau einmal aufgerufen, `TeamLeadHandler`, `DepartmentHeadHandler` und
`ManagementHandler` **genau nullmal**. Der Zaehler ist der Beweis, nicht das Ergebnis:
dasselbe Ergebnis wuerde auch entstehen, wenn alle vier laufen.

Reihenfolge (Aufgabe 6): dieselbe Eingabe `(250 EUR, Bewirtung)` liefert bei der
umgestellten Kette Teamleitung -> Compliance -> ... nicht mehr `Compliance`, sondern
`Teamleitung`. Zwei Tests, identische Eingabe, unterschiedliche Zusammenstellung,
unterschiedliches Ergebnis — das ist die fachliche Bedeutung der Reihenfolge, jetzt
als Test sichtbar.

Leere Kette: ein Kopf ohne Glieder liefert `NotHandled` und wirft nicht. Kette mit einem
einzigen nicht zustaendigen Handler: ebenfalls `NotHandled`, und der Handler wurde genau
einmal gefragt.

## Abgrenzung

- **Decorator** sieht identisch aus — Objekte, die einen Nachfolger halten — und meint das
  Gegenteil: dort wirken **alle** Glieder am Ergebnis mit (Logging plus Caching plus Retry),
  hier gewinnt **der erste Zustaendige** und die uebrigen laufen nie. Frag dich: sollen alle
  etwas beitragen oder soll einer entscheiden? Vergleiche mit Kata 14_09.
- **Command** kapselt *eine Anfrage als Objekt*, damit sie transportiert, protokolliert oder
  rueckgaengig gemacht werden kann. Chain of Responsibility entscheidet, *wer* sie bearbeitet.
  Die Kombination ist ueblich: ein Command wandert durch eine Kette.
- **Mediator** zentralisiert die Kommunikation in einem Vermittler, den alle kennen. Die
  Kette dezentralisiert sie: jedes Glied kennt nur seinen Nachfolger, keiner das Ganze.
- **Observer** verteilt eine Nachricht an *alle* Interessenten, ungeordnet und ohne
  Kurzschluss. Sobald "der erste, der passt" oder eine Reihenfolge im Spiel ist, ist es
  keine Observer-Situation mehr.

## Wann nicht

- Bei **drei stabilen Faellen**, die sich seit Jahren nicht bewegen, ist ein
  `switch`-Ausdruck kuerzer, schneller zu lesen und vollstaendigkeitsgepruefbar. Vier
  Klassen und ein Builder sind dann Verpackung ohne Inhalt.
- Eine lange Kette ist im **Debugger unangenehm**: der Stacktrace ist tief, gleichfoermig
  und verraet nicht, welches Glied gerade entscheidet. Ohne Logging pro Glied bezahlst du
  die Flexibilitaet mit Diagnosezeit.
- **Reihenfolgeabhaengigkeit ist eine versteckte Kopplung.** Die Handler wissen nichts
  voneinander, aber das Ergebnis haengt an ihrer Anordnung — ein Umsortieren in der
  DI-Registrierung aendert Fachverhalten, ohne dass eine Fachklasse angefasst wurde. Wer
  das nicht mit Tests festnagelt, hat den Kontrollfluss nur unsichtbar gemacht.

## Skills

Chain of Responsibility, Polymorphie statt Verzweigung, Open-Closed-Prinzip, Komposition
ueber Vererbung, ASP.NET-Core-Middleware-Pipeline, Grenzwerttests, Refactoring unter Test

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
