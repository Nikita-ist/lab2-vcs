using System.Diagnostics;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Serilog;

namespace CompressionSystem;

/// <summary>Результат работы одного алгоритма.</summary>
public record CompressionResult(
    string Algorithm,
    string OutputPath,
    long OriginalSize,
    long CompressedSize,
    TimeSpan Elapsed,
    bool RoundTripOk)
{
    public double Ratio => OriginalSize == 0 ? 0 : (double)CompressedSize / OriginalSize * 100.0;
}

/// <summary>
/// Основной модуль. Зависит ТОЛЬКО от абстракции ICompressor
/// (внедрение зависимостей через конструктор).
/// </summary>
public class ArchiveManager
{
    private readonly IReadOnlyList<ICompressor> _compressors;

    public ArchiveManager(IEnumerable<ICompressor> compressors)
    {
        ArgumentNullException.ThrowIfNull(compressors);
        _compressors = compressors.ToList();
    }

    public List<CompressionResult> CompressAll(string sourceFile, string outputDir)
    {
        if (!File.Exists(sourceFile))
            throw new FileNotFoundException("Исходный файл не найден", sourceFile);

        Directory.CreateDirectory(outputDir);
        long originalSize = new FileInfo(sourceFile).Length;
        var results = new List<CompressionResult>();

        Log.Information("Обработка файла {File} ({Size} байт), алгоритмов: {Count}",
            sourceFile, originalSize, _compressors.Count);

        foreach (var compressor in _compressors)
        {
            string outPath = Path.Combine(outputDir, Path.GetFileName(sourceFile) + compressor.Extension);

            var sw = Stopwatch.StartNew();
            using (var input = File.OpenRead(sourceFile))
            using (var output = File.Create(outPath))
            {
                compressor.Compress(input, output);
            }
            sw.Stop();

            bool ok = VerifyRoundTrip(compressor, sourceFile, outPath);

            var result = new CompressionResult(
                compressor.Name, outPath, originalSize,
                new FileInfo(outPath).Length, sw.Elapsed, ok);
            results.Add(result);

            Log.Information("[{Algorithm}] {Original} -> {Compressed} байт ({Ratio:F2} %), {Ms:F1} мс, проверка: {Ok}",
                result.Algorithm, result.OriginalSize, result.CompressedSize,
                result.Ratio, result.Elapsed.TotalMilliseconds, result.RoundTripOk);
        }

        return results;
    }

    /// <summary>Сохраняет список результатов в JSON (Newtonsoft.Json).</summary>
    public void SaveResultsToJson(IEnumerable<CompressionResult> results, string path)
    {
        string json = JsonConvert.SerializeObject(results, Formatting.Indented);
        File.WriteAllText(path, json);
        Log.Information("Результаты сохранены в {Path}", path);
    }

    private static bool VerifyRoundTrip(ICompressor compressor, string originalFile, string archiveFile)
    {
        string tmp = Path.GetTempFileName();
        try
        {
            using (var input = File.OpenRead(archiveFile))
            using (var output = File.Create(tmp))
            {
                compressor.Decompress(input, output);
            }
            return Hash(originalFile).SequenceEqual(Hash(tmp));
        }
        finally
        {
            File.Delete(tmp);
        }
    }

    private static byte[] Hash(string path)
    {
        using var sha = SHA256.Create();
        using var fs = File.OpenRead(path);
        return sha.ComputeHash(fs);
    }
}
