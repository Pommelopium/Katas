using Xunit;

namespace LinesOfCode.Tests;

public class LinesOfCodeTests
{

    private readonly ITestOutputHelper _testOutputHelper;

    public LinesOfCodeTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test1()
    {
        string sourceCode = File.ReadAllText("../../../../../RomanNumerals/RomanNumerals/Program.cs");
        (int, int, int) result = Program.LinesOfCode(sourceCode);
        _testOutputHelper.WriteLine($"Lines of Code: {result.Item1} Lines of Comments: {result.Item2} Lines of WhiteSpace: {result.Item3}");
        Assert.Equal(133, result.Item1);
        Assert.Equal(13, result.Item2);
        Assert.Equal(18, result.Item3);
    }
}