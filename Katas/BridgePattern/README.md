# Kata 14_07 — Bruecke (Bridge)

**Strukturmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/bridge)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und **richtig anwenden** — nicht: es ueberall anwenden. Bridge ist geuebt,
wenn du in fremdem Code die zwei unabhaengigen Dimensionen findest, die jemand per Vererbung
gekreuzt hat, und wenn du danach in *jeder* Dimension eine Variante ergaenzen kannst, ohne die
andere Dimension anzufassen. Wer zwei Hierarchien baut, wo es nur eine Variationsachse gibt, hat
die Kata nicht bestanden — auch wenn das Klassendiagramm hinterher lehrbuchmaessig aussieht.

## Woran du das Muster erkennst

- **Das Leitsymptom: kombinatorische Klassenexplosion.** Zwei unabhaengige Dimensionen werden
  durch Vererbung gekreuzt, aus 3x4 werden 12 Klassen — und jede neue Variante *multipliziert*
  statt zu addieren. Die fuenfte Kanalvariante kostet nicht eine Klasse, sondern drei.
- Die Klassennamen bestehen aus **zwei Bestandteilen**, und beide variieren unabhaengig:
  `EmailReminder`, `SmsReminder`, `EmailWeeklyReport`, `SmsWeeklyReport`. Die Namen sind die
  Matrix, nur ausgeschrieben.
- Es gibt **Loecher in der Matrix**: eine Kombination fehlt, weil sie noch niemand gebraucht hat.
  Man merkt es erst, wenn sie zur Laufzeit fehlt — der Compiler kennt die Matrix nicht.
- Derselbe technische Code steht **einmal pro Fachvariante kopiert** da (dreimal SMTP-Aufbau,
  dreimal Slack-Payload). Ein Bugfix am Versand muss in n Klassen gemacht werden.
- Die eine Dimension ist **fachlich**, die andere **technisch** (Plattform, Protokoll, Backend,
  Renderer). Man will die fachliche testen, ohne die technische anzufassen — und kann es nicht.
- Beim Anlegen einer neuen Klasse ist die Frage "welche Basisklasse nehme ich?" **nicht
  beantwortbar**, weil beide Antworten gleich richtig waeren.

## Aufgabe: die Benachrichtigungen des Kata-Trackers

Der **Kata-Tracker** verschickt drei Arten von Benachrichtigungen: eine **Erinnerung**
("heute noch nicht geuebt"), einen **Wochenbericht** (Versuche und Gesamtdauer der Woche) und
eine **Streak-Warnung** ("deine Serie von 12 Tagen reisst in 3 Stunden"). Jede davon geht ueber
einen von drei Kanaelen: **E-Mail**, **SMS** oder **Slack**. Die zwei Dimensionen sind
voneinander unabhaengig: der Wochenbericht weiss nichts ueber SMTP, und der SMS-Versand weiss
nichts ueber Streaks. Der heutige Stand kreuzt sie trotzdem per Vererbung:

```csharp
public abstract class Notification
{
    public abstract void Send(User user);
}

public abstract class ReminderNotification : Notification { /* Betreff, Text der Erinnerung */ }
public abstract class WeeklyReportNotification : Notification { /* Zeitraum, Versuchsliste */ }
public abstract class StreakWarningNotification : Notification { /* Serienlaenge, Restzeit */ }

public sealed class EmailReminderNotification : ReminderNotification
{
    public override void Send(User user) { /* SMTP-Aufbau + Erinnerungstext */ }
}

public sealed class SmsReminderNotification : ReminderNotification { /* ... */ }
public sealed class SlackReminderNotification : ReminderNotification { /* ... */ }

public sealed class EmailWeeklyReportNotification : WeeklyReportNotification { /* ... */ }
public sealed class SmsWeeklyReportNotification : WeeklyReportNotification { /* ... */ }
public sealed class SlackWeeklyReportNotification : WeeklyReportNotification { /* ... */ }

public sealed class EmailStreakWarningNotification : StreakWarningNotification { /* ... */ }
public sealed class SmsStreakWarningNotification : StreakWarningNotification { /* ... */ }
public sealed class SlackStreakWarningNotification : StreakWarningNotification { /* ... */ }
```

