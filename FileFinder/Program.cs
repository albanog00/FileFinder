using CSharpTui.Core.Prompts;
using FileFinder.Core;
using Cocona;

CoconaLiteApp.Run(async (
    [Argument] string fileName,
    [Option('p')]string? searchPath,
    bool showErrors,
    [Option('e')]string? extension,
    bool exact) =>
{
    var fileExplorer = new FileExplorer(
        fileName, extension, searchPath, showErrors, exact, null);

    var paths = await fileExplorer.FindAsync();
    var selectedPath = new SelectionPrompt<string>()
        .AddChoices(paths)
        .Show("Search results");

    Console.WriteLine(selectedPath);
});

