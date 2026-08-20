# Kata 04_26 — ToDo

**Application Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/application-katas/todo/)

## Ziel

Eine Anwendung zur Verwaltung von Aufgabenlisten entwickeln, in der Aufgaben erfasst, organisiert und ihr Status verfolgt wird.

## Anforderungen

- Erfassung: Kurzbeschreibung ist Pflicht; erweiterte Beschreibung, URL und Fertigstellungsdatum optional
- Die URL laesst sich aus der Liste heraus aufrufen
- Farbcodierung: gelb 3 Tage vor der Deadline, rot ab der Deadline
- Reihenfolge aenderbar, Aufgaben als erledigt markierbar, archivierbar und loeschbar
- Erledigte Aufgaben nach einem Tag automatisch archivieren
- Standardansicht zeigt nur aktuelle Eintraege; archivierte auf Wunsch einblendbar

## Beispiele und Testfaelle

- Die Farbcodierung ist zeitabhaengig -- ein Grund, `TimeProvider` statt `DateTime.Now` zu verwenden, damit sie testbar bleibt

## Variationen und Randbedingungen

- Morgens eine Email mit den Tagesaufgaben versenden, konfiguriert per Datei
- Alarmuhr fuer Aufgaben mit Ton- und Dialogbenachrichtigung

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

