using System.Text;
using CompressionSystem;
using Serilog;

Console.OutputEncoding = Encoding.UTF8;

// Настройка Serilog: вывод в консоль и в файл logs/app.log
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log")
    .CreateLogger();

try
{
    string sourceFile = args.Length > 0 ? args[0] : CreateSampleFile("sample.txt");
    string outputDir = "output";

    // 1. Система с тремя алгоритмами
    var compressors = new List<ICompressor>
    {
        new ZipCompressor(),
        new GZipCompressor(),
        new BZip2Compressor()
    };
    var manager = new ArchiveManager(compressors);
    PrintResults(manager.CompressAll(sourceFile, outputDir));

    // 2. Добавляем LZMA без изменения ArchiveManager
    var extended = new List<ICompressor>(compressors) { new LzmaCompressor() };
    var extendedManager = new ArchiveManager(extended);
    var results = extendedManager.CompressAll(sourceFile, outputDir);
    PrintResults(results);

    // 3. Сохранение результатов в JSON
    extendedManager.SaveResultsToJson(results, "results.json");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Необработанная ошибка");
}
finally
{
    Log.CloseAndFlush();
}

static void PrintResults(List<CompressionResult> results)
{
    Console.WriteLine();
    Console.WriteLine($"{"Алгоритм",-8} {"Исходный",12} {"Сжатый",12} {"Степень, %",11} {"Время, мс",10} {"Проверка",9}");
    Console.WriteLine(new string('-', 68));
    foreach (var r in results)
    {
        Console.WriteLine(
            $"{r.Algorithm,-8} {r.OriginalSize,12:N0} {r.CompressedSize,12:N0} " +
            $"{r.Ratio,11:F2} {r.Elapsed.TotalMilliseconds,10:F1} {(r.RoundTripOk ? "OK" : "ОШИБКА"),9}");
    }
    Console.WriteLine();
}

static string CreateSampleFile(string path)
{
    var sb = new StringBuilder();
    var rnd = new Random(42);
    string[] words = { "система", "сжатия", "данных", "алгоритм", "класс", "интерфейс",
                       "зависимость", "связность", "архив", "файл", "поток", "модуль" };
    for (int i = 0; i < 80_000; i++)
        sb.Append(words[rnd.Next(words.Length)]).Append(i % 12 == 11 ? '\n' : ' ');

    File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    return path;
}
