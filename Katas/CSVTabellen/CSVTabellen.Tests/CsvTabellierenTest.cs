using Xunit;

namespace CSVTabellen.Tests;

public class CsvTabellierenTest
{
    
    private readonly ITestOutputHelper _testOutputHelper;
 
    public CsvTabellierenTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void Test1()
    {
        List<string> input = new List<string>
        {
            "Name;Strasse;Ort;Alter;Gurke",
            "Peter Pan;Am Hang 5;12345 Einsam;42",
            "Maria Schmitz;Kölner Straße 45;50123 Köln;43;nein",
            "Paul Meier;Münchener Weg 1;87654 München;65;manchmal",
        };

        IEnumerable<string> result = Program.Tabellieren(input).ToList();
        Assert.NotEmpty(result);
        foreach (string item in result)
        {
            Assert.NotNull(item);
            _testOutputHelper.WriteLine(item);
        }
    }
}