using System.IO.Compression;
using Serilog;

namespace CompressionSystem;

/// <summary>Узел 1: ZIP (встроенный System.IO.Compression).</summary>
public class ZipCompressor : ICompressor
{
    private const string EntryName = "data";

    public string Name => "ZIP";
    public string Extension => ".zip";

    public void Compress(Stream input, Stream output)
    {
        Log.Information("[{Algorithm}] Сжатие началось", Name);
        using var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);
        var entry = zip.CreateEntry(EntryName, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        input.CopyTo(entryStream);
    }

    public void Decompress(Stream input, Stream output)
    {
        Log.Information("[{Algorithm}] Распаковка началась", Name);
        using var zip = new ZipArchive(input, ZipArchiveMode.Read, leaveOpen: true);
        using var entryStream = zip.Entries[0].Open();
        entryStream.CopyTo(output);
    }
}
