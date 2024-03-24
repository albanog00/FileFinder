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
        //AnsiConsole.MarkupLine(
        //   $"""
        //   {$"Searching [dodgerblue2]`{settings.Name}*[red]{settings.Extension ?? ""}[/]`" +
        //    $"[/] in {Emoji.Known.OpenFileFolder} [yellow]`{settings.SearchPath}`[/] and subdirectories"}
        //   Directories are{(settings.IncludeDirectories ? "" : " [red]not[/]")} included.
        //   Display errors is {(settings.ShowErrors ? "[green]enabled" : "[red]disabled")}[/].
        //   """);

        var fileExplorer = new FileExplorer(
            settings.Name,
            settings.Extension,
            settings.SearchPath);

        StringBuilder builder = new();
        StreamWriter writer = new(Console.OpenStandardOutput())
        {
            AutoFlush = true
        };

        var response = await fileExplorer.FindAsync();
        if (settings.ShowErrors)
        {
            foreach (var error in response.Errors)
            {
                if (builder.Length > 4096)
                {
                    writer.Write(builder);
                    builder.Clear();
                }
                builder.AppendLine(error);
            }
            builder.Append('\n');
        }

        foreach (var filePath in response.MatchedFilePaths)
        {
            if (builder.Length > 4096)
            {
                writer.Write(builder);
                builder.Clear();
            }
            builder.AppendLine(filePath);
        }

        writer.Write(builder);
        return 0;
    }
}
