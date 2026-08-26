using Xunit;

namespace HappyNumbers.Tests;

public class HappyNumbersTest
{
    private readonly ITestOutputHelper _testOutputHelper;
 
    public HappyNumbersTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void Test1()
    {
        for (int i = 10; i <= 200; i++)
        {
            _testOutputHelper.WriteLine($"{i} is Happy: {Program.IsHappyNumber(i)}");
        }
    }
}