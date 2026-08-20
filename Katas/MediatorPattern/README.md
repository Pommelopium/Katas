# Kata 14_16 — Vermittler (Mediator)

**Verhaltensmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/mediator)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und dann **richtig anwenden** — nicht: es ueberall anwenden. Mediator
loest genau ein Problem: mehrere Komponenten muessen aufeinander reagieren, kennen sich dafuer
aber gegenseitig. Der Vermittler nimmt jeder Komponente das Wissen ueber ihre Geschwister weg
und sammelt die Zusammenhaenge an einer Stelle. Wer das Muster verstanden hat, erkennt auch die
Faelle, in denen ein Vermittler nur ein neues God Object waere.

## Woran du das Muster erkennst

- **Jede Komponente kennt jede andere:** die Konstruktoren tauschen Referenzen aus, und die
  Reihenfolge des Aufbaus entscheidet, ob es ueberhaupt laeuft (Setter statt Konstruktor, weil
  es sonst zyklisch waere).
- Die Verdrahtung ist **n zu n**: bei fuenf Komponenten stehen zehn bis zwanzig direkte Verweise
  im Code, und keine Datei zeigt, wie das Ganze zusammenhaengt.
- **Eine Komponente allein ist nicht testbar:** um ein Eingabefeld zu pruefen, muss man vier
  weitere Objekte bauen — die Testklasse wird zum Setup-Monster.
- **Eine Aenderung an einer Stelle zieht fuenf andere nach sich:** ein neues Feld bedeutet
  Anpassungen in jeder Komponente, die es beeinflussen soll.
- Die Komponenten sind fachlich **nicht wiederverwendbar**, obwohl sie generisch aussehen: sie
  haengen an genau diesem Dialog, weil sie ihre Nachbarn namentlich kennen.
- Rueckkopplungen sind schon da, nur unbemerkt: A aendert B, B aendert A — und irgendwo steht
  ein Flag mit dem Namen `_updating`, das niemand mehr anzufassen wagt.

## Aufgabe: der Dialog "Uebungssitzung starten" im Kata-Tracker

Im **Kata-Tracker** gibt es einen Dialog, mit dem ein Teilnehmer eine Uebungssitzung anlegt.
Fachlich sind es fuenf Komponenten, die sich wechselseitig aktivieren und fuellen:

- **Stufenfilter** (`StufeAuswahl`): Stufe 1 bis 4 oder "alle".
- **Katalogliste** (`KataListe`): zeigt nur die Katas der gewaehlten Stufe; leert sich, wenn der
  Filter wechselt.
- **Zeitmessung** (`ZeitmessungSchalter`): an oder aus. Ist sie an, muss eine geplante Dauer
  stehen; ist sie aus, ist das Dauerfeld gesperrt und leer.
- **Dauerfeld** (`DauerEingabe`): Minuten zwischen 1 und 480. Waehlt man eine Kata, wird der
  Vorschlagswert aus dem Zeitrahmen der Kata vorbelegt — aber nur, wenn der Benutzer noch nichts
  eigenes eingetragen hat.
- **Startknopf** (`StartKnopf`): nur aktiv, wenn eine Kata gewaehlt ist und die Dauer gueltig
  ist oder die Zeitmessung aus ist.

Heute regeln die Komponenten das unter sich:

```csharp
public sealed class StufeAuswahl
{
    public KataListe Liste = null!;          // muss nach dem Konstruktor gesetzt werden,
    public StartKnopf Knopf = null!;         // sonst waere die Verdrahtung zyklisch

    public void Waehle(int? stufe)
    {
        Stufe = stufe;
        Liste.Fuellen(_katalog.Nach(stufe));  // kennt die Liste
        Liste.Auswahl = null;
        Knopf.Aktiv = false;                  // und den Knopf
    }
}

public sealed class KataListe
{
    public DauerEingabe Dauer = null!;
    public StartKnopf Knopf = null!;
    public ZeitmessungSchalter Zeitmessung = null!;

    public void Waehle(string? kataCode)
    {
        Auswahl = kataCode;
        if (kataCode is not null && Zeitmessung.Aktiv && !Dauer.VomBenutzerGeaendert)
            Dauer.Setze(_katalog.Zeitrahmen(kataCode));   // kennt Dauerfeld und Schalter
        Knopf.Aktiv = kataCode is not null && Dauer.IstGueltig;
    }
}

public sealed class ZeitmessungSchalter
{
    public DauerEingabe Dauer = null!;
    public StartKnopf Knopf = null!;
    public KataListe Liste = null!;

    public void Schalte(bool aktiv)
    {
        Aktiv = aktiv;
        Dauer.Gesperrt = !aktiv;
        if (!aktiv) Dauer.Leeren();
        else if (Liste.Auswahl is not null) Dauer.Setze(_katalog.Zeitrahmen(Liste.Auswahl));
        Knopf.Aktiv = Liste.Auswahl is not null && (!aktiv || Dauer.IstGueltig);
    }
}
```

