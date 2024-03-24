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
        this._searchPath =
            !string.IsNullOrEmpty(searchPath) && searchPath.Length > 0
                ? searchPath
                : Directory.GetCurrentDirectory();
        _directoryInfo = new DirectoryInfo(this._searchPath);
        _showErrors = showErrors ?? false;
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

    public async Task FindAsync()
    {
        var queue = new ConcurrentQueue<DirectoryInfo>();
        queue.Enqueue(_directoryInfo);

        while (!queue.IsEmpty)
        {
            List<Task> dirProcessTasks = [];

            // This while loop spawns threads until current queue elements
            // are dequeued scanning one level at a time using `queueSize`
            int queueSize = queue.Count;
            while (--queueSize >= 0)
            {
                if (queue.TryDequeue(out var currentDirectory))
                {
                    dirProcessTasks.Add(Task.Run(() =>
                        ProcessDirectory(currentDirectory, queue)));
                }
            }
            // Awaiting all threads to finish
            await Task.WhenAll(dirProcessTasks);
        }
    }

    private void ProcessDirectory(
        DirectoryInfo currentDirectory,
        ConcurrentQueue<DirectoryInfo> queue)
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

            foreach (var file in currentDirectory.EnumerateFiles())
            {
                string relativePath =
                    Path.GetRelativePath(_searchPath, file.FullName);
                if (_fileHandler.Validate(relativePath))
                {
                    builder.AppendLine(relativePath);
                }
            }
        }
        catch (Exception ex)
        {
            if (_showErrors)
            {
                builder.AppendLine($"Error: {ex.Message}");
            }
        }

        lock (_streamWriter)
        {
            _streamWriter.Write(builder);
        }
    }
}
