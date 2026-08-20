# Kata 04_29 — Wecker

**Application Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/application-katas/wecker/)

## Ziel

Eine Anwendung entwickeln, die den Benutzer mit Musik weckt. Eingegeben wird entweder eine konkrete Weckzeit oder eine Ruhezeit.

## Anforderungen

- Aktuelle Uhrzeit permanent sekundengenau anzeigen
- Eingabe wahlweise als Weckzeit (Uhrzeit) oder als Ruhezeit (Zeitspanne)
- Nach dem Start die Restzeit bis zur Weckzeit sekundengenau anzeigen
- Musikdatei abspielen (`weckton.wav` oder `weckton.mp3`), sobald die Weckzeit erreicht ist
- Der Wecker stoppt automatisch nach Musikstart
- Manuelles Stoppen jederzeit moeglich, die Restzeitanzeige verschwindet dann

## Beispiele und Testfaelle

- Auch hier gilt: die Weck-Logik ueber `TimeProvider` von der Systemzeit entkoppeln, sonst dauert jeder Test so lange wie die Ruhezeit

## Variationen und Randbedingungen

- Die Musikdatei laeuft in Endlosschleife, bis der Benutzer sie manuell beendet

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