Das ist der Zustand, den du erkennen sollst. Jede Komponente ist fuer sich harmlos — die Regel
"wann ist der Startknopf aktiv" steht trotzdem dreimal im Code, jedes Mal ein bisschen anders.
`DauerEingabe` schreibt bei jeder Eingabe zurueck auf `StartKnopf`, und weil `KataListe` beim
Fuellen wieder die Dauer setzt, gibt es schon ein `_updating`-Flag im Original.

## Aufgaben

1. Bau den Ausgangszustand nach: fuenf Komponenten mit direkten Referenzen und der
   Setter-Verdrahtung. **Zaehl die Verbindungen** (jede Referenz einer Komponente auf eine andere)
   und schreib die Zahl auf. Schreib Tests, die das heutige Verhalten festnageln — sie muessen
   nach dem Umbau unveraendert gruen bleiben.
2. Definiere den Vertrag:

```csharp
public interface ISessionDialogMediator
{
    void Notify(object sender, DialogEvent ereignis);
}
```

   Die Komponenten bekommen den Mediator im Konstruktor und **keine** Geschwisterreferenz mehr.
3. Verschieb die Regeln in `StartSessionDialog : ISessionDialogMediator`. Die Komponenten melden
   nur noch, **was passiert ist** (`StufeGeaendert`, `KataGewaehlt`, `ZeitmessungGeschaltet`,
   `DauerGeaendert`), nicht was daraufhin zu tun ist. Kein `if` ueber Komponententypen in einer
   Komponente.
4. Die Regel "wann ist der Startknopf aktiv" steht danach **genau einmal** — an einer Stelle im
   Mediator. Loesch die drei Kopien und beweis per Test, dass alle bisherigen Faelle weiter
   stimmen.
5. Ergaenze eine **neue Komponente**: ein Kontrollkaestchen "Ergebnis sofort ins Protokoll
   schreiben", das nur aktiv sein darf, wenn die Zeitmessung an ist. Regel: **keine bestehende
   Komponentenklasse** darf dafuer angefasst werden, nur der Mediator.
6. Zaehl die Verbindungen erneut und vergleich mit Schritt 1. Halte fest, was du gewonnen hast —
   und was der Mediator dafuer an Umfang zugelegt hat.
7. Entschaerfe die **Rueckkopplung** absichtlich: `DauerGeaendert` fuehrt zu einer Aktualisierung
   des Dauerfelds. Loes das nicht mit einem `_updating`-Flag in der Komponente, sondern im
   Mediator — Reentranz-Schutz oder "setze nur, wenn sich der Wert wirklich aendert". Begruende
   die Wahl in zwei Saetzen.
8. **Gegenprobe:** teste `DauerEingabe` allein gegen einen `FakeMediator`, der nur die Ereignisse
   mitschreibt. Kein zweites Dialogobjekt im Test. Halte in einem Kommentar fest, warum genau
   dieser Test vor dem Umbau unmoeglich war.

## Beispiele und Testfaelle

