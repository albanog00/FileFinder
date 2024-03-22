using FileFinder.Core;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Text;

namespace FileFinder.Cli;

public class FileFinderCommand : Command<FileFinderCommandSettings>
{
    public override int Execute(
        CommandContext context,
        FileFinderCommandSettings settings) => ExecuteAsync(settings).Result;

    public async Task<int> ExecuteAsync(FileFinderCommandSettings settings)
    {
        AnsiConsole.MarkupLine(
           $"""
           {$"Searching [dodgerblue2]`{settings.Name}*[red]{settings.Extension ?? ""}[/]`" +
            $"[/] in {Emoji.Known.OpenFileFolder} [yellow]`{settings.SearchPath}`[/] and subdirectories"}
           Directories are{(settings.IncludeDirectories ? "" : " [red]not[/]")} included.
           Display errors is {(settings.ShowErrors ? "[green]enabled" : "[red]disabled")}[/].
           """);

        var directoryInfo = new DirectoryInfo(settings.SearchPath);
        var fileExplorer = new FileExplorer(
            settings.Name,
            settings.Extension,
            settings.IncludeDirectories);

        var response = await fileExplorer.FindAsync(directoryInfo);
        StringBuilder builder = new();

        if (settings.ShowErrors)
        {
            builder.Append("\n--- [bold][red]ERRORS[/][/] ---\n");
            foreach (var line in response.Errors)
            {
                builder.AppendLine(
                    $":stop_sign: [bold][red]Error[/]: {Markup.Escape(line)}[/]");
            }
            builder.Append("\n--- [bold][green]MATCH[/][/] ---\n");
        }

        foreach (var line in response.MatchedFilePaths)
        {
            builder.AppendFormat("\n{0}", Markup.Escape(line));
        }
        builder.Append('\n');
        AnsiConsole.Markup(builder.ToString());

        return 0;
    }
}
