using FileFinder.Core.Handler;
using System.Collections.Concurrent;

namespace FileFinder.Core;

public class FileExplorer
{
    private string SearchPath { get; set; }
    private FileHandler FileChecker { get; set; }
    private DirectoryInfo DirectoryInfo { get; set; }
    private Response Response { get; set; } = new();

    public FileExplorer(
        string? fileName,
        string? extension,
        string? searchPath)
    {
        FileChecker = new(fileName, extension);
        SearchPath =
            !string.IsNullOrEmpty(searchPath) && searchPath.Length > 0
                ? searchPath
                : Directory.GetCurrentDirectory();
        DirectoryInfo = new DirectoryInfo(SearchPath);
    }

    public FileExplorer()
        : this(null, null, null) { }

    public FileExplorer(string fileName)
        : this(fileName, null, null) { }

    public FileExplorer(string fileName, string extension)
        : this(fileName, extension, null) { }

    public async Task<Response> FindAsync()
    {
        var queue = new ConcurrentQueue<DirectoryInfo>();
        queue.Enqueue(DirectoryInfo);

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
        return Response;
    }

    private void ProcessDirectory(
        DirectoryInfo currentDirectory,
        ConcurrentQueue<DirectoryInfo> queue)
    {
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
                    Path.GetRelativePath(SearchPath, file.FullName);
                if (FileChecker.Validate(relativePath))
                {
                    lock (Response.MatchedFilePaths)
                    {
                        Response.MatchedFilePaths.Add(relativePath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            lock (Response.Errors)
            {
                Response.Errors.Add(ex.Message);
            }
        }
    }
}
