using CompressionSystem;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CompressionSystem.Tests;

/// <summary>Тесты основного модуля: проверяют слабую связность (подстановка mock-объектов).</summary>
public class ArchiveManagerTests : IDisposable
{
    private readonly string _dir;
    private readonly string _source;

    public ArchiveManagerTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "lab3_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        _source = Path.Combine(_dir, "input.txt");
        File.WriteAllText(_source, string.Concat(Enumerable.Repeat("hello compression ", 500)));
    }

    public void Dispose() => Directory.Delete(_dir, true);

    [Fact]
    public void ArchiveManager_Should_Call_All_Compressors()
    {
        var mock1 = new MockCompressor("M1", ".m1");
        var mock2 = new MockCompressor("M2", ".m2");
        var manager = new ArchiveManager(new[] { mock1, mock2 });

        var results = manager.CompressAll(_source, Path.Combine(_dir, "out"));

        Assert.Equal(1, mock1.CompressCalls);
        Assert.Equal(1, mock2.CompressCalls);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void ArchiveManager_Should_Verify_RoundTrip_For_Every_Compressor()
    {
        var manager = new ArchiveManager(new[]
        {
            new MockCompressor("M1", ".m1"),
            new MockCompressor("M2", ".m2")
        });

        var results = manager.CompressAll(_source, Path.Combine(_dir, "out"));

        Assert.All(results, r => Assert.True(r.RoundTripOk));
    }

    [Fact]
    public void ArchiveManager_Should_Accept_New_Compressor_Without_Code_Changes()
    {
        var list = new List<ICompressor>
        {
            new MockCompressor("M1", ".m1"),
            new MockCompressor("M2", ".m2")
        };
        list.Add(new MockCompressor("M3", ".m3"));   // "добавили LZMA" — менеджер не меняется

        var results = new ArchiveManager(list).CompressAll(_source, Path.Combine(_dir, "out"));

        Assert.Equal(new[] { "M1", "M2", "M3" }, results.Select(r => r.Algorithm));
    }

    [Fact]
    public void ArchiveManager_Should_Throw_When_Source_File_Missing()
    {
        var manager = new ArchiveManager(new[] { new MockCompressor() });

        Assert.Throws<FileNotFoundException>(() =>
            manager.CompressAll(Path.Combine(_dir, "missing.txt"), Path.Combine(_dir, "out")));
    }

    [Fact]
    public void SaveResultsToJson_Should_Write_Valid_Json()
    {
        var manager = new ArchiveManager(new[]
        {
            new MockCompressor("M1", ".m1"),
            new MockCompressor("M2", ".m2")
        });
        var results = manager.CompressAll(_source, Path.Combine(_dir, "out"));
        string jsonPath = Path.Combine(_dir, "results.json");

        manager.SaveResultsToJson(results, jsonPath);

        var array = JArray.Parse(File.ReadAllText(jsonPath));
        Assert.Equal(2, array.Count);
        Assert.Equal("M1", (string?)array[0]["Algorithm"]);
    }
}
