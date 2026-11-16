using System.IO.Compression;
using Serilog;

namespace CompressionSystem;

/// <summary>Узел 2: GZIP (встроенный System.IO.Compression).</summary>
public class GZipCompressor : ICompressor
{
    public string Name => "GZIP";
    public string Extension => ".gz";

    public void Compress(Stream input, Stream output)
    {
        Log.Information("[{Algorithm}] Сжатие началось", Name);
        using var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true);
        input.CopyTo(gzip);
    }

    public void Decompress(Stream input, Stream output)
    {
        Log.Information("[{Algorithm}] Распаковка началась", Name);
        using var gzip = new GZipStream(input, CompressionMode.Decompress, leaveOpen: true);
        gzip.CopyTo(output);
    }
}
