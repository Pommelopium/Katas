# Kata 04_28 — Vortragsbewertung

**Application Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/application-katas/vortragsbewertung/)

## Ziel

Eine Konferenz-Bewertungsanwendung entwickeln, mit der Teilnehmer Vortraege bewerten; das System erfasst, verwaltet und wertet die Bewertungen aus.

## Anforderungen

- Vortragsliste mit Titel, Sprechername und Sprecherbild anzeigen
- Das Bewertungsfenster oeffnet 30 % nach Vortragsbeginn und schliesst 30 % nach Vortragsende
- Bewertung ueber ein Ampelsystem (rot, gelb, gruen) plus optionalem Kommentar
- Identifikation ueber die Email-Adresse des Teilnehmers
- Mehrfachbewertung: die spaetere ueberschreibt die fruehere
- Geschlossene Sessions bleiben in einer Gesamtliste erreichbar
- Auswertung filterbar nach Veranstaltung, sortierbar nach Score, Stimmen und Sprecher
- Konferenzen mit Titel und Kuerzel sowie Vortraege mit Metadaten verwalten
- Automatische Email an den Sprecher nach Abschluss des Feedbacks

## Beispiele und Testfaelle

- Vortrag von 10:00 bis 11:00 Uhr -> bewertbar von 10:20 bis 11:20 Uhr

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

