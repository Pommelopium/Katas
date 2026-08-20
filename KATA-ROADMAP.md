# C#-Kata-Roadmap 2026

Diese Sammlung übt C#/.NET jenseits der Algorithmik: Algorithmen allein bringen einen nicht
weiter — der Übungswert liegt in der **Schicht darüber**: Architektur, Testbarkeit, Async,
APIs, Persistenz, Container, Observability.

## Themenabdeckung

Stand der Sammlung: **138 Kata-Projekte** im Repo (35 aus dieser Roadmap, 81 aus dem CCD
Coding Dojo, 22 Entwurfsmuster aus dem Refactoring.Guru-Katalog), jedes mit vollständiger
`README.md`.

| Bereich | Konkrete Themen | Als Kata vorhanden |
|---|---|---|
| Sprache | C# 13/14, Records, Pattern Matching, `Span<T>`, Nullable Reference Types | ja — `TestableLinesOfCode`, `ResultPattern`, `SpanCsvParser` + 14 Function-/9 Class-Katas |
| SQL/Datenbank | T-SQL, Ausführungspläne, Index-Design, Isolationslevel, Deadlocks, Massendaten | ja — `SqlPerformance`, `TransactionsAndIsolation`, `BulkAndPaging`, `PostgresJsonb`, `DapperSql` |
| Web/API | ASP.NET Core, Minimal APIs, REST-Design, FluentValidation | ja — `MinimalApi`, `ContainerAndCiCd`; REST-Entwurf zusätzlich in `UrlShortener`, `Taxi`, `PizzaOnline`, `Quizduell` |
| Persistenz | EF Core 9/10, Dapper, SQL Server, PostgreSQL, Redis, Migrations ohne Downtime | ja — `EfCore` (inkl. Expand/Contract, Testcontainers), `DapperSql`, `PostgresJsonb`, `RedisCaching`, `OutboxPattern`, `DomainModel` |
| Architektur | Clean Architecture, DDD/Bounded Contexts, CQRS, MediatR, Repository/UoW | **stark** — `Cqrs`, `DomainModel` + 22 CCD Architecture Katas |
| Async | `async`/`await`, `IAsyncEnumerable`, `CancellationToken`, Channels, Parallelität | ja — `RateLimitedFetcher`, `ChannelPipeline`, `SpanCsvParser` |
| Testing | xUnit/NUnit, TDD, Mocking (NSubstitute/Moq), Testcontainers, Legacy unter Test | ja auf dem Papier — jede der 138 READMEs fordert Tests zuerst; dazu `LegacyRescue`, `ArchitectureFitness` |
| Security | OAuth2/OIDC, JWT, Autorisierung, Secrets, OWASP, DSGVO/PII | ja — `ApiSecurity`, `DataPrivacy`, `MultiTenancy` (Datenisolation) |
| Messaging | RabbitMQ, Azure Service Bus, Kafka, Outbox-Pattern | ja — `OutboxPattern` (RabbitMQ), `KafkaStreaming`, `AzureServiceBus` |
| Cloud/DevOps | Docker, Kubernetes/AKS, Azure Container Apps, CI/CD (GH Actions / Azure DevOps) | ja — `ContainerAndCiCd` (Compose, Aspire, k8s, GH Actions), `FeatureFlags` |
| Betrieb/Diagnose | Resilienz, Hintergrundjobs, Dumps und Profiling, Runbooks | ja — `ResilienceAndChaos`, `BackgroundJobs`, `ProductionDiagnostics` |
| Integration | gRPC, SignalR/Echtzeit, WCF/CoreWCF im Bestand, Mandantenfähigkeit | ja — `GrpcServices`, `RealtimeSignalR`, `MultiTenancy`; WCF als Stack-Variante in 24 Katas |
| Desktop | WPF, MVVM, Data Binding | ja — als Stack-Variante in 32 Application-/Library-Katas |
| Frontend | Blazor **oder** Angular/React + TypeScript | ja — `BlazorDashboard`, `TypeScriptFrontend` (BFF, generierte Typen) |
| AI-Integration | Semantic Kernel, Azure OpenAI — LLM-Aufrufe als testbare Software | ja — `AiIntegration` (Tool Calling, SSE, RAG, Fake-LLM), `AiAssistedDevelopment` |
| Entwurfsmuster | Alle 22 GoF-Muster — erkennen, anwenden, abgrenzen, weglassen | ja — `14_DesignPatterns`, je eine Kata pro Muster mit Ausgangscode zum Erkennen |