Der Schmerz ist bewusst klein und typisch: **9 konkrete Klassen fuer 3 Texte und 3 Kanaele**,
der SMTP-Aufbau dreimal kopiert, der Slack-Payload dreimal kopiert. Kommt ein vierter Kanal
(Push) und eine vierte Art (Monatsbericht) dazu, sind es **16**. Genau dieser Codeblock ist das,
was du in fremdem Code erkennen sollst.

## Aufgaben

1. Schreib den Ausgangscode oben ab — mindestens vier der neun Klassen ausformuliert, mit dem
   kopierten Versandcode — und sichere ihn mit Tests ab. Ohne gruenes Netz ist alles Folgende
   Umbau auf Verdacht.
2. **Benenne die zwei Dimensionen** schriftlich, je in einem Satz: was variiert fachlich (die
   Nachricht: Betreff, Text, Daten), was variiert technisch (der Kanal: Adressierung,
   Laengenlimit, Formatierung, Transport). Diese Trennung ist die eigentliche Arbeit; alles
   danach ist Mechanik.
3. Zieh die **Implementierung** heraus: `IChannel` mit `Send(string address, string subject,
   string body)` und `int MaxLength { get; }`. Implementiere `EmailChannel`, `SmsChannel`,
   `SlackChannel`. Diese Klassen kennen **keine** Benachrichtigungsart.
4. Behalte die **Abstraktion**: `Notification` bekommt den `IChannel` per Konstruktor als Feld
   (`protected readonly IChannel Channel`) — das ist die **Bruecke**, eine Komposition statt
   einer Vererbungskante. `Send(User)` bleibt nicht virtuell und delegiert an `Channel`.
5. Bau die drei **verfeinerten Abstraktionen** `ReminderNotification`,
   `WeeklyReportNotification`, `StreakWarningNotification`. Jede erzeugt nur noch Betreff und
   Text und weiss nicht, wohin es geht. Die neun konkreten Klassen werden geloescht.
6. **Der Beweis, Teil 1:** ergaenze einen vierten Kanal `PushChannel` — in **einer** neuen
   Datei, ohne eine der drei Notification-Klassen und ohne einen bestehenden Test anzufassen.
7. **Der Beweis, Teil 2:** ergaenze eine vierte Art `MonthlyReportNotification` — ebenfalls
   **eine** neue Datei, ohne einen der vier Kanaele anzufassen. Zusammen: **2 neue Klassen statt
   7**. Notier, welche 7 Klassen du im Ausgangscode haettest anlegen muessen.
8. Verschieb das Zusammenstecken an den Rand: welche Art auf welchem Kanal geht, entscheidet die
   Konfiguration (DI-Registrierung oder eine Factory), nicht die Klassenhierarchie. Danach ist
   jede Kombination zur **Laufzeit** waehlbar — vorher war sie eine Compilezeit-Entscheidung.
9. Optional, als Gegenprobe: bau die Loesung noch einmal mit einer `Func<Message, Task>`-
   Eigenschaft statt eines `IChannel`-Interfaces. Halt in drei Saetzen fest, ab welcher Anzahl
   von Kanalmethoden das Interface gewinnt — das ist die eigentliche Lernausgabe.

## Beispiele und Testfaelle

Nutzer `U1`: Mail `lasse@example.com`, Mobil `+491700000000`, Slack `@lasse`. Wochenbericht
`W1`: `2026-08-17` bis `2026-08-23`, zwei Versuche, Gesamtdauer 215 Minuten.

