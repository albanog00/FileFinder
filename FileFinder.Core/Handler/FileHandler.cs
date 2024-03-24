using System.Runtime.InteropServices;

namespace FileFinder.Core.Handler;

public class FileHandler
{
    private readonly bool _shouldCheckFileName = false;
    private readonly bool _shouldCheckExtension = false;
    private readonly bool _exactFileName = false;
    private readonly string _fileName = string.Empty;
    private readonly string _extension = string.Empty;

    public FileHandler(
        string? fileName,
        string? extension,
        bool? exactFileName)
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

    public bool Validate(string relativePath)
    {
        string fileName = GetFileName(relativePath);
        string extension = _shouldCheckExtension
            ? GetFileExtension(fileName)
            : string.Empty;

        // TODO: Rewrite this
        bool matchTarget = !_shouldCheckFileName
            || (_exactFileName && fileName.Split('.')[0] == _fileName)
            || (!_exactFileName && fileName.Contains(_fileName));

        bool matchExtension = !_shouldCheckExtension ||
            (extension != string.Empty
             && extension.Contains(_extension));

        return matchTarget && matchExtension;
    }

    public static string GetFileExtension(string fileName)
    {
        var spanFileName = CollectionsMarshal.AsSpan(fileName.ToList());
        int length = spanFileName.Length;

        for (int i = 0; i < length; ++i)
        {
            if (spanFileName[i] == '.')
            {
                return fileName[i..length];
            }
        }
        return string.Empty;
    }

    public static string GetFileName(string path)
    {
        var spanPath = CollectionsMarshal.AsSpan(path.ToList());
        int length = spanPath.Length;

        for (int i = length - 1; i >= 0; --i)
        {
            if (IsDirectorySeparator(spanPath[i]))
            {
                return path[(i + 1)..length];
            }
        }
        return path;
    }

    public static bool IsDirectorySeparator(char separator) => separator switch
    {
        '\\' => true,
        '/' => true,
        _ => false
    };
}
