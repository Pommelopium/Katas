# Kata 04_08 — Kassenbuch

**Application Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/application-katas/kassenbuch/)

## Ziel

Eine Anwendung entwickeln, die Bargeldbestaende monatlich revisionssicher dokumentiert.

## Anforderungen

- Bareinlagen und Barentnahmen mit Datum, Art und Betrag buchen
- Aktuellen Kassenbestand und den Vortrag aus dem Vormonat anzeigen
- Standardmaessig den aktuellen Monat oeffnen, wahlweise andere Monate
- Aenderbarkeit: aktueller Monat uneingeschraenkt, Vormonate nur nach Bestaetigung
- Revisionssicher: alle Veraenderungen bleiben nachvollziehbar
- Entscheiden und begruenden: sind Buchungen aenderbar oder nur ueber Korrekturbuchungen?
- Kein GUI erforderlich, aber benutzerfreundlich

## Beispiele und Testfaelle

- Die Revisionssicherheit ist die eigentliche Uebung: sie fuehrt zwangslaeufig zu einem Append-only-Modell -- vergleiche das mit dem Event-Sourcing-Ansatz aus Kata 11_01 der Roadmap

## Variationen und Randbedingungen

- Reports ueber mehrere Monate oder Jahre, CSV-Export
- Zugriff von mehreren Rechnern auf denselben Datenbestand
- Benutzerrechte

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

