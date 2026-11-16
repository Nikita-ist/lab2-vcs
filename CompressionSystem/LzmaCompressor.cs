using Serilog;
using SharpCompress.Compressors.LZMA;

namespace CompressionSystem;

/// <summary>
/// Узел 4: LZMA (NuGet-пакет SharpCompress). Добавлен в ЛР №1 без изменения ArchiveManager.
/// Формат: [5 байт свойств][8 байт исходной длины][сжатые данные]
/// </summary>
public class LzmaCompressor : ICompressor
{
    public string Name => "LZMA";
    public string Extension => ".lzma";

    public void Compress(Stream input, Stream output)
    {
        Log.Information("[{Algorithm}] Сжатие началось", Name);
        long originalLength = input.Length - input.Position;

        var props = new LzmaEncoderProperties();
        using var lzma = LzmaStream.Create(props, false, output);

        output.Write(lzma.Properties, 0, lzma.Properties.Length);
        output.Write(BitConverter.GetBytes(originalLength), 0, sizeof(long));

        input.CopyTo(lzma);
    }

    public void Decompress(Stream input, Stream output)
    {
        Log.Information("[{Algorithm}] Распаковка началась", Name);
        var props = new byte[5];
        input.ReadExactly(props, 0, props.Length);

        var lenBytes = new byte[sizeof(long)];
        input.ReadExactly(lenBytes, 0, lenBytes.Length);
        long originalLength = BitConverter.ToInt64(lenBytes, 0);

        long compressedSize = input.Length - input.Position;
        using var lzma = LzmaStream.Create(props, input, compressedSize, originalLength, true);
        lzma.CopyTo(output);
    }
}
