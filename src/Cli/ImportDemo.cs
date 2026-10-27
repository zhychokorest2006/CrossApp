using System.Text.Json;
using Core;
using Core.Dto;
using Core.Services;
using Core.Storage;

// Асинхронний імпорт у Cli: скасування (Ctrl+C, тайм-аут) і окрема обробка помилок
public static class ImportDemo
{
    public static async Task RunAsync(CatalogService service, string path, TimeSpan? timeout = null)
    {
        // Джерело сигналу скасування: його створює Cli, а перевіряють імпортер і сервіс
        using var cts = new CancellationTokenSource();

        // Ctrl+C не вбиває програму, а лише скасовує токен
        ConsoleCancelEventHandler onCancelKey = (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        Console.CancelKeyPress += onCancelKey;

        if (timeout is not null)
            cts.CancelAfter(timeout.Value);   // тайм-аут теж скасовує токен

        try
        {
            var added = await service.ImportAsync(new JsonCatalogImporter(), path, cts.Token);
            Console.WriteLine($"Імпортовано записів: {added}");
        }
        // Порядок важливий: скасування обробляємо до загальних помилок
        catch (OperationCanceledException)
        {
            Console.WriteLine("Імпорт скасовано.");
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"Дані відхилено: {ex.Message}");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Файл має хибний формат JSON: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Помилка читання файлу: {ex.Message}");
        }
        finally
        {
            Console.CancelKeyPress -= onCancelKey;
        }
    }

    // Демонстрація скасування: створюємо великий файл у тимчасовій папці
    public static async Task RunBigAsync(CatalogService service, bool waitForCtrlC)
    {
        string path = Path.Combine(Path.GetTempPath(), $"big-import-{Guid.NewGuid():N}.json");

        try
        {
            Console.WriteLine("Створюю великий файл (500 000 записів)...");
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(MakeDtos(500_000)));

            if (waitForCtrlC)
            {
                Console.WriteLine("Імпортую... Натисни Ctrl+C, щоб скасувати.");
                await RunAsync(service, path);
            }
            else
            {
                Console.WriteLine("Імпортую з тайм-аутом 20 мс...");
                await RunAsync(service, path, TimeSpan.FromMilliseconds(20));
            }
        }
        finally
        {
            File.Delete(path);   // тимчасовий файл видаляємо завжди
        }
    }

    private static List<ProductDto> MakeDtos(int count)
    {
        var list = new List<ProductDto>(count);
        for (int i = 0; i < count; i++)
            list.Add(new ProductDto($"B-{i:D6}", $"SKU-B{i:D6}", $"Товар {i}", "шт", i % 1000));
        return list;
    }
}