**Kurzfassung:** Die *Themenabdeckung* ist das erklärte Ziel dieser Sammlung und erreicht:
alle vierzehn Themenbereiche haben mindestens eine Aufgabe, die letzten Lücken (Security,
Legacy, Hintergrundjobs, Produktionsdiagnostik, Resilienz, gRPC, Echtzeit,
Mandantenfähigkeit, Datenschutz, Feature Flags, Testqualität, KI-Assistenz) sind
geschlossen.

Kata 07_01 (`TestableLinesOfCode`) schaltet die Zeile *Testing* frei, die in jeder der 138
Aufgabenbeschreibungen als Abnahmekriterium steht.

Seit `14_DesignPatterns` kommt ein fünfzehnter Bereich hinzu: **Entwurfsmuster** — alle 22
GoF-Muster, jedes mit einem Ausgangscode, an dem das Muster erst *erkannt* werden muss.

## Nummerierung

Jede Kata trägt die ID `<Ordner>_<Position>` und heißt in ihrer `README.md` so — passend zur
Schachtelung in `Katas.slnx`. `10_02` ist also die zweite Kata im Ordner
`10_DatenbankKatas`. Ordnernummern sind eindeutig und aufsteigend; Querverweise zwischen
Katas nutzen dieselbe ID.

| Ordner | Inhalt | Anzahl |
|---|---|---|
| `01_FunctionKatas` | CCD Coding Dojo — Function Katas | 14 |
| `02_ClassKatas` | CCD Coding Dojo — Class Katas | 9 |
| `03_LibraryKatas` | CCD Coding Dojo — Library Katas | 5 |
| `04_ApplicationKatas` | CCD Coding Dojo — Application Katas | 29 |
| `05_ArchitectureKatas` | CCD Coding Dojo — Architecture Katas | 22 |
| `06_RefactoringKatas` | CCD Coding Dojo — Refactoring Katas | 2 |
| `07_Modernes_CSharpKatas` | Stufe 1: modernes C#, Testbarkeit, Legacy | 5 |
| `08_AsyncKatas` | Stufe 2: Async und Nebenläufigkeit | 2 |
| `09_APIUndArchitekturKatas` | Stufe 3: API, Architektur, Integration | 7 |
| `10_DatenbankKatas` | Stufe 3: SQL, Persistenz, Massendaten | 5 |
| `11_VerteilteSystemeKatas` | Stufe 4: Messaging, Betrieb, Diagnose | 10 |
| `12_SicherheitKatas` | Stufe 3/4: AuthN/AuthZ, Datenschutz | 2 |
| `13_DifferenzierungKatas` | Stufe 5: Frontend und KI | 4 |
| `14_DesignPatterns` | GoF-Entwurfsmuster erkennen und anwenden | 22 |

Weil die ID jetzt den Ordner abbildet, ist die *didaktische* Reihenfolge nicht mehr aus ihr
ablesbar — die steht deshalb unten als **Lernpfad**.

## Stack-Varianten

Die 81 CCD-Katas geben eine Fachlichkeit vor, aber keinen Technologiestack. Genau darin
liegt zusätzlicher Übungswert: **dieselbe Kata mehrfach lösen und nur die äußere Schicht
tauschen.** 56 dieser Katas haben dafür einen Abschnitt `## Stack-Varianten`.

| Gruppe | Katas | Varianten |
|---|---|---|
| Oberfläche — `04_ApplicationKatas` + die werkzeugartigen `03_`-Katas | 32 | Konsole (Ausgangsvariante) · **WPF** (MVVM, Data Binding, `INotifyDataErrorInfo`) · **Blazor** (Komponenten, `EditForm`, bUnit) |
| Dienst — `05_ArchitectureKatas` + `WindowsService`, `Benutzeranmeldung` | 24 | Transport: REST · **gRPC** · **WCF** (CoreWCF) — dazu WPF oder Blazor als Client |

Der Nachweis ist in beiden Fällen derselbe und wichtiger als die Implementierung: **die
Fachlogik bleibt beim Tausch unverändert.** Das Kernprojekt darf keine Referenz auf WPF
oder ASP.NET tragen; wer für die zweite Variante den Kern anfassen muss, hat die Trennung
nicht sauber gezogen und notiert, an welcher Stelle es gehakt hat.

**Warum WCF, obwohl es kein Zukunftsthema ist:** weil in deutschen Unternehmen sehr viel
WCF im Bestand läuft. Die interessante Fähigkeit ist nicht WCF selbst, sondern der Weg
davon weg — denselben Vertrag in WCF und gRPC ausdrücken und den Umstieg planen
(`FaultException` gegen `StatusCode`, Sessions gegen Streams, `netTcp` gegen HTTP/2). Auf
.NET 10 läuft das über **CoreWCF**.

