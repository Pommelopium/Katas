# Kata 14_22 — Besucher (Visitor)

**Verhaltensmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/visitor)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und **richtig anwenden** — nicht: es ueberall anwenden. Visitor loest
genau eine Aufgabe: eine neue Operation ueber eine bestehende Typhierarchie hinzufuegen, ohne
die Typen anzufassen. Der Kern dieser Kata ist aber nicht das Interface, sondern die Abwaegung
dahinter — das **Expression Problem**: Visitor macht neue **Operationen** billig (eine neue
Klasse, null Aenderungen an der Hierarchie) und neue **Typen** teuer (jeder Besucher muss
angefasst werden). Vererbung mit virtuellen Methoden macht es genau umgekehrt: ein neuer Typ ist
eine Datei, eine neue Operation dagegen eine Methode in jeder Klasse der Hierarchie. Eines von
beidem musst du billig machen, beides gleichzeitig geht nicht.

Deshalb hat diese Kata zwei Haelften. In der ersten fuegst du Operationen hinzu und freust dich.
In der zweiten fuegst du einen Typ hinzu und zaehlst, wie viele Dateien rot werden. Am Ende
sollst du fuer eine konkrete Hierarchie in deinem eigenen Code sagen koennen, in welche Richtung
sie waechst — und daraus, ob Visitor die richtige Wahl ist.

## Woran du das Muster erkennst

- Die **Typhierarchie ist stabil**, die Menge der Auswertungen darauf nicht: die Knotentypen
  stehen seit Monaten, aber jede Woche kommt eine neue Frage an denselben Baum.
- **Fachfremde Operationen sammeln sich in den Domaenenklassen:** `ToJson()`, `ToHtml()`,
  `Drucke()`, `BerechnePreis()` haengen an einem Typ, der fachlich mit Export, Druck und Preisen
  nichts zu tun hat. Jede neue Ausgabeform verbreitert jede Klasse.
- Beim Aufrufer stehen **`is`-/`as`-Kaskaden ueber die Typen der Hierarchie** — dieselbe Kaskade
  in mehreren Methoden, und beim naechsten Typ vergisst genau eine davon den neuen Fall.
- Eine Operation braucht **Kontext, den die Domaenenklasse nicht haben soll**: der Exporteur
  kennt Zieldateien, Kulturen, Formatierungsoptionen. Das alles in die Knotenklasse zu ziehen,
  waere der falsche Weg.
- Eine Auswertung sammelt **eigenen Zustand ueber den ganzen Durchlauf** (Summen, Tiefen, Liste
  der Fehler). In einer virtuellen Methode je Knoten hat dieser Zustand keinen Ort.

## Aufgabe: die Punkteformel des Kata-Trackers

Im **Kata-Tracker** legt jeder Trainingsplan fest, wie ein Versuch bewertet wird — als kleiner
Formelbaum. Die Hierarchie ist klein und seit Monaten unveraendert:

- `Zahl(double Wert)` — eine Konstante, etwa `1.5`
- `Kennzahl(string Name)` — ein Messwert des Versuchs, etwa `dauer`, `tests`, `versuche`
- `Summe(Formel Links, Formel Rechts)`
- `Produkt(Formel Links, Formel Rechts)`

Die Formel `Produkt(Summe(Kennzahl("tests"), Zahl(0)), Zahl(1.5))` soll drei Dinge koennen:
**auswerten** gegen eine Messwerttabelle, sich als Text **ausgeben** (`((tests + 0) * 1.5)`) und
sich **vereinfachen** (`+ 0` und `* 1` fallen weg, `* 0` wird `0`, zwei Konstanten werden
zusammengerechnet). Spaeter kommt eine vierte Frage dazu — und genau das ist der Punkt.

Heute sieht es so aus:

