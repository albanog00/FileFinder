using FileFinder.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FileFinder.Cli;

public class FileFinderCommand : Command<FileFinderCommandSettings>
{
    public override int Execute(
        CommandContext context,
        FileFinderCommandSettings settings) => ExecuteAsync(settings).Result;

    // TODO: Interactive search 
    private async Task<int> ExecuteAsync(FileFinderCommandSettings settings)
    {
        AnsiConsole.Console = AnsiConsole.Create(new()
        {
            Out = new AnsiConsoleOutput(
                new StreamWriter(Console.OpenStandardOutput()))
        });
        AnsiConsole.WriteLine("Searching...");

        var fileExplorer = new FileExplorer(
            settings.Name,
            settings.Extension,
            settings.SearchPath,
            settings.ShowErrors,
            settings.Exact);

        var filePaths = await fileExplorer.FindAsync();

        var searchPrompt =
            new SelectionPrompt<string>()
            {
                PageSize = 30,
                SearchEnabled = true,
                Mode = SelectionMode.Independent
            };
        searchPrompt.AddChoices(filePaths);
        AnsiConsole.WriteLine();
        
        string selected = searchPrompt.Show(AnsiConsole.Console);
        AnsiConsole.Clear();
        AnsiConsole.WriteLine(selected);

        return 0;
    }
}