## Die 35 eigenen Katas

Die 81 Katas in `01_`–`06_` stammen aus dem CCD Coding Dojo. Die folgenden 35 sind für diese
Roadmap geschrieben, sortiert nach ID.

| ID | Kata | Stufe | Warum |
|---|---|---|---|
| 07_01 | `TestableLinesOfCode` | 1 | Vom Konsolenskript zur unit-getesteten Bibliothek. Schaltet die Zeile *Testing* für die ganze Sammlung frei |
| 07_02 | `ResultPattern` | 1 | Erwartbare Fehler sind Rückgabewerte, nicht Exceptions. Der Typ muss den Missbrauch verhindern |
| 07_03 | `SpanCsvParser` | 1 | Allokationsbewusstsein: RFC-4180-Parser naiv und `Span`-basiert, verglichen mit BenchmarkDotNet |
| 07_04 | `LegacyRescue` | 1 | Code ändern, den man nicht geschrieben hat: Charakterisierungstests, Approval Tests, Seams, Strangler Fig mit Parallel Run. Der Alltagsfall in gewachsenem Code |
| 07_05 | `ArchitectureFitness` | 1 | Aus „die Architektur ist eingehalten" einen roten Build machen: NetArchTest, Analyzer, und Mutation Testing als ehrlichere Zahl als Coverage |
| 08_01 | `RateLimitedFetcher` | 2 | Parallelität begrenzen, Abbruch durchreichen, Zeit über `TimeProvider` testbar machen |
| 08_02 | `ChannelPipeline` | 2 | Bounded Queues und Backpressure — die Grundlage jeder Ingest-Pipeline |
| 09_01 | `MinimalApi` | 3 | Startpunkt der gemeinsamen Codebase: `TypedResults`, FluentValidation, ProblemDetails, vertikale Slices |
| 09_02 | `EfCore` | 3 | Persistenz jenseits von `DbSet`: Value Converter, Concurrency, Expand/Contract-Migration, N+1 nachweisen, Testcontainers |
| 09_03 | `Cqrs` | 3 | Das Muster erst selbst bauen (Dispatcher, Behavior-Kette), dann mit MediatR — und den Unterschied benennen |
| 09_04 | `DomainModel` | 3 | Invarianten im Aggregat statt im Validator. Domänentests ohne Mock und ohne Datenbank |
| 09_05 | `GrpcServices` | 3 | REST außen, gRPC innen: Contract-first, vier Streaming-Arten, Schema-Evolution, Deadlines — plus Messvergleich gegen REST inklusive der Nachteile |
| 09_06 | `RealtimeSignalR` | 3 | Echtzeit richtig: Reconnect als Normalfall, Zustandslücke nach Trennung schließen, Hub-Autorisierung, zwei Instanzen ohne und mit Backplane |
| 09_07 | `MultiTenancy` | 3 | Datenisolation strukturell statt durch Disziplin — inklusive der Löcher, an denen der Filter nicht greift: rohes SQL, Bulk, Cache-Keys, Hintergrundjobs |
| 10_01 | `DapperSql` | 3 | Der zweite Datenzugriffsweg neben EF Core. Lesepfade mit handgeschriebenem SQL, gemeinsame Transaktion mit EF Core |
| 10_02 | `SqlPerformance` | 3 | Ausführungspläne, Index-Design, Covering Index, SARGability, Parameter Sniffing, DMVs/Query Store — gemessen an 2 Mio. Zeilen, nicht an 100 |
| 10_03 | `TransactionsAndIsolation` | 3 | Jede Anomalie einmal selbst erzeugt: Dirty/Non-repeatable/Phantom, Write Skew unter Snapshot, Deadlock samt Graph und Sperrreihenfolge |
| 10_04 | `BulkAndPaging` | 3 | `SqlBulkCopy`, TVPs, Upsert — und Keyset- statt Offset-Pagination. Liefert zugleich die Datenmenge für Kata 10_02 |
| 10_05 | `PostgresJsonb` | 3 | Zweiter Provider: PostgreSQL steht inzwischen neben SQL Server. JSONB, GIN, `EXPLAIN ANALYZE`, dieselbe Testsuite gegen beide |
| 11_01 | `OutboxPattern` | 4 | Konsistenz über zwei Systeme ohne verteilte Transaktion. At-least-once und Idempotenz nachweisen |
| 11_02 | `Observability` | 4 | Serilog, OpenTelemetry über die Message-Grenze, eigene Metriken, Health Checks. Fehler im Trace finden, nicht im Debugger |
| 11_03 | `ContainerAndCiCd` | 4 | Aus Code ein Artefakt: Multi-Stage-Image, Compose, Aspire, GitHub Actions, k8s — Rolling Update mit 0 Fehlern |
| 11_04 | `KafkaStreaming` | 4 | Log statt Queue: Partitionen, Consumer Groups, Offsets, Rebalancing, Lag, Replay |
| 11_05 | `AzureServiceBus` | 4 | Der Broker im Azure-Umfeld: PeekLock, DLQ, Sessions, Scheduled Messages, Duplicate Detection |
| 11_06 | `RedisCaching` | 4 | Die optimierte Query gar nicht mehr stellen. Cache-Aside, Stampede, Invalidierung, `HybridCache`, Ausfall ohne Ausfall |
| 11_07 | `BackgroundJobs` | 4 | Der nächtliche Lauf: Cron und Zeitzonen, Sommerzeit, „nur einmal bei drei Instanzen", Wiederaufsetzbarkeit, Graceful Shutdown |
| 11_08 | `ProductionDiagnostics` | 4 | Fünf Fehler absichtlich bauen und **ohne Debugger** finden — Leck, Thread-Pool-Starvation, CPU-Hotspot, Deadlock, GC-Druck. Ergebnis ist ein Runbook |
| 11_09 | `ResilienceAndChaos` | 4 | Retry allein macht es schlimmer: Timeout-Budgets, Circuit Breaker, Bulkhead, Fallback, Jitter, Chaos-Injektion als Test |
| 11_10 | `FeatureFlags` | 4 | Deployment und Release trennen: vier Flag-Typen, stabile Zielgruppen-Zuordnung, Progressive Delivery mit vorher definiertem Abbruchkriterium, Flag-Hygiene |
| 12_01 | `ApiSecurity` | 3 | Die größte Lücke der Sammlung war diese: OIDC/PKCE, JWT-Validierung von Hand, policy- und ressourcenbasierte Autorisierung, Refresh-Rotation, fünf Angriffe je mit ausführendem Test |
| 12_02 | `DataPrivacy` | 4 | Auskunft und Löschung sind Architektur, nicht Jura: Inventar aller Fundorte, Log-Scrubbing, Anonymisieren vs. Löschen, Aufbewahrungsfristen |
| 13_01 | `BlazorDashboard` | 5 | Frontend vollständig in C#: Render Mode begründen, generierter API-Client, geteilte Validierungsregeln, bUnit |
| 13_02 | `AiIntegration` | 5 | LLM-Integration als normale Software: Tool Calling auf echte Domänen-Services, SSE-Streaming, RAG, Kostenmetriken, testbar ohne echtes Geld |
| 13_03 | `TypeScriptFrontend` | 5 | Die andere Hälfte von „Blazor **oder** Angular/React": generierte Typen aus OpenAPI, BFF-Pattern, Vitest/Playwright |
| 13_04 | `AiAssistedDevelopment` | 5 | Das Gegenstück zu Kata 13_02: nicht LLM-Features bauen, sondern mit Assistenz entwickeln und die Verantwortung behalten — Fehlerklassen im generierten Code erkennen |

