using Xunit;

namespace FilePathHelper.Tests;

public class FilePathHelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;
 
    public FilePathHelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Theory]
    [InlineData("~/Downloads/mountains.jpg")] //` -> `/Users/brucew/Downloads/mountains.jpg`
    [InlineData("./bin/debug/samples/config.json")] //` -> `/Users/brucew/Projects/ETF/bin/debug/samples/config.json`
    [InlineData("/Users/brucew/Projects/ETF/bin/../program.cs")] // -> `/Users/brucew/Projects/ETF/program.cs`
    public void Test1(string path)
    {
        _testOutputHelper.WriteLine(Program.GetAbsolutFilePath(path));
    }
}