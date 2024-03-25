using FileFinder.Core.Handler;
using System.Collections.Concurrent;
using System.Text;

namespace FileFinder.Core;

public class FileExplorer
{
    private readonly StreamWriter _streamWriter =
        new(Console.OpenStandardOutput()) { AutoFlush = true };

    private readonly string _searchPath;
    private readonly FileHandler _fileHandler;
    private readonly DirectoryInfo _directoryInfo;
    private readonly bool _showErrors;

    public FileExplorer(
        string? fileName,
        string? extension,
        string? searchPath,
        bool? showErrors,
        bool? exactFileName)
    {
        _fileHandler = new(fileName, extension, exactFileName);
        _showErrors = showErrors ?? false;

        this._searchPath =
            !string.IsNullOrEmpty(searchPath) && searchPath.Length > 0
                ? searchPath
                : Directory.GetCurrentDirectory();

        _directoryInfo = new DirectoryInfo(this._searchPath);
    }

    public FileExplorer(
        string? fileName,
        string? extension,
        string? searchPath,
        bool? showErrors)
        : this(fileName, extension, searchPath, showErrors, null) { }

    public FileExplorer(string? fileName, string? extension, string? searchPath)
        : this(fileName, extension, searchPath, null, null) { }

    public FileExplorer(string? fileName, string? extension)
        : this(fileName, extension, null, null, null) { }

    public FileExplorer(string? fileName)
        : this(fileName, null, null, null, null) { }

    public FileExplorer()
        : this(null, null, null, null, null) { }

    public async Task<string[]> FindAsync()
    {
        var queue = new ConcurrentQueue<DirectoryInfo>();
        queue.Enqueue(_directoryInfo);

        List<string> paths = [];
        while (!queue.IsEmpty)
        {
            int queueSize = queue.Count;
            Task[] dirProcessTasks = new Task[queueSize];
            // This while loop spawns threads until current queue elements
            // are dequeued scanning one level at a time using `queueSize`
            while (--queueSize >= 0)
            {
                if (queue.TryDequeue(out var currentDirectory))
                {
                    dirProcessTasks[queueSize] = Task.Run(() =>
                        ProcessDirectory(currentDirectory, queue, paths));
                }
            }
            // Awaiting all threads to finish
            await Task.WhenAll(dirProcessTasks);
        }
        return [.. paths];
    }

    private void ProcessDirectory(
        DirectoryInfo currentDirectory,
        ConcurrentQueue<DirectoryInfo> queue,
        List<string> paths)
    {
        StringBuilder builder = new();

        try
        {
            // Excluding all the subdirectory that points to other folders
            // to avoid infinite recursion
            foreach (var subdirectory in currentDirectory
                .EnumerateDirectories()
                .Where(x => x.LinkTarget == null))
            {
                queue.Enqueue(subdirectory);
            }

            List<string> tempPaths = [];
            foreach (var file in currentDirectory.EnumerateFiles())
            {
                string relativePath =
                    Path.GetRelativePath(_searchPath, file.FullName);
                if (_fileHandler.Validate(relativePath))
                {
                    tempPaths.Add(relativePath);
                }
            }

            lock (paths)
            {
                paths.AddRange(tempPaths);
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
    }
}
