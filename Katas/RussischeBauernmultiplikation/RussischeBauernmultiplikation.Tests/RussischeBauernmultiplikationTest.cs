using Xunit;

namespace RussischeBauernmultiplikation.Tests;

public class RussischeBauernmultiplikationTest
{

    [Theory]
    [InlineData(0,1)]
    [InlineData(1,0)]
    [InlineData(1,1)]
    [InlineData(1,2)]
    [InlineData(2,2)]
    [InlineData(1,3)]
    [InlineData(2,3)]
    [InlineData(3,3)]
    [InlineData(4,4)]
    [InlineData(5,5)]
    [InlineData(75,434)]
    [InlineData(47,42)]
    public static void TestRusMul(int a, int b)
    {
        Assert.Equal(a*b, Program.Mul(a,b));
    }
}
