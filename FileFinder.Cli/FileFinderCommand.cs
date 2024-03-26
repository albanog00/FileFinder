using FileFinder.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FileFinder.Cli;

public class FileFinderCommand : Command<FileFinderCommandSettings>
{
    public override int Execute(
        CommandContext context,
        FileFinderCommandSettings settings) => ExecuteAsync(settings).Result;

    private async Task<int> ExecuteAsync(FileFinderCommandSettings settings)
    {
        var console = AnsiConsole.Create(new()
        {
            Out = new AnsiConsoleOutput(
                new StreamWriter(Console.OpenStandardOutput()))
        });

        var fileExplorer = new FileExplorer(
            settings.Name,
            settings.Extension,
            settings.SearchPath,
            settings.ShowErrors,
            settings.Exact,
            null);

        var filePaths = await fileExplorer.FindAsync();

        var searchPrompt =
            new SelectionPrompt<string>()
            {
                PageSize = 30,
                SearchEnabled = true,
                Mode = SelectionMode.Independent
            };
        console.WriteLine();

        searchPrompt.AddChoices(filePaths);
        string selected = searchPrompt.Show(console);

        console.WriteLine(selected);
        return 0;
    }
}
