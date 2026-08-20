# Kata 01_06 — Mail Followup

**Function Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/function-katas/mail-followup/)

## Ziel

Eine Funktion entwickeln, die Followup-Email-Adressen in Datums- und Uhrzeitangaben uebersetzt und daraus einen zukuenftigen Zeitpunkt berechnet.

## Anforderungen

- Signatur: `DateTime FollowupZeitpunkt(DateTime now, string emailadresse)`
- Den Teil vor `@followup.cc` als Zeitangabe parsen
- Relative Angaben unterstuetzen: Tage, Stunden, Wochen
- Absolute Angaben mit Uhrzeit unterstuetzen
- Kombinationen mehrerer Angaben unterstuetzen

## Beispiele und Testfaelle

- `7days@followup.cc` -> 7 Tage ab jetzt
- `12hours@followup.cc` -> 12 Stunden ab jetzt
- `aug15-9am@followup.cc` -> naechster 15. August, 9 Uhr
- `1week3days5hours@followup.cc` -> 1 Woche + 3 Tage + 5 Stunden ab jetzt
- `new DateTime(2013,2,4,10,30,0)` + `2weeks1day1hour` -> `new DateTime(2013,2,19,11,30,0)`

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

