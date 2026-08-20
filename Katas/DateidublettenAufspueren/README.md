# Kata 03_02 — Dateidubletten aufspueren

**Library Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/library-katas/dateidubletten-aufspueren/)

## Ziel

Eine Bibliothek entwickeln, die Dateidubletten in Verzeichnisbaeumen findet -- in zwei Phasen: grobe Vorfilterung, dann genaue Inhaltspruefung.

## Anforderungen

- Erste Methode durchsucht den Verzeichnisbaum und vergleicht Dateien nach Groesse und Name oder nur nach Groesse
- Zweite Methode prueft die Kandidaten per MD5-Hash auf echte Gleichheit
- Interface `IDublettenpruefung` mit zwei Ueberladungen von `Sammle_Kandidaten()` und einer `Pruefe_Kandidaten()`-Methode
- Interface `IDublette` mit einer Eigenschaft fuer die Dateipfade
- Enum `Vergleichsmodi` mit `Groesse_und_Name` und `Groesse`

## Beispiele und Testfaelle

- Eine Kandidatenliste kann mehrere Dubletten-Gruppen enthalten -- das Rueckgabemodell muss das abbilden

## Variationen und Randbedingungen

- Mechanismus fuer Fortschrittsmeldungen ergaenzen

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