| Eingabe | Erwartetes Ergebnis |
|---|---|
| `new ReminderNotification(new EmailChannel(spy)).Send(U1)` | genau **ein** Versand an `lasse@example.com`, Betreff `Heute noch nicht geuebt` |
| `new ReminderNotification(new SmsChannel(spy)).Send(U1)` | Versand an `+491700000000`, **identischer Text**, kein Betreff — dieselbe Abstraktion, andere Implementierung |
| **Kreuzprodukt** aller Kombinationen als parametrisierter Test (`4 Arten x 4 Kanaele = 16 Faelle`) | jeder Fall: genau 1 Versand, an die zum Kanal passende Adresse, Text nicht leer, `Length <= Channel.MaxLength`. Keine Kombination fehlt, keine wirft |
| **Klassenzaehlung** als Test ueber Reflection | vorher **9** konkrete Versandklassen, nachher **6** (3 Notification + 3 Channel); nach Aufgabe 6 und 7 **8** statt **16** |
| `WeeklyReportNotification` mit `SmsChannel`, Text laenger als 160 Zeichen | der **Kanal** kuerzt auf 160 mit `...`, die Abstraktion nicht — der Test prueft, dass `WeeklyReportNotification` das Limit nirgends kennt |
| Wochenbericht `W1` ueber alle Kanaele | Gesamtdauer `215` erscheint in **jedem** Text; die Formatierung unterscheidet sich (Markdown-Fettung nur bei Slack) |
| Wochenbericht ohne Versuche | Versand findet statt, Text nennt `0 min` — kein leerer Text, keine Exception |
| Kanal wechseln zur Laufzeit: dieselbe Notification-Instanz einmal mit `EmailChannel`, dann mit `PushChannel` | beide Versande enthalten denselben Text; im Ausgangscode war das nur mit zwei verschiedenen Klassen moeglich |
| **Erweiterungsnachweis:** Testlauf nach Aufgaben 6 und 7 | alle vorherigen Tests bleiben gruen und **unveraendert**; `git diff --stat` zeigt je Schritt genau eine neue Datei |
| Kanal wirft (SMTP nicht erreichbar) | der Fehler kommt aus der Implementierung, die Abstraktion faengt ihn nicht — nachgewiesen ueber einen Channel-Stub, der wirft |

## Abgrenzung

- **Adapter:** gleiche Struktur, umgekehrte Absicht und Zeitrichtung. Adapter passt eine
  **bestehende, fremde** Schnittstelle **nachtraeglich** an etwas an, das man nicht aendern kann.
  Bridge ist **geplant**: beide Seiten entstehen zusammen und werden gemeinsam entworfen, damit
  sie sich unabhaengig weiterentwickeln koennen. Frag: hast du die Implementierung entworfen
  oder vorgefunden?
- **Strategy:** dort ist **ein** austauschbarer Algorithmus in **eine** Klasse hineingegeben.
  Bridge hat **zwei Hierarchien**, die *beide* wachsen — und der Zweck ist genau dieses
  unabhaengige Wachsen, nicht der Tausch. Faustregel: gibt es auf der Abstraktionsseite nur eine
  einzige Klasse, ist es Strategy, nicht Bridge.
- **State:** ebenfalls delegierte Implementierung, aber die Instanz wechselt **zur Laufzeit
  abhaengig vom Zustand** und die Zustandsobjekte kennen ihre Nachfolger. Bei Bridge wird die
  Implementierung einmal beim Bauen gesetzt und bleibt.
- **Abstract Factory:** loest die Frage, **wie** die passenden Paare entstehen — sie ist oft der
  Partner von Bridge (Aufgabe 8), nicht die Alternative dazu.

## Wann nicht

- **Es gibt nur eine Dimension.** Drei Kanaele, aber nur eine Nachrichtenart: dann ist Bridge
  Ueberbau. Ein Interface plus Implementierungen genuegt — das ist Strategy und heisst auch so.
- **Die zweite Dimension unterscheidet sich nur in Daten, nicht in Verhalten.** Dann reicht ein
  Parameter, ein Template oder ein `record` mit Konfigurationswerten. Zwei Hierarchien fuer zwei
  Textbausteine sind teurer als der Schmerz, den sie loesen.
- **In C# reicht oft weniger Struktur:** eine **Delegate-Eigenschaft** (`Func<Message, Task>`)
  bei nur einer Methode oder **Generics** (`Notification<TChannel> where TChannel : IChannel`),
  wenn die Kombination zur Compilezeit feststeht und typgeprueft sein soll. Nimm die Bruecke
  erst, wenn die Implementierungsseite mehrere Methoden hat oder zur Laufzeit wechseln muss.

## Skills

Bridge, Komposition statt Vererbung, Erkennen kombinatorischer Klassenexplosion, Trennung von
Abstraktion und Implementierung, parametrisierte Tests ueber Kreuzprodukte, Abgrenzung von
Adapter, Strategy und State

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
