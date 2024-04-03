// using Xunit.Abstractions;

namespace FileFinder.Core.Test;

public class FileExplorerTest
{
    //[Fact]
    //public async Task ShouldReturnAllPathsThatContainsFileNameAndExtensionInProjectPathAsync()
    //{
    //    string[] expect = [
    //        "FileFinder\\bin\\Debug\\net8.0\\FileFinder.exe",
    //        "FileFinder\\bin\\Release\\net8.0\\FileFinder.exe",
    //        "FileFinder\\obj\\Debug\\net8.0\\apphost.exe",
    //        "FileFinder\\obj\\Release\\net8.0\\apphost.exe",
    //        "FileFinder.Core.Test\\bin\\Debug\\net8.0\\testhost.exe",
    //        "FileFinder.Core.Test\\bin\\Release\\net8.0\\testhost.exe"
    //    ];

    //    string projectPath = Path.Combine(
    //        Directory.GetCurrentDirectory(), @"..\..\..\..\");
    //    FileExplorer fileExplorer = new("File", "exe", projectPath);

    //    await fileExplorer.FindAsync();
    //    string[] actual = [];
    //    //[.. response.MatchedFilePaths];

    //    Assert.Equal(expect.Length, actual.Length);
    //    Assert.All(expect, x => Assert.Contains(x, actual));
    //}
}
