using CompressionSystem;

namespace CompressionSystem.Tests;

/// <summary>
/// Mock-компрессор: ничего не сжимает (копирует поток как есть) и считает вызовы.
/// Подставляется вместо реальных алгоритмов благодаря слабой связности (интерфейс ICompressor).
/// </summary>
public class MockCompressor : ICompressor
{
    public MockCompressor(string name = "MOCK", string extension = ".mock")
    {
        Name = name;
        Extension = extension;
    }

    public string Name { get; }
    public string Extension { get; }
    public int CompressCalls { get; private set; }
    public int DecompressCalls { get; private set; }

    public void Compress(Stream input, Stream output)
    {
        CompressCalls++;
        input.CopyTo(output);
    }

    public void Decompress(Stream input, Stream output)
    {
        DecompressCalls++;
        input.CopyTo(output);
    }
}
