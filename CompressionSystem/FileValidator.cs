using System.IO;

public static class FileValidator
{
    public static bool Exists(string path)
    {
        return File.Exists(path);
    }

    public static long GetSize(string path)
    {
        return new FileInfo(path).Length;
    }
}