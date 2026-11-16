namespace CompressionSystem;

/// <summary>
/// Абстракция (центр "звезды" сетевой структуры из ЛР №1).
/// ArchiveManager знает ТОЛЬКО про этот интерфейс.
/// </summary>
public interface ICompressor
{
    string Name { get; }
    string Extension { get; }
    void Compress(Stream input, Stream output);
    void Decompress(Stream input, Stream output);
}