```csharp
public abstract class Formel
{
    // Fachfremd: der Knoten kennt Textausgabe und bald auch JSON, HTML, SQL ...
    public abstract string AlsText();
}

public sealed class Produkt : Formel
{
    public Formel Links { get; }
    public Formel Rechts { get; }
    public override string AlsText() => $"({Links.AlsText()} * {Rechts.AlsText()})";
}

public sealed class FormelRechner
{
    public double Auswerten(Formel formel, IReadOnlyDictionary<string, double> messwerte)
    {
        // Dieselbe Kaskade steht noch in Vereinfachen und in ZaehleKennzahlen.
        if (formel is Zahl zahl) return zahl.Wert;
        if (formel is Kennzahl kennzahl) return messwerte[kennzahl.Name];
        if (formel is Summe summe)
            return Auswerten(summe.Links, messwerte) + Auswerten(summe.Rechts, messwerte);
        if (formel is Produkt produkt)
            return Auswerten(produkt.Links, messwerte) * Auswerten(produkt.Rechts, messwerte);

        throw new NotSupportedException(formel.GetType().Name);
    }
}
```

Zwei Symptome in einem Bild: `AlsText` steckt mitten im Domaenentyp, und die `is`-Kaskade steht
dreimal im Aufrufer — jedes Mal mit einem `throw` am Ende, das erst zur Laufzeit zuschlaegt.

## Aufgaben

1. Bau den Ausgangszustand nach: die vier Knotentypen, `AlsText()` in der Hierarchie, den
   `FormelRechner` mit `is`-Kaskade. Schreib die Tests fuer die Beispiele unten gegen **diesen**
   Stand — sie sind dein Netz fuer alles Folgende. Halte fest, an wie vielen Stellen die Kaskade
   steht; das ist deine Messgroesse.
2. Fuehr `IFormelBesucher<T>` ein: **genau eine Methode je Knotentyp**
   (`T Besuche(Zahl zahl)`, `T Besuche(Kennzahl kennzahl)`, `T Besuche(Summe summe)`,
   `T Besuche(Produkt produkt)`). Der Rueckgabetyp ist generisch, damit Auswerten `double` und
   Ausgeben `string` liefern kann, ohne dass jemand castet.
3. Gib der Hierarchie `abstract T Accept<T>(IFormelBesucher<T> besucher)`, und implementier sie
   in jedem Knoten als einzeiliges `besucher.Besuche(this)`. Das ist **Double Dispatch**: der
   erste Sprung waehlt ueber die virtuelle Methode den Knotentyp, der zweite ueber die
   Ueberladung die passende Besuchermethode. Nenn den Mechanismus in einem Satz schriftlich —
   und pruef danach, dass in keinem Besucher noch ein `is`, `as` oder `GetType()` vorkommt.
4. Zieh `AlsText()` aus der Hierarchie heraus. Nach diesem Schritt hat `Formel` **nur noch**
   `Accept` — keine Ausgabe, kein Export, keine Berechnung. Das ist das Abnahmekriterium des
   Schritts.
5. Bau die ersten zwei Besucher: `AuswertenBesucher : IFormelBesucher<double>` (mit der
   Messwerttabelle als Konstruktorargument — genau der Kontext, der in den Knoten nichts zu
   suchen hat) und `AusgebenBesucher : IFormelBesucher<string>`.
6. Bau den **dritten** Besucher `VereinfachenBesucher : IFormelBesucher<Formel>` — er liefert
   einen neuen Baum statt eines Werts. Regel dieses Schritts: **keine Zeile** in den vier
   Knotentypen darf sich dafuer aendern. Pruef es am Diff, nicht am Gefuehl.
7. Optional als vierter Besucher, wenn du den Gewinn noch deutlicher sehen willst:
   `KennzahlenBesucher : IFormelBesucher<IReadOnlySet<string>>`, der alle benutzten Kennzahlen
   sammelt — die Validierung "welche Messwerte muss der Aufrufer liefern?" ohne den Baum zu
   veraendern.
