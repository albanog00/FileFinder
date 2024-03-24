using FileFinder.Core;
using Spectre.Console.Cli;

namespace FileFinder.Cli;

public class FileFinderCommand : Command<FileFinderCommandSettings>
{
    public override int Execute(
        CommandContext context,
        FileFinderCommandSettings settings) => ExecuteAsync(settings).Result;

    public async Task<int> ExecuteAsync(FileFinderCommandSettings settings)
    {
        var fileExplorer = new FileExplorer(
            settings.Name,
            settings.Extension,
            settings.SearchPath,
            settings.ShowErrors,
            settings.Exact);
        await fileExplorer.FindAsync();

        return 0;
    }
}
