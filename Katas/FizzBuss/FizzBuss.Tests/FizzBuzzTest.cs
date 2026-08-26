using Xunit;

namespace FizzBuss.Tests;

public class FizzBuzzTest
{
    
    private readonly ITestOutputHelper _testOutputHelper;
 
    public FizzBuzzTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void Test1()
    {
        Program.FizzBuzz();
    }
}