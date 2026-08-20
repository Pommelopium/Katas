
namespace BridgePattern;

/// <summary>
///     Kata 14_07 — Bruecke (Bridge), Strukturmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode mit der gekreuzten Vererbungshierarchie hinschreiben (9 Klassen)
    // [ ] die zwei unabhaengigen Dimensionen benennen: Nachrichtenart und Versandkanal
    // [ ] Implementierung herausziehen: IChannel mit Send(...)
    // [ ] Abstraktion behalten: Notification haelt den IChannel als Feld — die Bruecke
    // [ ] verfeinerte Abstraktionen: Reminder-, WeeklyReport-, StreakWarningNotification
    // [ ] konkrete Implementierungen: Email-, Sms-, SlackChannel
    // [ ] je eine neue Variante pro Dimension ergaenzen — 2 neue Klassen statt 7
    // [ ] Tests: Kreuzprodukt aller Kombinationen parametrisiert, Klassenzahl gezaehlt
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_07");
    }
}
