using CSharpTui.Core.Prompts;
using FileFinder.Core;
using Cocona;

CoconaLiteApp.Run((
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
    var cancellationToken = new CancellationTokenSource();

    var task = Task.Run(async () =>
    {
        await foreach (var paths in fileExplorer.FindAsync(cancellationToken))
            prompt.AddChoices(paths);
    });

    var selectedPath = prompt
        .Show("Search results");
    cancellationToken.Cancel();

    Console.WriteLine(selectedPath);
});

