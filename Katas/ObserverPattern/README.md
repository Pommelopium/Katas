# Kata 14_18 — Beobachter (Observer)

**Verhaltensmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/observer)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen**, es einmal **von Hand bauen** und danach gegen die eingebauten
Varianten stellen. Observer ist in C# mehrfach in Sprache und Framework vorhanden:
`event` mit `EventHandler<T>`, `IObservable<T>`/`IObserver<T>` samt Rx, und
`INotifyPropertyChanged` als spezialisierter Sonderfall fuer Datenbindung. Wer das Muster
nur benutzt, kennt die Mechanik nicht; wer es nur von Hand baut, baut in C# etwas nach, das
es schon gibt. Diese Kata macht beides und vergleicht.

Der eigentlich interessante Teil ist nicht das Anmelden, sondern das **Abmelden**. Eine
langlebige Quelle haelt Referenzen auf ihre Empfaenger — vergessene Abmeldungen sind die
klassische Speicherleck-Quelle in .NET-Anwendungen, und sie sind der Grund, warum diese
Kata am Ende einen *Nachweis* fordert und nicht nur eine Behauptung.

## Woran du das Muster erkennst

- Bei **einer** Aenderung muessen **n** andere Stellen reagieren, und die Liste dieser
  Stellen waechst mit jedem Ticket.
- Die Quelle kennt alle Empfaenger **namentlich**: sie haelt Felder mit deren konkreten
  Typen und ruft sie der Reihe nach auf.
- Ein neuer Empfaenger erfordert eine **Aenderung an der Quelle** — obwohl die Quelle
  fachlich unveraendert bleibt. Das ist der Open-Closed-Verstoss, den das Muster aufloest.
- Statt Benachrichtigung wird **gepollt**: eine Schleife oder ein Timer fragt regelmaessig
  "hat sich etwas geaendert?" und arbeitet die meiste Zeit umsonst.
- Die Empfaenger sind fachlich voneinander unabhaengig und wissen nichts voneinander — sie
  interessieren sich nur fuer dasselbe Ereignis.

## Aufgabe: die Versuchserfassung des Kata-Trackers

Der **Kata-Tracker** erfasst Uebungsversuche (siehe Kata 07_02). Sobald ein Versuch
gespeichert ist, muessen mehrere Dinge passieren: die Streak-Statistik wird fortgeschrieben,
die Abzeichenvergabe prueft, ob eine Serie erreicht wurde, und die geoeffnete
Fortschrittsanzeige aktualisiert sich. Fachlich hat das Speichern mit keinem dieser drei
Dinge zu tun.

Heute steht das so im Code:

```csharp
public sealed class AttemptRepository
{
    private readonly StreakStatistics _statistics = new();
    private readonly BadgeAwarder _badges = new();
    private readonly ProgressView _view = new();

    public void Record(Attempt attempt)
    {
        Save(attempt);

        _statistics.Update(attempt);
        _badges.Check(attempt);
        _view.Refresh(attempt);
    }
}
```

Das ist der Ausgangszustand und das Lernobjekt: drei namentlich bekannte Empfaenger, fest
verdrahtet, in fester Reihenfolge. Der vierte Empfaenger (ein E-Mail-Versand) kommt naechste
Woche, die `ProgressView` lebt nur so lange wie ein Fenster geoeffnet ist, und das Repository
lebt so lange wie die Anwendung. Genau diese Lebensdauer-Differenz ist das Speicherleck,
das am Ende nachgewiesen wird.

## Aufgaben

1. **Erst absichern, dann umbauen.** Schreib die Testfaelle unten gegen den Ausgangszustand,
   bevor du ihn anfasst. Ein Zaehler pro Empfaenger genuegt als Nachweis.
2. **Von Hand: Subject und Observer.** `IAttemptObserver` mit `OnAttemptRecorded(Attempt
   attempt)` und ein Subject mit `Subscribe(IAttemptObserver observer)` und
   `Unsubscribe(IAttemptObserver observer)`. Das Repository kennt ab jetzt keinen einzigen
   konkreten Empfaenger mehr. Ein neuer Empfaenger darf **keine** Zeile im Repository aendern.
3. **Abmelden richtig machen.** `Subscribe` gibt ein `IDisposable` zurueck, dessen
   `Dispose()` die Abmeldung erledigt — damit ist die Abmeldung an das `using`-Muster
   gebunden statt an Disziplin. Doppeltes Anmelden desselben Observers und Abmelden eines
   nie angemeldeten Observers brauchen ein definiertes Verhalten; entscheide dich, begruende
   es in einem Kommentar und nagle es mit einem Test fest.
