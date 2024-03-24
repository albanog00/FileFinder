using Spectre.Console.Cli;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ValidationResult = Spectre.Console.ValidationResult;

namespace FileFinder.Cli
{
    public class FileFinderCommandSettings : CommandSettings
    {
        [Description("File name to search.")]
        [CommandArgument(0, "[FILE_NAME]")]
        [Required]
        public string Name { get; init; } = string.Empty;

        [Description("Path where to search file. Default current directory.")]
        [CommandOption("-p|--path <SEARCH_PATH>")]
        public string SearchPath { get; init; } = Directory.GetCurrentDirectory();

        [Description("Display errors. Default disabled")]
        [CommandOption("--show-errors")]
        public bool ShowErrors { get; init; } = false;

        [Description("All files with the provided extension")]
        [CommandOption("-e|--extension")]
        public string Extension { get; init; } = string.Empty;

        [Description("Search for exact file name")]
        [CommandOption("--exact")]
        public bool Exact { get; init; } = false;

        public override ValidationResult Validate()
        {
            if (!Directory.Exists(SearchPath))
            {
                return ValidationResult.Error($"Provided path `{SearchPath}` doesn't exists.");
            }

            if (Exact && (string.IsNullOrEmpty(Name) || Name.Length == 0))
            {
                return ValidationResult.Error("Can't search for exact match if file name is null.");
            }
            return base.Validate();
        }
    }
}