8. Jetzt die **Gegenprobe**: fuege der Hierarchie den Knotentyp `Minimum(Formel Links, Formel
   Rechts)` hinzu. Erweitere `IFormelBesucher<T>` um `Besuche(Minimum minimum)` und zaehl, wie
   viele Dateien der Compiler rot macht und wie viele du anfassen musst. Schreib die Zahl neben
   die Messgroesse aus Aufgabe 1. Das ist das Ergebnis der Kata: eine Operation kostete eine
   neue Datei, ein Typ kostet alle.
9. Rechne die Alternative gegen: loes dieselben drei Operationen ein zweites Mal mit einem
   `switch`-Ausdruck ueber `sealed`-Knotentypen und Positionsmustern, ohne `Accept` und ohne
   Besucherinterface. Vergleiche in drei Saetzen Zeilenzahl, Lesbarkeit und — der entscheidende
   Punkt — was beim Hinzufuegen von `Minimum` passiert: Compilerfehler oder erst eine Exception
   zur Laufzeit.

## Beispiele und Testfaelle

Referenzformel `F` = `Produkt(Summe(Kennzahl("tests"), Zahl(0)), Zahl(1.5))`, Messwerte
`{ tests: 4, dauer: 95 }`.

| Fall | Erwartetes Ergebnis |
|---|---|
| `F.Accept(auswerten)` | `6.0` — `(4 + 0) * 1.5` |
| `F.Accept(ausgeben)` | `"((tests + 0) * 1.5)"` — derselbe Baum, zweiter Besucher, voellig anderes Ergebnis |
| `F.Accept(vereinfachen).Accept(ausgeben)` | `"(tests * 1.5)"`; `Accept(auswerten)` darauf bleibt `6.0` |
| `Produkt(Kennzahl("dauer"), Zahl(0))` vereinfacht | `Zahl(0)` — `AlsText` ergibt `"0"`, der Kennzahl-Zweig ist weg |
| `Summe(Zahl(2), Zahl(3))` vereinfacht | `Zahl(5)`, nicht `"(2 + 3)"` — Konstanten werden zusammengefasst |
| `F.Accept(kennzahlen)` | genau `{ "tests" }`; auf `Summe(Kennzahl("tests"), Kennzahl("dauer"))` beide, jede einmal |
| `Auswerten` mit fehlendem Messwert (`{}` statt `{ tests: 4 }`) | definierter Fehler mit dem Kennzahlnamen im Text, kein `KeyNotFoundException` aus der Tiefe |

Vier Faelle, die keine Tabellenzeile sind — sie sind der eigentliche Inhalt der Kata:

- **Einelementiger Baum:** `new Zahl(7)` allein -> `auswerten` = `7.0`, `ausgeben` = `"7"`,
  `vereinfachen` liefert einen Baum, der `7.0` ergibt, `kennzahlen` ist **leer**. Ebenso
  `new Kennzahl("tests")` allein -> `4.0` und `"tests"`. Ein Blatt ist ein vollstaendiger Baum,
  kein Sonderfall.
- **Leerer Baum:** es gibt in dieser Hierarchie **kein** leeres Element — leg fest, was
  `Auswerten(null)` bzw. eine `Summe` mit fehlendem Ast tut (`ArgumentNullException` beim
  Konstruieren, sodass es einen unvollstaendigen Baum gar nicht gibt), und teste diese
  Entscheidung. Alternativ ein `Nichts`-Knoten mit neutralem Element — dann ist es ein fuenfter
  Typ und faellt unter Aufgabe 8.
- **Der dritte Besucher kommt ohne Aenderung der Domaenentypen dazu.** Nachweis: der Commit, der
  `VereinfachenBesucher` (oder `KennzahlenBesucher`) einfuehrt, aendert **nur** neue Dateien.
  Die Tests der ersten beiden Besucher bleiben gruen, ohne angefasst zu werden. Wenn dein Diff
  eine Knotenklasse enthaelt, ist das Muster nicht sauber gebaut.
