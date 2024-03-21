using System.Collections.Concurrent;

namespace FileFinder.Core
{
    public class FileExplorer
    {
        public FileExplorer(string name, bool? exact, bool? includeDirectories)
        {
            Target = name;
            Exact = exact ?? false;
            IncludeDirectories = includeDirectories ?? false;
            Response = new();
        }

        public string Target { get; set; }
        public bool Exact { get; set; }
        public bool IncludeDirectories { get; set; }
        private Response Response { get; set; } = new();

        public async Task<Response> FindAsync(DirectoryInfo directoryInfo)
        {
            var queue = new ConcurrentQueue<DirectoryInfo>();
            queue.Enqueue(directoryInfo);

            while (!queue.IsEmpty)
            {
                await Task.Run(async () =>
                {
                    var dirProcessTasks = new List<Task>();
                    while (!queue.IsEmpty)
                    {
                        if (queue.TryDequeue(out var currentDirectory))
                        {
                            dirProcessTasks.Add(Task.Run(() => ProcessDirectory(currentDirectory, queue)));
                        }
                    }
                    await Task.WhenAll(dirProcessTasks);
                });
            }
            return Response;
        }

        public void ProcessDirectory(DirectoryInfo directoryInfo, ConcurrentQueue<DirectoryInfo> queue)
        {
            try
            {
                CheckDirectoryName(directoryInfo);

                foreach (var subdirectory in directoryInfo.EnumerateDirectories()
                    .Where(x => x.LinkTarget == null))
                {
                    queue.Enqueue(subdirectory);
                }

                foreach (FileInfo file in directoryInfo.EnumerateFiles())
                {
                    CheckFileName(file);
                }
            }
            catch (Exception ex)
            {
                Response.Errors.Add(ex.Message);
            }
        }

        public void CheckDirectoryName(DirectoryInfo directoryInfo)
        {
            if (IncludeDirectories &&
                ((Exact && directoryInfo.Name == Target) ||
                (Exact && directoryInfo.FullName.Contains(Target))))
            {
                Response.MatchedFilePaths.Add(directoryInfo.FullName);
            }
        }

        public void CheckFileName(FileInfo file)
        {
            if ((Exact && file.Name == Target) ||
                (!Exact && file.FullName.Contains(Target)))
            {
                Response.MatchedFilePaths.Add(file.FullName);
            }
        }
    }
}
