# Kata 04_09 — Kinokasse

**Application Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/application-katas/kinokasse/)

## Ziel

Eine Webanwendung entwickeln, mit der Nutzer Kinokarten online kaufen koennen -- von der Filmauswahl ueber die Platzreservierung bis zur Bezahlung.

## Anforderungen

- Startseite zeigt alle Vorstellungen; Nutzer waehlt Film, Wochentag und Uhrzeit
- Belegungsplan: freie Plaetze markieren, belegte grau darstellen
- Gesamtsumme dynamisch aktualisieren, Buchen-Button erst nach Platzauswahl aktivieren
- Zahlungsdialog fuer Kreditkartendaten, Weiter-Button erst nach vollstaendiger Eingabe
- Bestelluebersicht als PDF ausgeben
- Vorstellungen aus einer CSV-Datei laden (Wochentag, Uhrzeit, Saal, Preis)
- Saalbestuhlung aus Textdateien einlesen (`X` = Sitzplatz)

## Beispiele und Testfaelle

- Vorstellungsdatei mit `Der Name der Rose`, `Pumuckl`, `Der Schuh des Koenigs` mit je mehreren Zeitslots
- Saalformat: 6 Reihen mit unterschiedlicher Platzanzahl pro Reihe

## Variationen und Randbedingungen

- Kreditkartennummer per Luhn-Algorithmus validieren

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

