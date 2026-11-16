using CompressionSystem;
using Xunit;

namespace CompressionSystem.Tests;

/// <summary>Тесты реальных алгоритмов сжатия (дополнительное задание).</summary>
public class CompressorTests
{
    public static IEnumerable<object[]> AllCompressors()
    {
        yield return new object[] { new ZipCompressor() };
        yield return new object[] { new GZipCompressor() };
        yield return new object[] { new BZip2Compressor() };
        yield return new object[] { new LzmaCompressor() };
    }

    private static byte[] SampleData() =>
        System.Text.Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("compression test data ", 1000)));

    private static byte[] CompressToBytes(ICompressor compressor, byte[] data)
    {
        var output = new MemoryStream();
        compressor.Compress(new MemoryStream(data), output);
        return output.ToArray();
    }

    [Theory]
    [MemberData(nameof(AllCompressors))]
    public void Compress_Then_Decompress_Should_Return_Original_Data(ICompressor compressor)
    {
        byte[] original = SampleData();
        byte[] compressed = CompressToBytes(compressor, original);

        var restored = new MemoryStream();
        compressor.Decompress(new MemoryStream(compressed), restored);

        Assert.Equal(original, restored.ToArray());
    }

    [Theory]
    [MemberData(nameof(AllCompressors))]
    public void Compress_Should_Make_Repetitive_Data_Smaller(ICompressor compressor)
    {
        byte[] original = SampleData();

        byte[] compressed = CompressToBytes(compressor, original);

        Assert.True(compressed.Length < original.Length,
            $"{compressor.Name}: {compressed.Length} >= {original.Length}");
    }

    [Fact]
    public void Compressors_Should_Have_Unique_Extensions_Starting_With_Dot()
    {
        var extensions = AllCompressors().Select(a => ((ICompressor)a[0]).Extension).ToList();

        Assert.All(extensions, e => Assert.StartsWith(".", e));
        Assert.Equal(extensions.Count, extensions.Distinct().Count());
    }
}
