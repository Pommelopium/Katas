# Kata 04_11 — Leiterspiel

**Application Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/application-katas/leiterspiel/)

## Ziel

Eine Anwendung entwickeln, mit der 2 bis 4 Spieler das Brettspiel Leiterspiel (Snakes and Ladders) spielen koennen.

## Anforderungen

- Wuerfeln bestimmt die Anzahl der Felder; bei Leitern automatisch hochklettern, bei Schlangen hinunterrutschen
- 2 bis 4 Spieler ziehen reihum, ein Wurf pro Runde
- Der erste Zug setzt den Spielstein aufs Brett
- Spieleranzahl und Spielbrett vor Spielbeginn waehlbar
- Bei Erreichen des Zielfeldes wird der Gewinner angezeigt
- Wuerfeln durch explizite Benutzerinteraktion ausloesen
- Spielbretter als JPG/PNG mit einer Metadaten-Textdatei (Zeilen, Spalten, Positionen von Leitern und Schlangen)

## Beispiele und Testfaelle

- Feld 1 fuehrt ueber eine Leiter zu Feld 30; Feld 14 hat eine Schlange zu Feld 6
- Whitespace in den Metadaten ignorieren; Felder gleichmaessig verteilt

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

