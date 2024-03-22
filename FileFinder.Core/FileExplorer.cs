using System.Collections.Concurrent;

namespace FileFinder.Core
{
    public class FileExplorer
    {
        private readonly bool _shouldCheckFileName = false;
        private readonly bool _shouldCheckExtension = false;

        public string FileName { get; init; } = string.Empty;
        public string Extension { get; init; } = string.Empty;
        public bool IncludeDirectories { get; init; } = false;
        private Response Response { get; set; } = new();

        public FileExplorer(
            string? fileName, string? extension, bool? includeDirectories)
        {
            IncludeDirectories = includeDirectories ?? false;

            if (!string.IsNullOrEmpty(fileName) && fileName.Length > 0)
            {
                FileName = fileName;
                _shouldCheckFileName = true;
            }

            if (!string.IsNullOrEmpty(extension) && extension.Length > 0)
            {
                // Adds a `.` at the start if not present
                Extension = extension[0] == '.' ? extension : '.' + extension;
                _shouldCheckExtension = true;
            }
        }

        public async Task<Response> FindAsync(DirectoryInfo directoryInfo)
        {
            var queue = new ConcurrentQueue<DirectoryInfo>();
            queue.Enqueue(directoryInfo);

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
            DirectoryInfo directoryInfo,
            ConcurrentQueue<DirectoryInfo> queue)
        {
            try
            {
                // Excluding all the subdirectory that points to other folders
                // to avoid infinite recursion
                foreach (var subdirectory in directoryInfo
                    .EnumerateDirectories()
                    .Where(x => x.LinkTarget == null))
                {
                    queue.Enqueue(subdirectory);
                }

                foreach (var file in directoryInfo.EnumerateFiles())
                {
                    if (CheckFile(file))
                    {
                        lock (Response.MatchedFilePaths)
                        {
                            Response.MatchedFilePaths.Add(file.FullName);
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

        private bool CheckDirectoryName(DirectoryInfo directoryInfo) =>
            IncludeDirectories && directoryInfo.FullName.Contains(FileName);

        private bool CheckFile(FileInfo file)
        {
            bool containsTarget = !_shouldCheckFileName ||
                file.Name.Contains(FileName);
            bool matchExtension = !_shouldCheckExtension ||
                file.Extension == Extension;

            return containsTarget && matchExtension;
        }
    }
}
