using System.Runtime.InteropServices;

namespace FileFinder.Core.Handler;

public class FileHandler
{
    private readonly bool _shouldCheckFileName = false;
    private readonly bool _shouldCheckExtension = false;

    public string FileName { get; init; } = string.Empty;
    public string Extension { get; init; } = string.Empty;

    public FileHandler(
        string? fileName,
        string? extension)
    {
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

    public FileHandler(string fileName)
        : this(fileName, null) { }

    public bool Validate(string relativePath)
    {
        string extension = _shouldCheckExtension
            ? GetFileExtension(relativePath)
            : string.Empty;

        bool containsTarget = !_shouldCheckFileName ||
            relativePath.Contains(FileName);

        bool matchExtension = !_shouldCheckExtension ||
            (extension != string.Empty && Extension.Contains(extension));

        return containsTarget && matchExtension;
    }

    public static string GetFileExtension(string path)
    {
        string fileName = GetFileName(path);
        var spanFileName = CollectionsMarshal.AsSpan(fileName.ToList());
        int length = spanFileName.Length;
        string extension = string.Empty;

        for (int i = length - 1; i >= 0; --i)
        {
            if (spanFileName[i] == '.')
            {
                extension = fileName[i..length];
            }
        }
        return extension;
    }

    public static string GetFileName(string path)
    {
        var spanPath = CollectionsMarshal.AsSpan(path.ToList());
        int length = spanPath.Length;

        for (int i = length - 1; i >= 0; --i)
        {
            if (IsDirectorySeparator(spanPath[i]))
            {
                return path[(i+1)..length];
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
