using CSharpTui.Core.Prompts;
using FileFinder.Core;
using Cocona;

CoconaLiteApp.Run(async (
    [Argument] string? fileName,
    [Option('p')] string? searchPath,
    [Option('e')] string? extension,
    bool showErrors,
    bool exact
    ) =>
{
    var fileExplorer = new FileExplorer(
        fileName, extension, searchPath, showErrors, exact, null);
    var prompt = new SelectionPrompt<string>();
    var tokenSource = new CancellationTokenSource();

    var task = Task.Run(async () =>
    {
        await foreach (var paths in fileExplorer.FindAsync(tokenSource))
            prompt.AddChoices(paths);
    });

    var selectedPath = await prompt.ShowAsync("Search results", tokenSource);

    Console.WriteLine(selectedPath);
});

