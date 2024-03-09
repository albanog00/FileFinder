using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ValidationResult = Spectre.Console.ValidationResult;

namespace FileFinder.Cli;

public class FileFinderCommand : Command<FileFinderCommand.Settings>
{
    public Settings _settings { get; private set; } = new Settings();

    public class Settings : CommandSettings
    {
        [Description("File name to search.")]
        [CommandArgument(0, "[FILE_NAME]")]
        [Required]
        public string Name { get; init; } = string.Empty;

        [Description("Path where to search file. Default current directory.")]
        [CommandOption("-p|--path <SEARCH_PATH>")]
        public string SearchPath { get; init; } = Directory.GetCurrentDirectory();

        [Description("Display errors when encountered if true.")]
        [CommandOption("--show-errors")]
        public bool ShowErrors { get; init; } = false;

        [Description("Find exact file name provided.")]
        [CommandOption("-e|--exact")]
        public bool Exact { get; init; } = false;

        [Description("Include directories to the search results.")]
        [CommandOption("--include-dir")]
        public bool IncludeDirectories { get; init; } = false;
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (string.IsNullOrEmpty(settings.Name))
            return ValidationResult.Error("Argument 0 `Name` cannot be empty.");

        if (!Directory.Exists(settings.SearchPath))
            return ValidationResult.Error($"Provided path `{settings.SearchPath}` doesn't exists.");

        return base.Validate(context, settings);
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        _settings = settings;
        return ExecuteAsync().Result;
    }

    public async Task<int> ExecuteAsync()
    {
        AnsiConsole.MarkupLineInterpolated($"Searching[red]{(_settings.Exact ? " exactly" : "")}[/] [dodgerblue2]`{_settings.Name}`[/] in {Emoji.Known.OpenFileFolder} [yellow]`{_settings.SearchPath}`[/] and subdirectories.");
        AnsiConsole.MarkupLine($"Directories are{(_settings.IncludeDirectories ? "" : " [red]not[/]")} included.");
        AnsiConsole.MarkupLine($"Display errors is {(_settings.ShowErrors ? "[green]enabled" : "[red]disabled")}[/].\n");

        var directoryInfo = new DirectoryInfo(_settings.SearchPath);
        await FindAsync(directoryInfo);

        return 0;
    }

    private async Task FindAsync(DirectoryInfo directoryInfo)
    {
        var queue = new ConcurrentQueue<DirectoryInfo>();
        queue.Enqueue(directoryInfo);

        while (!queue.IsEmpty)
        {
            await Task.Run(async () =>
            {
                var dirProcessTasks = new List<Task>();
                while (!queue.IsEmpty)
                {
                    if (queue.TryDequeue(out var currentDirectory))
                    {
                        dirProcessTasks.Add(Task.Run(() => ProcessDirectoryAsync(currentDirectory, queue)));
                    }
                }
                await Task.WhenAll(dirProcessTasks);
            });
        }
    }

    private void ProcessDirectoryAsync(DirectoryInfo directoryInfo, ConcurrentQueue<DirectoryInfo> queue)
    {
        try
        {
            if (_settings.IncludeDirectories &&
                ((_settings.Exact && directoryInfo.FullName == _settings.Name) ||
                (!_settings.Exact && directoryInfo.FullName.Contains(_settings.Name))))
            {
                AnsiConsole.MarkupLine($"[bold][green]Match![/] {Emoji.Known.OpenFileFolder} [yellow]{directoryInfo.FullName.Replace(_settings.Name, $"[red]{_settings.Name}[/]")}[/][/]");
            }

            foreach (var file in directoryInfo.EnumerateFiles()
                .Where(x => (_settings.Exact && x.FullName == _settings.Name) ||
                    (!_settings.Exact && x.FullName.Contains(_settings.Name))))
            {
                {
                    AnsiConsole.MarkupLine($"[bold][green]Match![/] {Emoji.Known.PageFacingUp} [dodgerblue2]{file.FullName.Replace(_settings.Name, $"[red]{_settings.Name}[/]")}[/][/]");
                }
            }

            foreach (var subdirectory in directoryInfo.EnumerateDirectories()
                .Where(x => x.LinkTarget == null))
            {
                queue.Enqueue(subdirectory);
            }
        }
        catch (Exception ex)
        {
            if (_settings.ShowErrors)
            {
                AnsiConsole.MarkupLineInterpolated($"[bold][red]Error:[/] {Emoji.Known.StopSign} {ex.Message}[/]");
            }
        }
    }
}
