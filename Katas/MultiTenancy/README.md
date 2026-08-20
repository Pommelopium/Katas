# Kata 09_07 — Mandantenfaehigkeit

**Stufe 3: API, Persistenz, Architektur** · Zeitrahmen: 1–2 Abende

## Ziel

Sobald ein Produkt mehr als einen Kunden hat, ist das Thema da — und der Fehler, der dabei
passiert, ist der schlimmste, den ein Fachsystem machen kann: Kunde A sieht die Daten von
Kunde B. Diese Kata dreht sich um genau die Frage, wie man das **strukturell** ausschliesst,
statt es zu pruefen.

## Domaene: Kata-Tracker fuer mehrere Mandanten

Derselbe Kata-Tracker wie in Kata 09_01 und 09_02, jetzt aber als Produkt fuer mehrere
Kunden. Drei Mandanten sind fest verabredet, damit die Testfaelle unten benennbar bleiben:

- `sinc` — Mandant A, die eigene Firma.
- `contoso` — Mandant B, ein Kunde mit eigenen Katas, Versuchen und Trainingsplaenen.
- `fabrikam` — Mandant C, frisch angelegt und damit der Onboarding-Fall.

Mandantenbezogen sind alle Daten des Trackers: `Kata`, `Attempt` und `TrainingPlan`.
Nicht mandantenbezogen ist die Mandantenverwaltung selbst (`Tenant` mit Slug, Anzeigename,
Grenzen und Feature-Umfang) — sie ist die einzige Tabelle, die absichtlich ueber alle
Mandanten hinweg gelesen wird. Der Mandant kommt aus dem Aufruf: Subdomain
(`contoso.tracker.local`), Header (`X-Tenant: contoso`) oder Claim `tenant` im Token.
Titel und Tags von Katas duerfen sich zwischen Mandanten wiederholen — "Bowling" existiert
bei `sinc` und bei `contoso`, und das sind zwei verschiedene Katas.

Zwei Zahlen aus der Konfiguration liefern die Faelle fuer Aufgabe 6: `contoso` darf
maximal 50 Katas fuehren und 60 Requests pro Minute stellen, `sinc` 500 und 600.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Eine Entitaet und eine Abfrage darauf genuegen
als Ausgangspunkt.
**Empfohlen, nicht erforderlich:** Kata 09_02 (EF Core) fuer Punkt 3, Kata 12_01 (Auth) fuer die
Mandantenermittlung aus dem Token.

## Minimalpfad

Punkte 1, 3, 4 und 6.

## Aufgaben

1. **Isolationsmodell begruenden** — die eigentliche Entscheidung dieser Kata. Drei
   Varianten, je mit Kosten:
   - eine Datenbank pro Mandant
   - ein Schema pro Mandant
   - gemeinsame Tabellen mit `TenantId`
   Bewerte nach Isolationsgrad, Betriebsaufwand, Migrationsaufwand bei 500 Mandanten,
   Kosten und "Noisy Neighbour". Waehl eine Variante und bau sie.
2. **Mandantenermittlung** aus Subdomain, Header oder Token-Claim, als eigene Abstraktion
   (`ITenantContext`), einmal pro Request gesetzt. Fehlender oder unbekannter Mandant ist
   ein harter Fehler, kein Fallback auf "Standardmandant".
3. **Automatische Filterung** statt Disziplin: globaler Query-Filter, der `TenantId`
   erzwingt, plus Setzen der `TenantId` beim Speichern in `SaveChanges` — nicht im Handler.
   Der Entwickler soll den Mandanten *vergessen koennen*.
4. **Der wichtigste Test der Kata:** ein Test, der versucht, mit Mandant A eine Entitaet von
   Mandant B zu lesen, zu aendern und zu loeschen — ueber Id, ueber einen Join und ueber
   `IgnoreQueryFilters`. Alle Wege muessen scheitern oder leer liefern. Schreib dazu, welcher
   Weg dich am meisten ueberrascht hat.
5. Die Loecher im Netz suchen, an denen der Filter typischerweise nicht greift: rohes SQL
   (Kata 10_01), Bulk-Operationen (Kata 10_04), Cache-Keys (Kata 11_06 — ein Cache ohne
   Mandant im Key ist ein Datenleck), Hintergrundjobs (Kata 11_07 — dort gibt es keinen
   Request und damit keinen Mandantenkontext). Loese jedes einzeln.
6. **Mandantenbezogene Konfiguration**: Grenzen und Feature-Umfang pro Mandant
   (`IOptionsSnapshot` je Mandant oder eigener Resolver), plus mandantenbezogenes Rate
   Limiting, damit ein Kunde nicht die Kapazitaet aller anderen verbraucht.
7. Betrieb: `TenantId` in jedem Log, jedem Trace und jeder Metrik als Dimension — und die
   Gegenfrage, ob das personenbezogene Daten sind (Kata 12_02).
8. Migrationen bei N Mandanten: wie rollst du eine Schemaaenderung fuer 500 Datenbanken
   aus, und was passiert, wenn Mandant 237 dabei fehlschlaegt? Beschreib das Verfahren,
   auch wenn du es nicht baust.
9. Onboarding und Loeschung eines Mandanten als Ablauf: anlegen, initial befuellen,
   exportieren, restlos loeschen.

## Beispiele und Testfaelle

Alle Faelle sind automatisierte Tests. Ausgangsdaten, wenn nichts anderes dasteht: `sinc`
hat 3 Katas, `contoso` hat 2 Katas, darunter eine mit dem Titel "Bowling" bei beiden.

