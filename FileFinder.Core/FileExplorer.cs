using System.Collections.Concurrent;
using FileFinder.Core.Handler;

namespace FileFinder.Core;

public class FileExplorer
{
    private readonly StreamWriter _streamWriter;
    private readonly string _searchPath;
    private readonly FileHandler _fileHandler;
    private readonly DirectoryInfo _directoryInfo;
    private readonly bool _showErrors;

    public FileExplorer(
        string? fileName,
        string? extension,
        string? searchPath,
        bool? showErrors,
        bool? exactFileName,
        Stream? stream
    )
    {
        _fileHandler = new(fileName, extension, exactFileName);
        _showErrors = showErrors ?? false;

        _streamWriter = stream is not null ? new(stream) : new(Console.OpenStandardError());

        _streamWriter.AutoFlush = true;

        this._searchPath =
            !string.IsNullOrEmpty(searchPath) && searchPath.Length > 0
                ? searchPath
                : Directory.GetCurrentDirectory();

        _directoryInfo = new DirectoryInfo(this._searchPath);
    }

    public FileExplorer(string? fileName, string? extension, string? searchPath, bool? showErrors)
        : this(fileName, extension, searchPath, showErrors, null, null) { }

    public FileExplorer(string? fileName, string? extension, string? searchPath)
        : this(fileName, extension, searchPath, null, null, null) { }

    public FileExplorer(string? fileName, string? extension)
        : this(fileName, extension, null, null, null, null) { }

    public FileExplorer(string? fileName)
        : this(fileName, null, null, null, null, null) { }

    public FileExplorer()
        : this(null, null, null, null, null, null) { }

    public async IAsyncEnumerable<List<string>> FindAsync(
        CancellationTokenSource cancellationToken
    )
    {
        var queue = new ConcurrentQueue<DirectoryInfo>();
        queue.Enqueue(_directoryInfo);

        while (!queue.IsEmpty)
        {
            List<string> paths = [];
            int queueSize = queue.Count;
            Task<List<string>>[] tasks = new Task<List<string>>[queueSize];
            // This while loop spawns threads until current queue elements
            // are dequeued scanning one level at a time using `queueSize`
            while (--queueSize >= 0)
            {
                if (queue.TryDequeue(out var currentDirectory))
                {
                    tasks[queueSize] = Task.Run(() => ProcessDirectory(currentDirectory, queue));
                }
            }
            // Awaiting all tasks to finish
            await Task.WhenAll(tasks);

            if (cancellationToken.IsCancellationRequested)
                yield break;

            foreach (var task in tasks)
                paths.AddRange(task.Result);

            yield return paths;
        }
    }

    private List<string> ProcessDirectory(
        DirectoryInfo currentDirectory,
        ConcurrentQueue<DirectoryInfo> queue
    )
    {
        List<string> tempPaths = [];

        try
        {
            // Excluding all the subdirectory that points to other folders
            // to avoid infinite recursion
            foreach (
                var subdirectory in currentDirectory
                    .EnumerateDirectories()
                    .Where(x => x.LinkTarget == null)
            )
            {
                queue.Enqueue(subdirectory);
            }

            foreach (var file in currentDirectory.EnumerateFiles())
            {
                string relativePath = Path.GetRelativePath(_searchPath, file.FullName);
                if (_fileHandler.Validate(relativePath))
                {
                    tempPaths.Add(relativePath);
                }
            }
        }
        catch (Exception ex)
        {
            if (_showErrors)
            {
                lock (_streamWriter)
                {
                    _streamWriter.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        return tempPaths;
    }
}
