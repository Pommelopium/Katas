# Kata 02_05 — Linked List

**Class Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/class-katas/linked-list/)

## Ziel

Den abstrakten Datentyp Liste als verkettete Liste implementieren -- als generische Klasse `LinkedList<T>`, die `IList<T>` erfuellt.

## Anforderungen

- Interne Implementierung ueber `Element<T>`-Objekte mit Wert und Zeiger auf das naechste Element
- Die Elemente sind nach aussen nicht sichtbar
- Die externe Schnittstelle verhaelt sich wie eine Standardliste
- Alle Mitglieder von `IList<T>` implementieren, auch `Insert`, `RemoveAt`, `IndexOf`

## Beispiele und Testfaelle

- Randfaelle testen: leere Liste, Einfuegen an Position 0, Entfernen des letzten Elements, Index ausserhalb des Bereichs

## Variationen und Randbedingungen

- Doppelt verkettete Liste mit zusaetzlicher `Prev`-Eigenschaft fuer Rueckwaerts-Traversierung

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