1. **Mandant A sieht die Daten von Mandant B nicht.** `GET /api/v1/katas` mit
   `X-Tenant: sinc` liefert genau 3 Eintraege, mit `X-Tenant: contoso` genau 2. Keine Id
   aus der einen Antwort taucht in der anderen auf. Der Gesamtbestand der Tabelle ist 5 —
   die Trennung entsteht in der Abfrage, nicht durch getrennte Datenbestaende (bzw. bei
   Datenbank-je-Mandant durch getrennte Verbindungen; halte fest, welche Variante du gebaut
   hast).
2. **Jeder Zugriffsweg auf eine fremde Id scheitert.** Mit der `KataId` einer
   `contoso`-Kata und dem Kontext `sinc`:
   - `GET /api/v1/katas/{id}` antwortet `404 Not Found` — nicht `403`, denn `sinc` darf
     nicht erfahren, dass die Id existiert.
   - `PUT`/`PATCH` auf dieselbe Id antwortet `404`, und der Titel bei `contoso` ist danach
     unveraendert.
   - `DELETE` antwortet `404`, die Zeile existiert weiter.
   - Ein Zugriff ueber den Join (`TrainingPlan` von `sinc`, der auf die fremde `KataId`
     verweist) liefert an dieser Stelle nichts.
   - `IgnoreQueryFilters()` liefert die fremde Zeile **sehr wohl** — genau deshalb gehoert
     dieser Aufruf in der Anwendung verboten. Der Test dokumentiert das Loch, und ein
     Architekturtest (Kata 07_05) verbietet den Aufruf ausserhalb der Mandantenverwaltung.
3. **Der vergessene Filter macht einen Test rot.** Nimm den globalen Query-Filter aus
   Aufgabe 3 heraus (oder kommentier ihn aus) und lass die Suite laufen: Fall 1 und Fall 2
   werden rot. Genau das ist der Nachweis, dass die Isolation strukturell wirkt und nicht am
   Handler-Code haengt. Umgekehrt: ein neuer Endpunkt, der beim Speichern die `TenantId`
   nirgends erwaehnt, erzeugt trotzdem eine Zeile mit korrekter `TenantId` — gepruefte
   Nachbedingung, kein Zufall.
4. **Fehlender Mandantenkontext bricht ab, statt alles zu liefern.** Ein Request ohne
   Header, ohne passende Subdomain und ohne `tenant`-Claim antwortet `400` (bzw. `401` bei
   Token-basierter Ermittlung) mit ProblemDetails — er antwortet **nicht** mit den Daten
   aller Mandanten und nicht mit einer leeren Liste. Ein unbekannter Mandant (`X-Tenant:
   gibtsnicht`) antwortet ebenso `400`; es entsteht kein Mandant und kein Fallback auf einen
   Standardmandanten. Derselbe Fall im Code: eine Abfrage ohne gesetzten `ITenantContext`
   wirft, statt ungefiltert zu laufen.
5. **Rohes SQL, Bulk und Cache haben dieselbe Grenze.** Je ein Fall pro Loch aus Aufgabe 5:
   - Eine Dapper-Abfrage (Kata 10_01) ohne `TenantId` im `WHERE` findet 5 Zeilen — mit
     Mandantenparameter 3. Der Test fixiert die zweite Zahl.
   - `ExecuteDelete`/`ExecuteUpdate` (Kata 10_04) im Kontext `sinc` laesst die 2 Zeilen von
     `contoso` unberuehrt.
   - Zwei Mandanten fragen denselben logischen Schluessel ab ("Kata mit Titel Bowling").
     Der Cache-Key enthaelt den Mandanten: `contoso` bekommt seine eigene Kata, nicht die
     zuvor von `sinc` eingelagerte. Ohne Mandant im Key wird dieser Test rot — fuehr ihn
     einmal so aus.
6. **Hintergrundjob ohne Request hat trotzdem einen Mandanten.** Ein Job (Kata 11_07), der
   die Streak-Statistik neu berechnet, laeuft ueber alle Mandanten und schreibt pro Mandant
   nur in dessen Daten. Ein Job, der ohne gesetzten Mandanten startet, bricht ab statt
   ungefiltert zu arbeiten. Kein Ergebnis eines Mandanten landet beim anderen.
7. **Grenzen und Limits gelten pro Mandant.** `contoso` legt die 51. Kata an und bekommt
   `409`/`422` mit ProblemDetails; `sinc` legt zur selben Zeit die 51. Kata an und bekommt
   `201`. Beim Rate Limiting verbraucht `contoso` sein Budget bis `429` — die naechste
   Anfrage von `sinc` wird im selben Moment normal beantwortet.
8. **Onboarding und Loeschung sind Ablaeufe mit pruefbarem Ergebnis.** `fabrikam` wird
   angelegt: das Schema ist migriert (bzw. die Zeilen sind angelegt), der Seed enthaelt die
   vereinbarten Startdaten, und `GET /api/v1/katas` mit `X-Tenant: fabrikam` liefert genau
   diesen Seed — nicht die Daten von `sinc` oder `contoso`. Danach `fabrikam` restlos
   loeschen: keine Zeile mit dessen `TenantId` bleibt uebrig (Pruefung mit
   `IgnoreQueryFilters()`), und die Bestaende von `sinc` und `contoso` sind unveraendert.
   Fuer den Migrationsfall aus Aufgabe 8: ein Lauf ueber drei Mandanten, bei dem der zweite
   fehlschlaegt, laesst den ersten migriert und den dritten unangetastet — und der
   Wiederholungslauf ist idempotent.

## Nachweise

Die Testsuite aus Punkt 4 und 5 — jeder Zugriffsweg einmal versucht und abgewiesen. Plus
eine halbe Seite zur Entscheidung aus Punkt 1.

## Skills

Multi-Tenancy-Modelle, Tenant-Resolution, globale Query-Filter, Datenisolation als
Struktur, mandantenbezogene Konfiguration und Limits, Migrationen ueber viele Mandanten

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
