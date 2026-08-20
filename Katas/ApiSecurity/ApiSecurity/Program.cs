
namespace ApiSecurity;

/// <summary>
///     Kata 12_01 — Authentifizierung und Autorisierung (Stufe 3: API, Persistenz, Architektur)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] JWT von Hand zerlegt und Signatur gegen JWKS geprueft
    // [ ] Authorization Code Flow mit PKCE gegen Keycloak
    // [ ] Jede Validierungspruefung einzeln abgeschaltet, Angriff je Test gezeigt
    // [ ] Autorisierung rollen-, policy- und ressourcenbasiert am selben Endpunkt
    // [ ] Refresh-Token-Rotation mit Wiederverwendungserkennung
    // [ ] Client Credentials Flow fuer einen Hintergrunddienst
    // [ ] Secrets ueber user-secrets/Umgebung, Schluesselrotation ohne Logout
    // [ ] IDOR, alg:none, fehlende Audience, Mass Assignment, Login-Rate-Limit je Test
    // [ ] Passwort-Hashing nach Stand der Technik, Dauer gemessen
    // [ ] Security Headers, enges CORS, keine Tokens in Logs
    // [ ] Tabelle: Angriff | Test | Gegenmassnahme | Begruendung
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 12_01");
        await Task.CompletedTask;
    }
}
