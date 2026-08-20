# Kata 04_25 — Tic Tac Toe Bot

**Application Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/application-katas/tic-tac-toe-bot/)

## Ziel

Ein Konsolenprogramm entwickeln, bei dem ein Mensch gegen einen computergesteuerten Bot Tic Tac Toe spielt.

## Anforderungen

- Zufaellig bestimmen, wer beginnt (Spieler 1 = X, Spieler 2 = O)
- Der Spieler gibt Koordinaten ein (z. B. `A0`), danach zieht der Bot automatisch
- Ungueltige Zuege fuehren zur Neuanzeige ohne Aenderung
- Kommandos `neu` und `ende`, Grossschreibung irrelevant
- Spielende bei Sieg oder wenn kein Zug mehr moeglich ist
- Nach Spielende nur noch `neu` und `ende` akzeptieren

## Beispiele und Testfaelle

- Gewinnmeldung: `*** Der Bot gewinnt`

## Variationen und Randbedingungen

- Der Bot als eigene, testbare Einheit: die Zugauswahl vollstaendig ohne Konsole testen (Minimax lohnt sich hier)

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

