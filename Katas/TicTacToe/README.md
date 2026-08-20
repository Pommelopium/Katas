# Kata 04_24 — Tic Tac Toe

**Application Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/application-katas/tic-tac-toe/)

## Ziel

Ein Konsolenprogramm entwickeln, mit dem zwei Spieler gegeneinander Tic Tac Toe spielen koennen.

## Anforderungen

- Das Spiel startet automatisch und zeigt ein leeres 3x3-Feld
- Spieler geben Koordinaten ein, z. B. `A0`, `C2`
- Spieler 1 nutzt `X`, Spieler 2 nutzt `O`, die Zuege wechseln
- Ungueltiger Zug auf belegtem Feld: Board erneut anzeigen, derselbe Spieler ist wieder dran
- Nach jedem Zug das aktualisierte Spielfeld anzeigen
- Kommandos `neu` und `ende`, Gross- und Kleinschreibung egal
- Nach Spielende nur noch `neu` und `ende` akzeptieren

## Beispiele und Testfaelle

- Gewinnfall: drei in Reihe -> `*** Spieler 1 gewinnt`
- Unentschieden: kein Zug mehr moeglich -> entsprechende Meldung

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