| Fall | Erwartetes Ergebnis |
|---|---|
| `stufe.Waehle(2)` bei gewaehlter Kata aus Stufe 3 | Liste enthaelt nur Stufe-2-Katas, Auswahl ist `null`, Startknopf inaktiv — und **nichts weiter**: Zeitmessung und Dauerwert bleiben unveraendert |
| `liste.Waehle("14_16")`, Zeitmessung an, Dauer unberuehrt | Dauer wird auf 240 vorbelegt, Startknopf aktiv; der Fake-Mediator zaehlt **genau ein** `KataGewaehlt`-Ereignis und keine Ereignisschleife |
| `liste.Waehle("14_16")`, Dauer vorher vom Benutzer auf 90 gesetzt | Dauer bleibt **90** — die Vorbelegung ueberschreibt keine Benutzereingabe |
| `zeitmessung.Schalte(false)` | Dauerfeld gesperrt und leer, Startknopf **aktiv** (ohne Zeitmessung braucht es keine Dauer); danach `Schalte(true)` sperrt auf und belegt wieder vor |
| `dauer.Setze(0)` und `dauer.Setze(481)` | Startknopf inaktiv, Hinweis `duration_out_of_range`; `1` und `480` sind gueltig — die Grenzen sind eingeschlossen |
| Rueckkopplung (Aufgabe 7): Mediator setzt im `DauerGeaendert`-Zweig die Dauer neu | der Aufruf terminiert; ein Zaehler im Fake-Mediator steht bei **hoechstens 2** Durchlaeufen, kein `StackOverflowException` |
| Isolierter Komponententest (Aufgabe 8) | `new DauerEingabe(fake).Setze(120)` laeuft ohne Liste, Schalter und Knopf; `fake.Ereignisse` enthaelt `[DauerGeaendert]` |
| Neue Komponente (Aufgabe 5) | `git diff` beruehrt nur `StartSessionDialog` und die neue Klasse; die vier alten Komponentendateien sind unveraendert |
| Verbindungen vorher/nachher (Aufgaben 1 und 6) | vorher **11** Referenzen zwischen Komponenten, nachher **0** — jede Komponente haelt genau **eine** Referenz auf den Mediator, also 5 statt 11 |

## Abgrenzung

- **Observer** benachrichtigt **anonym** in eine Richtung: der Publisher weiss nicht, wer
  zuhoert, und die Abonnenten reagieren unabhaengig voneinander. Mediator vermittelt **gerichtet**
  — er weiss genau, welche Komponente auf welches Ereignis hin was tun soll, und diese
  Entscheidung ist der ganze Zweck. Wird der Mediator per Events verdrahtet, ist das eine
  Implementierungsfrage, kein Musterwechsel.
- **Facade** buendelt ein Subsystem fuer einen Aufrufer, aber **das Subsystem weiss nichts von
  der Fassade** — der Verkehr laeuft nur in eine Richtung. Beim Mediator kennen die Komponenten
  den Vermittler und melden aktiv an ihn zurueck; die Kommunikation ist zweiseitig. Siehe
  Kata 14_10.
- **Chain of Responsibility** (Kata 14_13) gibt **eine** Anfrage weiter, bis jemand zustaendig
  ist; die Glieder kennen nur ihren Nachfolger. Der Mediator verteilt **ein** Ereignis
  gleichzeitig an mehrere Beteiligte und entscheidet selbst, wer reagiert.
- **MediatR** heisst so, ist aber keiner: die Library ist ein **Dispatcher**, der eine Nachricht
  an genau einen Handler zustellt (plus Pipeline-Behaviors). Es gibt keine Kollegen, die sich
  gegenseitig beeinflussen, und niemand vermittelt zwischen ihnen — der Nutzen liegt in der
  Entkopplung von Aufrufer und Handler. Wer in einer CQRS-Diskussion "wir nutzen Mediator" sagt
  und MediatR meint, redet ueber Kata 09_03, nicht ueber dieses Muster.

## Wann nicht

- **Bei zwei Komponenten ist es Ueberbau:** eine Verbindung durch zwei Verbindungen plus eine
  neue Klasse zu ersetzen, verschlechtert die Lage. Der Mediator zahlt sich erst aus, wenn die
  Zahl der Verbindungen schneller waechst als die Zahl der Komponenten.
- **Der Mediator wird selbst schnell zum God Object:** alle Regeln aller Komponenten landen in
  einer Klasse, und aus fuenf kleinen Kopplungen wird eine grosse. Gegenmittel: pro
  **Dialog/Kontext** ein eigener Mediator mit fester Obergrenze an Komponenten, keine Fachlogik
  im Mediator (nur Koordination — Validierung bleibt in der Komponente oder in der Domaene), und
  beim Wachstum aufteilen statt anbauen. Waechst er ueber die Grenze, hast du das Problem
  verschoben, nicht geloest.
- Die Komponenten sind bereits **unabhaengig** und reagieren nur auf denselben Zustand. Dann
  brauchst du einen gemeinsamen Zustand mit Benachrichtigung (Observer), keinen Vermittler.

## Skills

Verhaltensmuster erkennen, n-zu-n-Kopplung aufloesen, Ereignisse statt Anweisungen, Verdrahtung
an einer Stelle, Reentranz und Rueckkopplung beherrschen, Komponenten isoliert testen mit
Fake-Mediator, Grenzen gegen God Objects, Abgrenzung zu Observer, Facade, Chain of
Responsibility und MediatR

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
