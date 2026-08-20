# Kata 13_04 — Arbeiten mit KI-Assistenz

**Stufe 5: Differenzierung** · Zeitrahmen: 1 Abend, danach dauerhaft

## Ziel

Kata 13_02 baut LLM-Funktionen **in** Software. Diese Kata uebt das andere: mit
KI-Assistenz entwickeln, ohne die Verantwortung fuer den Code abzugeben. Die
entscheidende Frage lautet nicht "nutzt du Copilot", sondern "woran erkennst du, dass
generierter Code falsch ist".

## Aufgabe: der RateLimitedFetcher, zweimal gebaut

Der Arbeitsgegenstand ist Kata 08_01 (`RateLimitedFetcher`): ein Abrufer, der eine Liste von
URLs mit begrenzter Parallelitaet laedt, den Abbruch durchreicht und seine Wartezeiten ueber
`TimeProvider` testbar macht. Genau die Aufgabe, bei der ein Assistent fluessig aussehenden
Code liefert und dabei zuverlaessig den `CancellationToken`, die Freigabe des Semaphores im
`finally` oder die testbare Zeit verfehlt.

Du baust ihn zweimal — einmal ohne Assistenz, einmal mit — und fuehrst dabei ein Protokoll
(`PROTOKOLL.md` im Kata-Ordner) mit Dauer, Fehlerarten und getroffenen Entscheidungen. Als
zweiten Gegenstand nimmst du eine Legacy-Klasse aus Kata 07_04 und laesst sie dir erklaeren.
Das Protokoll ist die Abgabe dieser Kata, nicht der Code.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Du brauchst nur eine beliebige andere Kata aus
dieser Sammlung als Arbeitsgegenstand — am ergiebigsten sind die mit Nebenlaeufigkeit
(Kata 08_01, 08_02) oder SQL (Kata 10_02, 10_03).

## Die Regel dieser Kata

Der generierte Code ist ein **Vorschlag eines Fremden ohne Kontext**. Du bist der Reviewer,
und dein Name steht am Commit.

## Minimalpfad

Punkte 1, 2 und 5.

## Aufgaben

1. **Dieselbe Kata zweimal**: einmal ohne Assistenz, einmal mit. Miss beide Male die Dauer
   und zaehle die Fehler, die dein Test danach findet. Zwei Datenpunkte sind keine Statistik
   — aber der Vergleich zeigt dir, in welcher Phase (Entwurf, Tippen, Debuggen) die Hilfe
   tatsaechlich wirkt und wo sie dich verlangsamt.
2. **Fehlerjagd im generierten Code.** Lass dir bewusst Code fuer Themen generieren, in
   denen du inzwischen sattelfest bist, und such die typischen Fehler:
   - fehlender `CancellationToken` oder verschluckte Exception (Kata 08_01/08_02)
   - `async void`, `.Result`, fehlendes `ConfigureAwait` in Bibliothekscode
   - N+1-Query oder nicht-SARGable Filter (Kata 10_02)
   - Race Condition unter Nebenlaeufigkeit, die im Einzeltest nie auffaellt
   - eine erfundene API oder ein NuGet-Paket, das es nicht gibt
   - Sicherheitsluecken aus Kata 12_01, allen voran fehlende Ownership-Pruefung
   Fuehr eine Liste: **Fehlerart | wie oft | wie gefunden.** Diese Liste ist das Ergebnis.
3. **Kontext bewusst setzen** statt zu hoffen: Projektregeln als Datei im Repo
   (`CLAUDE.md`, `.github/copilot-instructions.md` o.ae.), die Konventionen, Zielframework
   und verbotene Konstrukte nennt. Zeig an einem Beispiel den Unterschied im Ergebnis mit
   und ohne diese Datei.
4. **Tests zuerst, auch hier** — und die Tests nicht von derselben Quelle wie den Code
   generieren lassen. Wenn Test und Implementierung denselben Denkfehler teilen, ist beides
   gruen und beides falsch. Beschreib, wie du das trennst.
