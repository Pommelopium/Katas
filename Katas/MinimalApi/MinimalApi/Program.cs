
namespace MinimalApi;

/// <summary>
///     Kata 09_01 — Minimal API mit vertikalen Slices (Stufe 3: API, Persistenz, Architektur)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
///     HINWEIS: Ab hier wird aus dem Konsolenprojekt eine Web-Anwendung.
///     Sdk in der .csproj auf "Microsoft.NET.Sdk.Web" umstellen, sobald du startest.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Minimal APIs mit MapGroup + TypedResults
    // [ ] FluentValidation als Endpoint-Filter
    // [ ] Fehler als ProblemDetails (RFC 9457)
    // [ ] Ordner nach Feature, nicht nach Technik
    // [ ] IOptions<T> mit ValidateOnStart
    // [ ] OpenAPI + Versionierung /api/v1
    // [ ] Integrationstests mit WebApplicationFactory<Program>
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 09_01");
    }
}
