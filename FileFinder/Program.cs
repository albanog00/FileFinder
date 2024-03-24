using FileFinder.Cli;
using FileFinder.Core;
using Spectre.Console.Cli;

//var fileFinder = new CommandApp<FileFinderCommand>();
//fileFinder.Run(args);

var fileExplorer = new FileExplorer("File", null, @"..\");
StreamWriter writer = new(Console.OpenStandardOutput())
{
    AutoFlush = true
};

var response = await fileExplorer.FindAsync();
foreach (var filePath in response.MatchedFilePaths)
{
    writer.WriteLine(filePath);
}