5. **Die Grenze schriftlich ziehen**: wofuer du Assistenz nutzt (Boilerplate, Testdaten,
   unbekannte API erkunden, Refactoring-Mechanik, erster Entwurf) und wofuer nicht
   (Architekturentscheidungen, Sicherheitscode, alles ohne Test, alles was du nicht lesen
   kannst). Diese Grenze muss mit Beispielen belegt sein, nicht als Haltung.
6. Werkzeuggebrauch im Bestand: lass eine Legacy-Klasse aus Kata 07_04 erklaeren und pruef die
   Erklaerung gegen das Verhalten deiner Charakterisierungstests. Wo lag die Erklaerung
   falsch? Das ist die lehrreichste Uebung der Kata.
7. Review-Disziplin: verlange fuer jeden uebernommenen Vorschlag eine Begruendung, die **du**
   formulieren kannst. Was du nicht erklaeren kannst, kommt nicht in den Commit.
8. Der Betriebsaspekt: was darf nicht in ein Prompt-Fenster? Zugangsdaten, Kundendaten,
   personenbezogene Daten (Kata 12_02), interner Code unter Vertraulichkeit. Schreib die Regel
   auf, die fuer dich gilt.

## Beispiele und Testfaelle

- **Selbst geschriebener Test faellt durch:** dein Test
  `MaxParallel_2_erlaubt_nie_drei_gleichzeitige_Requests` ist rot beim generierten Erstentwurf,
  weil der Semaphore nicht im `finally` freigegeben wird. Nachweis: roter Lauf vor, gruener
  Lauf nach der Korrektur — beide im Protokoll.
- **Messbare Testlaufzeit:** der generierte Test zur Wartezeit wartet echt und braucht
  Sekunden; dein Test mit `FakeTimeProvider` laeuft unter 50 ms. Pruefbar an der Laufzeit
  beider Testlaeufe.
- **Verschluckte Exception:** `Cancel_nach_erstem_Request_wirft_OperationCanceledException`
  schlaegt fehl, weil der generierte Code den Abbruch abfaengt und eine Teilliste liefert.
  Fehlerart "verschluckte Exception" landet in der Liste aus Punkt 2.
- **Erfundene API:** mindestens ein Vorschlag nennt eine Methode oder ein NuGet-Paket, das es
  nicht gibt (etwa `HttpClient.GetWithRetryAsync`). Nachweis ist die Compiler- oder
  Restore-Fehlermeldung, woertlich im Protokoll.
- **Kontextdatei macht einen Unterschied:** derselbe Auftrag ("mach die Wartezeit testbar")
  einmal ohne und einmal mit `CLAUDE.md`, die `TimeProvider` fordert und `Thread.Sleep`
  verbietet. Pruefbares Ergebnis: die Antwort ohne Datei enthaelt `Task.Delay`/`Thread.Sleep`,
  die mit Datei `TimeProvider.Delay`. Diff beider Antworten ablegen.
- **Gemeinsamer Denkfehler:** Test und Implementierung aus derselben Sitzung sind beide gruen,
  obwohl die Parallelitaet um eins zu hoch gezaehlt wird. Dein handgeschriebener Test findet
  es. Beide Testlaeufe festhalten.
- **Begruendet abgelehnter Vorschlag:** mindestens ein Vorschlag wird verworfen (etwa ein
  `lock` statt `SemaphoreSlim` fuer den asynchronen Abschnitt), und die Begruendung steht als
  ein Satz im Protokoll, den du ohne Nachschlagen sagen kannst.
- **Review findet, was der Assistent uebersah:** die Erklaerung der Legacy-Klasse aus Kata
  07_04 enthaelt mindestens eine Aussage, die dein Charakterisierungstest widerlegt. Nachweis:
  Aussage, Test und Testergebnis nebeneinander.

## Fertig, wenn

Du die Liste aus Punkt 2 und die Grenze aus Punkt 5 an eigenen Beispielen erklaeren
kannst — konkret, nicht als Meinung ueber KI.

## Skills

Review generierten Codes, typische Fehlerklassen erkennen, Kontext- und Regeldateien,
Testtrennung, Verantwortungsgrenze, Vertraulichkeit

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
