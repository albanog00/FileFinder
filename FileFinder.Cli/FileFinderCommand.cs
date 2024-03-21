using FileFinder.Core;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text;
using ValidationResult = Spectre.Console.ValidationResult;

namespace FileFinder.Cli;

public class FileFinderCommand : Command<FileFinderCommand.Settings>
{
    public Settings _settings { get; private set; } = new Settings();

    public class Settings : CommandSettings
    {
        internal bool? includeDirectories;

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
        AnsiConsole.MarkupLine(
            $"""
            {$"Searching[red]{(_settings.Exact ? " exactly" : "")}[/] " +
                $"[dodgerblue2]`{_settings.Name}`[/] in {Emoji.Known.OpenFileFolder} " +
                $"[yellow]`{_settings.SearchPath}`[/] and subdirectories"}
            Directories are{(_settings.IncludeDirectories ? "" : " [red]not[/]")} included.
            Display errors is {(_settings.ShowErrors ? "[green]enabled" : "[red]disabled")}[/].
            """);

        try
        {
            Directory.Exists(_settings.SearchPath);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            Environment.Exit(1);
        }

        var directoryInfo = new DirectoryInfo(_settings.SearchPath);
        var fileExplorer = new FileExplorer(
            _settings.Name,
            _settings.Exact,
            _settings.includeDirectories);

        var response = await fileExplorer.FindAsync(directoryInfo);
        StringBuilder builder = new();
        if (_settings.ShowErrors)
        {
            builder.Append("\n--- [bold][red]ERRORS[/][/] ---\n");
            foreach (var line in response.Errors)
            {
                builder.AppendLine(
                    $"\n:stop_sign: [bold][red]Error[/]: {Markup.Escape(line)}[/]");
            }
        }

        builder.Append("\n--- [bold][green]MATCH[/][/] ---\n");
        foreach (var line in response.MatchedFilePaths)
        {
            builder.Append(
                $"\n[bold][green]Match[/]: {Markup.Escape(line)}[/]");
        }
        AnsiConsole.Markup(builder.ToString());
        return 0;
    }
}