- **Der neue Typ macht alle Besucher zum Compilezeitfehler.** Nach `Besuche(Minimum minimum)` im
  Interface uebersetzt **kein** Besucher mehr — und das ist **erwuenscht**: der Compiler
  zaehlt dir die Stellen auf, die eine Entscheidung brauchen, statt sie stillschweigend in ein
  `default:` laufen zu lassen. Der Test dazu ist kein Assert, sondern die notierte Zahl: n
  Besucher = n Fehler. Genau das ist der Preis, den du in Aufgabe 9 gegen die
  `switch`-Variante haeltst, bei der derselbe neue Typ erst zur Laufzeit auffaellt.

## Abgrenzung

- **Iterator** (Kata 14_15) legt fest, in **welcher Reihenfolge** die Knoten kommen; Visitor legt
  fest, **welche Operation** auf jedem Knoten passiert. Aendert sich die Frage (Summe, Ausgabe,
  Validierung), ist es Visitor. Aendert sich der Weg durch die Struktur, ist es Iterator. Beides
  laesst sich mischen — dann entscheide bewusst, ob der Besucher selbst absteigt (wie hier) oder
  ob ein Iterator ihn mit flachen Knoten fuettert.
- **Composite** (Kata 14_08) ist meist der **Gegenstand** des Besuchs, nicht die Alternative
  dazu: das Kompositum haelt den Baum, der Besucher wertet ihn aus. Faustregel: eine Operation,
  die fuer Blatt und Gruppe fachlich dasselbe bedeutet, gehoert als Methode ins Kompositum; eine
  Operation, die je Typ etwas anderes tut und fachfremd ist, wird ein Besucher.
- **Strategy** (Kata 14_20) tauscht **einen** Algorithmus hinter **einer** Schnittstelle aus —
  eine Methode, ein Typ. Visitor ist eine Familie von Methoden ueber **mehrere** Typen. Wer nur
  eine Methode und keine Hierarchie hat, braucht Strategy.
- **Pattern Matching mit `switch`-Ausdruck** ist in modernem C# oft die richtige Antwort und
  erledigt dasselbe mit deutlich weniger Zeremonie: kein `Accept`, kein Interface, die Operation
  steht als ein zusammenhaengender Ausdruck da statt verteilt auf vier Methoden. Der Preis ist
  die **fehlende Vollstaendigkeitspruefung**: C# kennt keine echten geschlossenen Summentypen,
  also warnt der Compiler bei einem neuen Knotentyp nicht — der `switch` faellt in seinen
  Default-Zweig und wirft zur Laufzeit, oft weit entfernt von der Ursache. Visitor kauft dir
  diese Pruefung; das ist der einzige echte Vorteil, und alles andere spricht fuer den `switch`.

## Wann nicht

- **Wenn die Typhierarchie waechst, ist Visitor die falsche Richtung.** Kommt alle zwei Wochen
  ein Knotentyp dazu, aber selten eine neue Auswertung, dann zahlst du bei jedem Typ in n
  Besuchern — nimm virtuelle Methoden in der Hierarchie und stell das Expression Problem auf die
  andere Seite.
- **Bei zwei Operationen ist es Ueberbau.** Interface, `Accept` in jedem Knoten und zwei
  Besucherklassen fuer zwei `switch`-Ausdruecke sind mehr Zeremonie als Nutzen. Fang mit dem
  `switch` an und wechsle, wenn die dritte oder vierte Operation kommt und die Kaskaden anfangen,
  sich zu widersprechen.
- **Wenn der Besucher die Interna der Knoten braucht,** die niemand sonst sehen darf: Visitor
  verlangt oeffentliche Zugriffe auf die Bestandteile jedes Typs und oeffnet damit die
  Kapselung. Ist der Zustand wirklich privat, gehoert die Operation in die Klasse.

## Skills

Verhaltensmuster erkennen, Visitor und Double Dispatch, generisches Besucherinterface,
Expression Problem und die Abwaegung Operationen gegen Typen, fachfremde Operationen aus
Domaenentypen ziehen, Ausdrucksbaeume auswerten und umformen, Pattern Matching mit
`switch`-Ausdruck als Alternative, Abgrenzung zu Iterator, Composite und Strategy

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
