using Xunit;

namespace RomanNumerals.Tests;

public class RomanNumeralsTest
{
    private readonly ITestOutputHelper _testOutputHelper;
 
    public RomanNumeralsTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void Test1()
    {
        _testOutputHelper.WriteLine("RomanNumerals: ");
        _testOutputHelper.WriteLine($"I :{Program.Parse("I")}");
        _testOutputHelper.WriteLine($"II :{Program.Parse("II")}");
        _testOutputHelper.WriteLine($"IV :{Program.Parse("IV")}");
        _testOutputHelper.WriteLine($"V :{Program.Parse("V")}");
        _testOutputHelper.WriteLine($"IX :{Program.Parse("IX")}");
        _testOutputHelper.WriteLine($"XLII :{Program.Parse("XLII")}");
        _testOutputHelper.WriteLine($"XCIX :{Program.Parse("XCIX")}");
        _testOutputHelper.WriteLine($"MMXIII :{Program.Parse("MMXIII")}");
        _testOutputHelper.WriteLine($"MMIXIII :{Program.Parse("MMIXIII")}");
        _testOutputHelper.WriteLine($"MMIIXIII :{Program.Parse("MMIIXIII")}");
        _testOutputHelper.WriteLine($"IM :{Program.Parse("IM")}");
    }
}