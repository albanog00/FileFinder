namespace FileFinder.Core.Handler;

public class FileHandler
{
    private readonly bool _shouldCheckFileName = false;
    private readonly bool _shouldCheckExtension = false;
    private readonly bool _exactFileName = false;
    private readonly string _fileName = string.Empty;
    private readonly string _extension = string.Empty;

    public FileHandler(string? fileName, string? extension, bool? exactFileName)
    {
        if (!string.IsNullOrEmpty(fileName) && fileName.Length > 0)
        {
            _fileName = fileName;
            _shouldCheckFileName = true;
        }

        if (!string.IsNullOrEmpty(extension) && extension.Length > 0)
        {
            // Adds a `.` at the start if not present
            _extension = extension[0] == '.' ? extension : '.' + extension;
            _shouldCheckExtension = true;
        }
        _exactFileName = exactFileName ?? false;
    }

    public FileHandler(string? fileName, string? extension)
        : this(fileName, extension, null) { }

    public FileHandler(string? fileName)
        : this(fileName, null, null) { }

    public FileHandler()
        : this(null, null, null) { }

    public bool Validate(string relativePath) => Validate(relativePath.AsSpan());

    public bool Validate(ReadOnlySpan<char> relativePath)
    {
        string fileName =
            _shouldCheckFileName || _shouldCheckExtension
                ? GetFileName(relativePath)
                : string.Empty;

        string extension = _shouldCheckExtension
            ? GetFileExtension(fileName.AsSpan())
            : string.Empty;

        // TODO: Rewrite this
        bool matchTarget =
            !_shouldCheckFileName
            || (_exactFileName && fileName.Split('.')[0] == _fileName)
            || (!_exactFileName && fileName.Contains(_fileName));

        bool matchExtension =
            !_shouldCheckExtension || (extension != string.Empty && extension.Contains(_extension));

        return matchTarget && matchExtension;
    }

    public static string GetFileExtension(string fileName) => GetFileExtension(fileName.AsSpan());

    public static string GetFileExtension(ReadOnlySpan<char> fileName)
    {
        int length = fileName.Length;
        for (int i = 0; i < length; ++i)
        {
            if (fileName[i] == '.')
            {
                return fileName[i..length].ToString();
            }
        }
        return string.Empty;
    }

    public static string GetFileName(string path) => GetFileName(path.AsSpan());

    public static string GetFileName(ReadOnlySpan<char> path)
    {
        int length = path.Length;
        for (int i = length - 1; i >= 0; --i)
        {
            if (IsDirectorySeparator(path[i]))
            {
                return path[(i + 1)..length].ToString();
            }
        }
        return path.ToString();
    }

    public static bool IsDirectorySeparator(char separator) =>
        separator switch
        {
            '\\' => true,
            '/' => true,
            _ => false,
        };
}
