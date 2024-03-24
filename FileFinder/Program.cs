using FileFinder.Cli;
using FileFinder.Core;
using Spectre.Console.Cli;

#if !DEBUG
var fileFinder = new CommandApp<FileFinderCommand>();
fileFinder.Run(args);
#else
var fileExplorer = new FileExplorer(null, null, @"..\", true);
await fileExplorer.FindAsync();
#endif
