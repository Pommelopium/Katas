using Xunit;

namespace FilePathHelper.Tests;

public class FilePathHelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;
 
    public FilePathHelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void Test1()
    {
        //IEnumerable<string> result = Program.(input).ToList();
        //Assert.NotEmpty(result);
        //foreach (string item in result)
        //{
        //    Assert.NotNull(item);
        //    _testOutputHelper.WriteLine(item);
        //}
    }
}