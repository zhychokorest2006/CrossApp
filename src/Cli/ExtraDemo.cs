using Core.Abstractions;
using Core.Import;
using Core.Services;
using Core.Storage;

// Додаткові завдання лабораторної 6
public static class ExtraDemo
{
    // Додаткове 1: ті самі сповіщення, але через інтерфейс INotificationSink
    public static void RunSinkDemo()
    {
        string sinkLogPath = Path.Combine(AppContext.BaseDirectory, "logs", "sink.log");

        // Приймачі створюємо тут і передаємо сервісу через конструктор (ручний DI)
        var sinks = new INotificationSink[] { new ConsoleSink(), new FileSink(sinkLogPath) };
        var service = new CatalogService(new InMemoryCatalogStore(), sinks);

        var item = service.Add("SKU-201", "Лампа LED", "шт", 20);
        service.Receive(item.Id, 10);
    }

    // Додаткове 2: імпорт приймає Action<string>, і лямбда отримує кожен пропущений рядок
    public static void RunImportDemo()
    {
        string path = Path.Combine("data", "sample.csv");
        if (!File.Exists(path))
        {
            Console.WriteLine($"  Файл не знайдено: {Path.GetFullPath(path)}");
            return;
        }

        int skipped = 0;
        var result = ProductCsvImporter.Load(path, message =>
        {
            skipped++;
            Console.WriteLine($"  пропущено: {message}");
        });

        Console.WriteLine($"Прийнято: {result.Items.Count}, пропущено: {skipped}");
    }
}