## Die 22 Entwurfsmuster-Katas

Ordner `14_DesignPatterns`, Aufgabenstellungen nach dem
[Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog), Nummerierung in
Katalogreihenfolge (Erzeugung, Struktur, Verhalten).

Diese Katas trainieren bewusst **zwei** Fähigkeiten, und die erste ist die seltenere: das
Muster im vorliegenden Code *erkennen*. Jede README beginnt deshalb mit den Symptomen und
einem Ausgangscodeblock, der den typischen Schmerz zeigt — `switch`-Kaskade, gekreuzte
Vererbung, Konstruktor-Teleskop. Erst danach kommt der Umbau. Zwei Abschnitte halten den
Pattern-Kult in Schach: `## Abgrenzung` nennt die Muster, mit denen dieses regelmäßig
verwechselt wird, und `## Wann nicht` die Fälle, in denen es Überbau ist — oft mit der
C#-Sprachalternative (`Func<T>`, `record` mit `with`, `yield return`, Pattern Matching, DI).

| ID | Muster | Gruppe | Übungsschwerpunkt |
|---|---|---|---|
| 14_01 | `FactoryMethodPattern` | Erzeugung | Fabrikmethode — Ablauf festhalten, Erzeugung variieren |
| 14_02 | `AbstractFactoryPattern` | Erzeugung | Abstract Factory — Produktfamilien, die nicht gemischt werden dürfen |
| 14_03 | `BuilderPattern` | Erzeugung | Builder — Teleskopkonstruktor auflösen, Director, Pflichtfelder |
| 14_04 | `PrototypePattern` | Erzeugung | Prototype — flach gegen tief kopieren, Zyklen, abgeleitete Typen |
| 14_05 | `SingletonPattern` | Erzeugung | Singleton — threadsicher bauen und dann wieder **loswerden** (DI) |
| 14_06 | `AdapterPattern` | Struktur | Adapter — Fremd-API aus der Domäne heraushalten |
| 14_07 | `BridgePattern` | Struktur | Bridge — kombinatorische Klassenexplosion zweier Dimensionen |
| 14_08 | `CompositePattern` | Struktur | Composite — Baum ohne Typprüfung, Transparenz gegen Sicherheit |
| 14_09 | `DecoratorPattern` | Struktur | Decorator — Querschnittsbelange stapeln, Reihenfolge beobachtbar |
| 14_10 | `FacadePattern` | Struktur | Facade — sieben Subsystemaufrufe auf einen fachlichen reduzieren |
| 14_11 | `FlyweightPattern` | Struktur | Flyweight — Optimierung nur mit Messung vorher und nachher |
| 14_12 | `ProxyPattern` | Struktur | Proxy — Virtual, Protection und Logging, Stapelreihenfolge |
| 14_13 | `ChainOfResponsibilityPattern` | Verhalten | Zuständigkeitskette — `if`/`else if` auflösen, Reihenfolge als Fachregel |
| 14_14 | `CommandPattern` | Verhalten | Command — Undo/Redo, Makro-Command, Abgrenzung zum CQRS-Command |
| 14_15 | `IteratorPattern` | Verhalten | Iterator — von Hand, dann `yield return`, verzögerte Auswertung |
| 14_16 | `MediatorPattern` | Verhalten | Mediator — n-zu-n-Verdrahtung auflösen, Abgrenzung zu MediatR |
| 14_17 | `MementoPattern` | Verhalten | Memento — Zustand sichern, ohne die Kapselung aufzugeben |
| 14_18 | `ObserverPattern` | Verhalten | Observer — von Hand, `event`, `IObservable<T>`, und das Speicherleck |
| 14_19 | `StatePattern` | Verhalten | State — Lebenszyklus als Typen, vollständige Übergangstabelle als Test |
| 14_20 | `StrategyPattern` | Verhalten | Strategy — Verfahren austauschen, Interface gegen `Func<>` |
| 14_21 | `TemplateMethodPattern` | Verhalten | Template Method — fester Ablauf, Hooks, Gegenprobe mit Strategy |
| 14_22 | `VisitorPattern` | Verhalten | Visitor — Expression Problem: neue Operationen billig, neue Typen teuer |

