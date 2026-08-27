using Xunit;

namespace Rot13.Tests;

public class Rot13Test
{
    [Theory]
    [InlineData("Hello World!", "URYYB JBEYQ!")]
    [InlineData("0123456789", "3456789012")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "NOPQRSTUVWXYZABCDEFGHIJKLM")]
    [InlineData("ß", "FF")]
    [InlineData("Ä", "NR")]
    [InlineData("Ü", "HR")]
    [InlineData("Ö", "BR")]
    public void TestRot13(string input, string expected)
    {
        Assert.Equal(expected, Program.RotVerschluesseln(input));
    }
}