using System.Text;

namespace FileFinder.Core
{
    public record struct Response()
    {
        public List<string> Errors { get; private set; } = [];
        public List<string> MatchedFilePaths { get; private set; } = [];
    }
}
