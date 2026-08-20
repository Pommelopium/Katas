# Kata 04_16 — n-back

**Application Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/application-katas/n-back/)

## Ziel

Ein Programm entwickeln, das einen n-back-Test durchfuehrt: der Proband muss erkennen, ob der aktuelle Reiz mit dem Reiz n Schritte zuvor uebereinstimmt.

## Anforderungen

- Eingabeparameter: Probandenname, n-Wert (1-9), Reizdauer in Millisekunden, Anzahl Reize (10-100)
- Buchstaben nacheinander anzeigen
- Proband meldet erkannte Wiederholungen per Knopf- oder Tastendruck
- Prozentsatz korrekter Entscheidungen anzeigen
- Testergebnisse in eine Datei protokollieren: Proband, Parameter, Reizfolge, Antworten
- Test jederzeit abbrechbar mit Ergebnisanzeige

## Beispiele und Testfaelle

- Bei 3-back und der Folge `T L H C H S C C Q L C K L H C Q T R R K C H R` sind die Wiederholungen zu erkennen, bei denen der Buchstabe drei Positionen zuvor identisch war

## Stack-Varianten

Die Fachlogik dieser Kata ist von der Oberflaeche unabhaengig -- und genau das laesst sich
hier ueben. Loese die Kata einmal, dann tausch die Praesentationsschicht aus, **ohne** den
Kern anzufassen.

- **Konsole** (Ausgangsvariante): der schnellste Weg zu gruenen Tests, keine UI-Technik im
  Weg. Empfohlen fuer den ersten Durchgang.
- **WPF**: MVVM mit `INotifyPropertyChanged`, `ICommand`, Data Binding und
  `IValueConverter`; Eingabepruefung ueber `INotifyDataErrorInfo`. Getestet werden die
  ViewModels, nicht die Views -- wenn du fuer einen Test ein `Window` brauchst, liegt Logik
  in der falschen Schicht.
- **Blazor**: dieselbe Fachlogik in Komponenten, Zustand ueber Parameter und
  `EventCallback`, Formulare mit `EditForm` und Validierung. Komponententests mit bUnit.

**Der Nachweis:** Das Projekt mit der Fachlogik darf in **keiner** Variante eine Referenz
auf WPF oder ASP.NET haben. Baust du die zweite Variante und musst dafuer den Kern
anfassen, war die Trennung nicht sauber -- notier, an welcher Stelle es gehakt hat. Das ist
die eigentliche Uebung.

**Wenn die Aufgabe mehrere Nutzer oder geteilte Daten hat:** zieh die Fachlogik hinter
einen Dienst und waehle den Transport wie in den Architecture-Katas beschrieben
(REST, gRPC oder CoreWCF). Die Oberflaechen oben bleiben dabei unveraendert -- sie reden
dann nur mit einem Client statt direkt mit dem Kern.

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

