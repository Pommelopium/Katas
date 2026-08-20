
namespace BuilderPattern;

/// <summary>
///     Kata 14_03 — Erbauer (Builder), Erzeugungsmuster aus dem Refactoring.Guru-Katalog
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangszustand nachbauen: Teleskopkonstruktor mit 8 Parametern, Geruch benennen
    // [ ] IWochenberichtBuilder mit Reset / Kopf / Abschnitt / Tabelle / Fussnote
    // [ ] MarkdownBuilder und KonsolenBuilder, je eigenes Build()-Ergebnis
    // [ ] Build() validiert: Pflichtfelder fehlen -> Fehler beim Build, nicht spaeter
    // [ ] WochenberichtDirector: ein Bauablauf, zwei Builder, zwei Repraesentationen
    // [ ] dritte Repraesentation (CSV) ergaenzen, ohne Director und Produkte zu aendern
    // [ ] Abgrenzung schriftlich: Builder vs. Abstract Factory vs. Fluent-API vs. record with
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_03");
    }
}
