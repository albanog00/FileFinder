using Xunit.Abstractions;

namespace FileFinder.Core.Test;

public class FileExplorerTest
{
    private readonly ITestOutputHelper _output;

    public FileExplorerTest(ITestOutputHelper output)
    {
        _output = output;
    }

    //[Fact]
    public async Task ShouldReturnAllPathsThatContainsFileNameAndExtensionInProjectPathAsync()
    {
        string[] expect = [
            "FileFinder\\bin\\Debug\\net8.0\\FileFinder.exe",
            "FileFinder\\bin\\Release\\net8.0\\FileFinder.exe",
            "FileFinder\\obj\\Debug\\net8.0\\apphost.exe",
            "FileFinder\\obj\\Release\\net8.0\\apphost.exe",
            "FileFinder.Core.Test\\bin\\Debug\\net8.0\\testhost.exe",
            "FileFinder.Core.Test\\bin\\Release\\net8.0\\testhost.exe"
        ];

        string projectPath = Path.Combine(
            Directory.GetCurrentDirectory(), @"..\..\..\..\");
        FileExplorer fileExplorer = new("File", "exe", projectPath);

        var response = await fileExplorer.FindAsync();
        string[] actual = [.. response.MatchedFilePaths];

        Assert.Equal(expect.Length, actual.Length);
        Assert.All(expect, x => Assert.Contains(x, actual));
    }
}
