
namespace TypeScriptFrontend;

/// <summary>
///     Kata 13_03 — TypeScript-Frontend mit BFF (Stufe 5: Differenzierung)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
///     Der .NET-Teil ist der BFF-Host; die Oberflaeche liegt unter frontend/.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] tsconfig mit strict und noUncheckedIndexedAccess, kein any im Projekt
    // [ ] Typen aus dem OpenAPI-Dokument generiert; DTO-Aenderung bricht den Build
    // [ ] Laufzeitvalidierung an der Grenze statt Cast
    // [ ] Datenzugriff mit Cache, Dedup und Invalidierung
    // [ ] Formular mit denselben Validierungsregeln wie das Backend
    // [ ] ProblemDetails typisiert ausgewertet, AbortController bei Navigation
    // [ ] BFF: Token im HttpOnly-Cookie, CSRF-Schutz begruendet
    // [ ] npm-Build als MSBuild-Target, ein Docker-Image
    // [ ] Vitest-Komponententests und ein Playwright-E2E-Test
    // [ ] Eine Seite: Blazor vs. TypeScript-SPA
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 13_03");
        await Task.CompletedTask;
    }
}