## Lernpfad

Die IDs folgen der Projektmappe, nicht der Didaktik. Empfohlene Reihenfolge:

```
Stufe 1   07_01 → 07_02 → 07_03
Stufe 2   08_01 → 08_02
Stufe 3   09_01 → 09_02 → 09_03 → 09_04
Stufe 4   11_01 → 11_02 → 11_03
Stufe 5   13_01 → 13_02
Persistenz vertiefen   10_01 → 10_02 → 10_03 → 10_04 → 10_05
Messaging vertiefen    11_04 → 11_05 → 11_06
Frontend vergleichen   13_03
Muster, quer zu allem   14_20 → 14_19 → 14_09 → 14_01 → 14_13 → 14_18 → dann der Rest von 14_

Eigenständig, jederzeit   12_01 · 07_04 · 11_07 · 11_08 · 11_09 · 09_05 · 09_06 · 07_05 · 09_07 · 12_02 · 11_10 · 13_04
```

**Zwei Nutzungsarten, bewusst nebeneinander.** Die Kette `09_01 → 09_02 → 09_03 → 09_04 →
11_01 → 11_02 → 11_03 → 13_01 → 13_02` und ihre Vertiefungen bauen aufeinander auf und
münden in **ein** durchgängiges System — lehrreicher als isolierte
Übungen, weil erst das Zusammenspiel die Entwurfsentscheidungen sichtbar macht. Alle
übrigen Katas stehen für sich und sind als Einzelübung vollständig. Welche
Variante eine Kata verlangt, steht in ihrem Abschnitt *Voraussetzung*: „muss zuvor erledigt
sein" ist eine harte Abhängigkeit, „empfohlen, nicht erforderlich" nicht. Die zwölf
zuletzt ergänzten Katas haben zusätzlich einen **Minimalpfad** — drei bis vier Punkte, die
den Skill freischalten, getrennt vom Ausbau.