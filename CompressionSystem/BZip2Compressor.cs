using ICSharpCode.SharpZipLib.BZip2;
using Serilog;

namespace CompressionSystem;

/// <summary>Узел 3: BZIP2 (NuGet-пакет SharpZipLib).</summary>
public class BZip2Compressor : ICompressor
{
    public string Name => "BZIP2";
    public string Extension => ".bz2";

    public void Compress(Stream input, Stream output)
    {
        Log.Information("[{Algorithm}] Сжатие началось", Name);
        BZip2.Compress(input, output, false, 9);
    }

    public void Decompress(Stream input, Stream output)
    {
        Log.Information("[{Algorithm}] Распаковка началась", Name);
        BZip2.Decompress(input, output, false);
    }
}
