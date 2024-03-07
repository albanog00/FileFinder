using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console;
using Spectre.Console.Cli;

using ValidationResult = Spectre.Console.ValidationResult;

namespace FileFinder.Cli
{
    public class FileFinderCommand : Command<FileFinderCommand.Settings>
    {
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

        public override int Execute(CommandContext context, Settings settings) => ExecuteAsync(context, settings).Result;

        public async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            AnsiConsole.MarkupLineInterpolated($"Searching[red]{(settings.Exact ? " exactly" : "")}[/] [dodgerblue2]`{settings.Name}`[/] in {Emoji.Known.OpenFileFolder} [yellow]`{settings.SearchPath}`[/] and subdirectories.");
            AnsiConsole.MarkupLine($"Directories are{(settings.IncludeDirectories ? "" : " [red]not[/]")} included.");
            AnsiConsole.MarkupLine($"Display errors is {(settings.ShowErrors ? "[green]enabled" : "[red]disabled")}[/].\n");

            var directoryInfo = new DirectoryInfo(settings.SearchPath);
            await Find(directoryInfo, settings);

            return 0;
        }

        private async Task Find(DirectoryInfo directoryInfo, Settings settings)
        {
            if (settings.IncludeDirectories)
                if (settings.Exact ? directoryInfo.Name == settings.Name : directoryInfo.Name.Contains(settings.Name))
                    AnsiConsole.MarkupLineInterpolated($"[bold][green]Match![/] {Emoji.Known.OpenFileFolder} [yellow]{directoryInfo.FullName}[/][/]");

            try {
                foreach (var file in directoryInfo.GetFiles())
                    if (settings.Exact ? file.Name == settings.Name : file.Name.Contains(settings.Name))
                        AnsiConsole.MarkupLineInterpolated($"[bold][green]Match![/] {Emoji.Known.PageFacingUp} [dodgerblue2]{file.FullName}[/][/]");

                await Task.Run(async () => await Task.WhenAll(directoryInfo.GetDirectories().Select(x => Find(x, settings))));
            }
            catch (Exception ex) {
                if (settings.ShowErrors)
                    AnsiConsole.MarkupLineInterpolated($"[bold][red]Error:[/] {Emoji.Known.StopSign} {ex.Message}[/]");
            }
        }
    }
}
