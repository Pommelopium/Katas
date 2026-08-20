# Kata 04_23 — Ticket Scanner

**Application Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/application-katas/ticket-scanner/)

## Ziel

Eine Anwendung zur elektronischen Einlasskontrolle entwickeln, die Barcodes scannt und anzeigt, ob Einlass gewaehrt wird.

## Anforderungen

- Barcodes von Papier oder Display scannen
- Verschluesselte Daten im Barcode: Produkt, Transaktionsnummer, Gueltigkeitszeitraum
- Validierung muss schnell sein
- Bei gueltigem Barcode Platznummern bzw. Personenzahl anzeigen
- Fehlermeldungen bei abgelaufenem Ticket, falschem Code oder bereits gescanntem Ticket
- Strichcodes und QR-Codes unterstuetzen
- Symmetrische Verschluesselung, gemeinsamer Schluessel fuer Aussteller und Kontrolleur

## Beispiele und Testfaelle

- Gueltig -> Ticket-Infos anzeigen
- Ungueltig -> Grund anzeigen: ausserhalb des Gueltigkeitszeitraums, ungueltig oder bereits gescannt

## Variationen und Randbedingungen

- Mehrere Scanner im Einsatz: alle muessen die bereits gescannten Codes kennen

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

