using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Services;

// Сценарій лабораторної 6 (події, підписка, відписка), винесений з Program.cs у команду "events"
public static class EventsDemo
{
    public static void Run(CatalogService service, ICatalogStore store)
    {
        // 2. Файл журналу: каталог створюємо самі
        string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

        // 3. Підписник 1: локальний метод, друкує подію в консоль і у файл
        void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
        {
            string line = ChangeFormatter.Format(e);
            Console.WriteLine(line);

            try
            {
                File.AppendAllText(logPath, line + Environment.NewLine);
            }
            catch (IOException ex)
            {
                // Помилка запису в журнал не повинна ламати саму операцію
                Console.WriteLine($"  (не вдалося записати в журнал: {ex.Message})");
            }
        }

        // 4. Підписник 2: лямбда, що збільшує лічильник
        int changes = 0;
        service.Changed += OnCatalogChanged;
        service.Changed += (_, _) => changes++;

        // Допоміжний метод, щоб не повторювати рядок виводу товару
        void PrintProduct(Product p) =>
            Console.WriteLine($"  {p.Id} {p.Sku,-10} {p.Name,-22} {p.Quantity,5} {p.Unit}");

        Console.WriteLine($"Сховище: {store.GetType().Name}");
        Console.WriteLine();

        // 5. Зміни: кожну з них повідомляє подія
        Console.WriteLine("=== Зміни: кожну повідомляє подія ===");
        var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
        service.Receive(created.Id, 25);
        service.Issue(created.Id, 10);

        var temp = service.Add("SKU-102", "Тимчасовий товар", "шт", 3);
        service.Remove(temp.Id);
        Console.WriteLine($"Подій отримано: {changes}");

        // 6. Пошук і список
        Console.WriteLine();
        Console.WriteLine("=== Знайти за id ===");
        Console.WriteLine(service.Find(created.Id));

        Console.WriteLine();
        Console.WriteLine("=== Усі товари ===");
        foreach (var p in service.All())
            PrintProduct(p);

        Console.WriteLine();
        Console.WriteLine("=== Пошук через Func: залишок менше 100 ===");
        foreach (var p in service.FindAll(p => p.Quantity < 100))
            PrintProduct(p);

        // 7. Відписка: операція виконується, повідомлення немає
        Console.WriteLine();
        Console.WriteLine("=== Після відписки: операція є, повідомлення немає ===");
        service.Changed -= OnCatalogChanged;
        service.Receive(created.Id, 5);
        var updated = service.Find(created.Id);
        Console.WriteLine($"Залишок змінився: {updated?.Quantity}");
        Console.WriteLine($"Подій отримано: {changes}"); // +1 лише від лямбди

        // 8. Відмови: помилка є, події немає
        Console.WriteLine();
        Console.WriteLine("=== Сценарій відмови: помилка є, події немає ===");

        try
        {
            service.Receive("NO-SUCH-ID", 5);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  помилка: {ex.Message}");
        }

        try
        {
            service.Receive(created.Id, -5);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  помилка: {ex.Message}");
        }

        try
        {
            service.Issue(created.Id, 9999);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  помилка: {ex.Message}");
        }

        Console.WriteLine($"Подій отримано: {changes}"); // не змінилось

        // 9. Додаткові завдання
        Console.WriteLine();
        Console.WriteLine("=== Додатково 1: те саме через INotificationSink ===");
        ExtraDemo.RunSinkDemo();

        Console.WriteLine();
        Console.WriteLine("=== Додатково 2: Action<string> в імпорті ===");
        ExtraDemo.RunImportDemo();
    }
}