4. **Dasselbe mit `event`.** Ersetze das eigene Subject durch
   `event EventHandler<AttemptRecordedEventArgs>`. Halt schriftlich fest, was die Sprache
   dir schenkt (Multicast-Delegate, `+=`/`-=`, Thread-sichere Invocation-List) und was sie
   dir *nicht* schenkt (Reihenfolgegarantie, Fehlerisolation, Abmeldung).
5. **Dasselbe mit `IObservable<T>`/`IObserver<T>`.** Implementiere den Vertrag vollstaendig:
   `OnNext`, `OnError`, `OnCompleted`, und `Subscribe` liefert das `IDisposable`. Klaer, was
   nach `OnCompleted` oder `OnError` passieren darf (naemlich nichts mehr) und was ein
   `Subject<T>` aus Rx zusaetzlich uebernimmt. Kuerze die Namensfrage nicht ab: dass beide
   Rollen im Framework schon Namen haben, ist der halbe Lerneffekt.
6. **Der werfende Observer.** Ein Empfaenger wirft in seiner Benachrichtigung eine Exception.
   Entscheide bewusst: Fehler pro Observer isolieren und sammeln (`AggregateException`) oder
   die Benachrichtigung abbrechen. Zeig fuer `event` und fuer die Handimplementierung, was
   jeweils **standardmaessig** passiert — bei einem Multicast-Delegate bekommen die
   nachfolgenden Handler nichts mehr.
7. **Reihenfolge und Nebenlaeufigkeit.** Formulier explizit, ob die Reihenfolge der
   Benachrichtigung Teil des Vertrags ist (Empfehlung: nein — und dann darf sich kein
   Empfaenger darauf verlassen). Danach: Anmelden und Abmelden aus einem anderen Thread
   waehrend einer laufenden Benachrichtigung, ohne
   `InvalidOperationException: Collection was modified`. Die Loesung ist eine Kopie der
   Empfaengerliste vor dem Durchlaufen; schreib den Test, der ohne Kopie rot ist.
8. **Das Speicherleck nachweisen.** Melde eine `ProgressView` an, verwirf die Referenz darauf
   und beweise per `WeakReference` und `GC.Collect()`, dass sie **noch lebt**. Danach
   derselbe Test mit Abmeldung ueber `Dispose()`: die `WeakReference` ist toter Draht. Ein
   dritter Durchlauf mit einem Weak-Event-Ansatz (`WeakReference<T>` in der Empfaengerliste
   und Aufraeumen der toten Eintraege) zeigt die Alternative — inklusive ihres Preises.
9. **Der Sonderfall im Framework.** Implementiere `INotifyPropertyChanged` an einem
   Fortschritts-ViewModel und ordne die Rollen zu: wer ist Subject, wer Observer, was ist
   `PropertyChangedEventArgs.PropertyName`. Drei Saetze genuegen — danach liest sich
   Datenbindungscode anders.

## Beispiele und Testfaelle

Versuchserfassung mit drei angemeldeten Empfaengern (`StreakStatistics`, `BadgeAwarder`,
`ProgressView`), jeder mit einem Aufrufzaehler:

| Ausgangslage und Aktion | Erwartetes Ergebnis |
|---|---|
| drei Angemeldete, ein Versuch erfasst | alle drei Zaehler stehen auf **genau 1** |
| drei Angemeldete, drei Versuche erfasst | jeder Zaehler steht auf **3**, keiner auf 2 oder 4 |
| `ProgressView` abgemeldet, dann ein Versuch | `ProgressView` bleibt bei 0, die anderen zaehlen weiter |
| kein Angemeldeter, ein Versuch erfasst | keine Exception, das Speichern gelingt trotzdem |
| derselbe Observer zweimal angemeldet | das festgenagelte Verhalten (1x oder 2x), nicht das zufaellige |
| Abmelden eines nie angemeldeten Observers | keine Exception, kein Effekt auf die anderen |

Fehlerisolation (Aufgabe 6): `BadgeAwarder` wirft bei jeder Benachrichtigung. Ein Versuch
wird erfasst -> `StreakStatistics` **und** `ProgressView` stehen trotzdem auf 1. Die
Zustellung an die anderen wird **nicht** verhindert; der Fehler geht nicht verloren, sondern
erscheint als gesammelte `AggregateException` oder im Protokoll. Gegenprobe mit dem rohen
`event`: dort ist mindestens ein Zaehler auf 0 — genau dieser Unterschied ist das Ergebnis
der Aufgabe und gehoert als zweiter Test daneben.

