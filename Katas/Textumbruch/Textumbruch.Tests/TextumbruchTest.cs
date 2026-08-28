using Xunit;

namespace Textumbruch.Tests;

public class TextumbruchTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TextumbruchTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Theory]
    [InlineData("Hello, World!", 1)]
    [InlineData("Hello, World!", 2)]
    [InlineData("Hello, World!", 3)]
    [InlineData("Hello, World!", 4)]
    [InlineData("Hello, World!", 5)]
    [InlineData("Hello, World!", 6)]
    [InlineData("Hello, World!", 7)]
    [InlineData("Superkalifragelistigexpialigetisch", 7)]
    public void TestTextumbruch(string text, int lineLength)
    {
        foreach (string result in Program.Textumbruch(text, lineLength, false))
        {
            _testOutputHelper.WriteLine(result);
            Assert.True(lineLength >= result.Length);
        }
    }
    
    [Theory]
    [InlineData(9)]
    [InlineData(14)]
    [InlineData(28)]
    [InlineData(40)]
    public void TestTextumbruch2(int lineLength)
    {
        foreach (string result in Program.Textumbruch(_advent, lineLength, true))
        {
            _testOutputHelper.WriteLine(result);
            Assert.True(lineLength == result.Length, result);
        }
    }

    private string _advent = @"Advent Von Loriot
Es blaut die Nacht. Die Sternlein blinken.
Schneeflöcklein leise niedersinken.
Auf Edeltännleins grünem Wipfel
häuft sich ein kleiner weißer Zipfel.

Und dort, vom Fenster her durchbricht
den dunklen Tann' ein warmes Licht.
Im Forsthaus kniet bei Kerzenschimmer
die Försterin im Herrenzimmer.

In dieser wunderschönen Nacht
hat sie den Förster umgebracht.
Er war ihr bei der Heimespflege
seit langer Zeit schon sehr im Wege.

So kam sie mit sich überein:
Am Nicklausabend muß es sein.
Und als das Rehlein ging zur Ruh',
das Häslein tat die Augen zu,

Erlegte sie - direkt von vor'n
- den Gatten über Kimm' und Korn.
Vom Knall geweckt rümpft nur der Hase
zwei-, drei-, viermal die Schnuppernase.

Und ruhet weiter süß im Dunkeln,
Derweil die Sternlein traulich funkeln.
Und in der guten Stube drinnen,
da läuft des Försters Blut von hinnen.

Nun muß die Försterin sich eilen,
den Gatten sauber zu zerteilen.
Schnell hat sie ihn bis auf die Knochen
nach Waidmanns Sitte aufgebrochen.

Voll Sorgfalt legt sie Glied auf Glied
- was der Gemahl bisher vermied -
Behält ein Teil Filet zurück,
als festtägliches Bratenstück.

Und packt zum Schluß - es geht auf vier -
die Reste in Geschenkpapier.
Da dröhnt's von fern wie Silberschellen.
Im Dorfe hört man Hunde bellen.

Wer ist's, der in so tiefer Nacht
im Schnee noch seine Runde macht?
Knecht Ruprecht kommt mit goldenem Schlitten
auf einem Hirsch herangeritten!

»Heh, gute Frau, habt ihr noch Sachen,
die armen Menschen Freude machen?«
Des Försters Haus ist tief verschneit,
doch seine Frau steht schon bereit:

»Die sechs Pakete, heil'ger Mann,
's ist alles, was ich geben kann!«
Die Silberschellen klingen leise.
Knecht Ruprecht macht sich auf die Reise.

Im Försterhaus die Kerze brennt.
Ein Sternlein blinkt: Es ist Advent.";
}