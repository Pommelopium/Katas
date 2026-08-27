using Xunit;

namespace Tannenbaum.Tests;

public class TannebaumTest
{
    private readonly ITestOutputHelper _testOutputHelper;
 
    public TannebaumTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test1()
    {
        //Der Testoutput hat die ersten leerzeichen weg ge-trimmed, daher noch ein text vorne dran, dass das nicht passiert.
        _testOutputHelper.WriteLine("Gurke:\n\r"+string.Join("\n\r", Program.Tannenbaum(5)));
    }
}