using FileFinder.Cli;
using Spectre.Console.Cli;

var fileFinder = new CommandApp<FileFinderCommand>();
fileFinder.Run(args);

