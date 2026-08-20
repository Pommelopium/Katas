# Kata 05_07 — Kanban Board

**Architecture Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/architecture-katas/kanban-board/)

## Ziel

Ein Softwaresystem entwerfen, das verteilten Teams die gemeinsame Arbeit an einem digitalen Kanban Board ermoeglicht.

## Anforderungen

- Spalten (z. B. ToDo, In Dev, In Test, Done) mit individuellen WIP-Limits
- Aufgaben in die erste Spalte einsetzen und nach rechts verschieben, nur wenn dort Platz ist
- Vor der Zuordnung koennen Aufgaben zurueck, danach nur vorwaerts
- Benutzer mit Name, Email und Passwort
- Rollen: Admin (alles), Mitarbeiter (nur eigene Aufgaben verschieben), Leser (nur Ansicht)
- Echtzeitaktualisierung: Aenderungen sofort auf allen verbundenen Geraeten sichtbar

## Beispiele und Testfaelle

- Die Echtzeitaktualisierung ist der architektonisch interessante Teil -- SignalR ist hier der naheliegende .NET-Baustein

## Variationen und Randbedingungen

- Aufgaben enthalten Text, Datum pro Spalte, Farbe und spaeter Notizen
- Benachrichtigungen per Push oder Email mit Aenderungsbeschreibung
- Nach der ersten Zuordnung keine Aenderung von Text, Datum oder Farbe

## Stack-Varianten

Diese Kata entwirft ein System, kein Programm -- Transport und Oberflaeche sind damit
Entwurfsentscheidungen und keine Vorgabe. Bau den fachlichen Kern einmal und leg dann
nacheinander verschiedene Schichten darum.

### Dienst und Transport

- **REST** (Ausgangsvariante): ASP.NET Core, Ressourcen und Statuscodes, Fehler als
  ProblemDetails, OpenAPI als Vertrag.
- **gRPC**: `.proto` zuerst, Server und Client daraus generiert. Interessant sind hier die
  Streaming-Faelle, die Deadline pro Aufruf und die Frage, welche Felder du kuenftig noch
  aendern darfst.
- **WCF**: auf .NET 10 ueber **CoreWCF**. `ServiceContract`, `OperationContract`,
  `DataContract`, Bindings und `FaultException`. Der Grund, das zu ueben, ist nicht die
  Zukunft, sondern der Bestand -- in deutschen Unternehmen laeuft sehr viel WCF, und die
  interessante Faehigkeit ist der Weg **davon weg**: erst denselben Vertrag in WCF und gRPC
  ausdruecken, dann den Umstieg planen (Contract-Abbildung, `FaultException` gegen
  `StatusCode`, Sessions gegen Streams, `netTcp` gegen HTTP/2).

### Oberflaeche

- **WPF**: MVVM, Data Binding, generierter oder handgeschriebener Client gegen den Dienst
  oben.
- **Blazor**: Komponenten, `EditForm`, typisierter API-Client. Bei gRPC gehoert die Frage
  dazu, warum der Browser gRPC-Web braucht.

**Der Nachweis:** Ein fachlicher Anwendungsfall laeuft ueber **mindestens zwei** Transporte,
und die Fachlogik ist dafuer unveraendert geblieben. Halte die Vertragsartefakte
gegenueber -- OpenAPI-Dokument, `.proto` und WSDL fuer denselben Anwendungsfall -- und
notier, was jedes davon ausdruecken kann und was nicht. Diese Gegenueberstellung
lehrt mehr ueber Vertragsentwurf als die Implementierung selbst.

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

