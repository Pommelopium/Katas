
namespace ContainerAndCiCd;

/// <summary>
///     Kata 11_03 — Containerisierung und CI/CD (Stufe 4: Verteilte Systeme und Betrieb)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Multi-Stage Dockerfile, Non-Root, Image unter 120 MB
    // [ ] docker-compose: API + SQL Server + RabbitMQ + Jaeger, mit Healthchecks
    // [ ] .NET Aspire als Alternative, Vergleich schriftlich
    // [ ] GitHub Actions: Build -> Tests -> Coverage-Gate -> Image -> GHCR (Tag = Git-SHA)
    // [ ] k8s-Manifeste: Deployment, Service, ConfigMap, Secret, Probes, Resource Limits
    // [ ] Rolling Update ohne fehlgeschlagenen Request, per k6 nachgewiesen
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 11_03");
    }
}