Nebenlaeufigkeit (Aufgabe 7): ein Observer meldet **in** seiner eigenen
`OnAttemptRecorded`-Methode einen weiteren Observer an und sich selbst ab. Die laufende
Benachrichtigung laeuft ohne `InvalidOperationException` durch; der neu angemeldete
Empfaenger bekommt das **laufende** Ereignis nicht mehr, das naechste aber schon. Ohne
Listenkopie ist dieser Test rot — fuehr ihn einmal absichtlich so aus.

Speicherleck (Aufgabe 8): `ProgressView` anmelden, nur eine `WeakReference` darauf behalten,
lokale Referenz verwerfen, `GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();` ->
`weak.IsAlive` ist **`true`**, das Objekt haengt an der Empfaengerliste des Subjects.
Derselbe Ablauf mit `Dispose()` vor dem Sammeln -> `weak.IsAlive` ist **`false`**. Beide
Faelle als je ein Test; der erste ist kein Bug im Test, sondern der Beweis fuer das Leck.

`IObservable<T>`-Vertrag (Aufgabe 5): nach `OnCompleted()` fuehrt ein weiteres `OnNext(...)`
zu **keiner** Benachrichtigung mehr; ein zweiter Aufruf von `Dispose()` auf demselben
Subscription-Objekt ist erlaubt und ohne Wirkung.

## Abgrenzung

- **Mediator** wird regelmaessig verwechselt, weil beide Muster "n Objekte reagieren auf
  etwas" umsetzen. Observer benachrichtigt **anonym**: die Quelle weiss nicht, wer zuhoert,
  und es interessiert sie nicht. Mediator **vermittelt gerichtet**: der Vermittler kennt die
  Teilnehmer und entscheidet, wer was bekommt. Frag dich: gibt es eine Stelle, die die
  Empfaenger kennen *muss*? Dann ist es Mediator.
- **Chain of Responsibility** verteilt nicht, sondern sucht **den ersten Zustaendigen** und
  kuerzt danach ab; die Reihenfolge ist fachlich bedeutsam. Observer benachrichtigt **alle**
  und sollte gerade keine Reihenfolge zusagen. Vergleiche mit Kata 14_13.
- **Messaging ueber einen Broker** (Kata 11_01) sieht aus wie Observer mit anderen Worten —
  Publisher, Subscriber, Topic — ist aber eine andere Kategorie: Prozessgrenze, Persistenz
  der Nachrichten, Zustellgarantien, Wiederholungen, Reihenfolge pro Partition. Observer ist
  In-Process, synchron und ohne jede Garantie: stuerzt der Prozess mitten in der
  Benachrichtigung ab, ist das Ereignis weg. Wer Observer benutzt und Broker-Garantien
  erwartet, baut sich still einen Datenverlust.
- **Command** kapselt eine Anfrage als Objekt, damit sie transportiert oder rueckgaengig
  gemacht werden kann; Observer transportiert eine *Tatsache*, die schon eingetreten ist.
  Ereignisnamen im Praeteritum (`AttemptRecorded`, nicht `RecordAttempt`) halten den
  Unterschied im Code sichtbar.

## Wann nicht

- **In C# fast immer nicht von Hand.** Ein `event` oder `IObservable<T>` ist kuerzer,
  bekannter und von Werkzeugen unterstuetzt. Ein eigenes Subject rechtfertigt sich nur mit
  einem Grund, den die eingebaute Variante nicht erfuellt: Fehlerisolation pro Observer,
  garantierte Reihenfolge, Weak-Referenzen oder asynchrone Handler.
- **Bei vielen Ereignissen wird der Kontrollfluss unlesbar.** Wenn ein Ereignis einen Handler
  ausloest, der ein zweites Ereignis ausloest, sagt kein Stacktrace und kein "Go to
  Definition" mehr, was eigentlich passiert. Ereigniskaskaden und -zyklen sind die typische
  Folge; ab dieser Stelle ist ein expliziter Aufruf oder ein Workflow-Objekt ehrlicher.
- **Bei genau einem Empfaenger, der immer da ist**, ist der direkte Aufruf richtig. Ein
  Subject mit einem einzigen Abonnenten ist Verpackung ohne Inhalt — und verschleiert, dass
  die Abhaengigkeit sehr wohl existiert.

## Skills

Observer, `event` und `EventHandler<T>`, Multicast-Delegates, `IObservable<T>`/`IObserver<T>`,
`INotifyPropertyChanged`, `IDisposable` als Abmeldung, Weak Events, Speicherlecks nachweisen,
Nebenlaeufigkeit bei Benachrichtigungen, Open-Closed-Prinzip

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
