using System.Diagnostics;
using Core.Domain;
using Core.Reports;

// Додаткове 4: вимірюємо час звіту на 100 000 записів
public static class Benchmark
{
    public static void Run()
    {
        var source = Generate(100_000);
        const int runs = 10;

        Console.WriteLine($"Записів: {source.Count}, повторів кожного варіанту: {runs}");

        // Варіант A: один ланцюжок, ToList лише в кінці
        double a = Measure(runs, () => source
            .Where(p => p.Quantity < 100)
            .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList());

        // Варіант B: зайвий ToList посередині (ще один тимчасовий список)
        double b = Measure(runs, () => source
            .Where(p => p.Quantity < 100)
            .ToList()
            .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList());

        // Варіант C: типова помилка, Where після OrderBy (сортуємо всі 100 000, а не відібрані)
        double c = Measure(runs, () => source
            .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Where(p => p.Quantity < 100)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList());

        Console.WriteLine($"  A: один ланцюжок           {a,8:F2} мс");
        Console.WriteLine($"  B: ToList посередині       {b,8:F2} мс");
        Console.WriteLine($"  C: Where після OrderBy     {c,8:F2} мс");
    }

    // Середній час виконання дії в мілісекундах (перший прогін не рахуємо: він "прогріває" код)
    static double Measure(int runs, Action action)
    {
        action();

        var watch = Stopwatch.StartNew();
        for (int i = 0; i < runs; i++)
            action();
        watch.Stop();

        return watch.Elapsed.TotalMilliseconds / runs;
    }

    // Генеруємо товари з однаковим результатом при кожному запуску (Random з фіксованим зерном)
    static List<Product> Generate(int count)
    {
        string[] units = { "шт", "м", "кг", "л", "уп" };
        var random = new Random(42);
        var list = new List<Product>(count);

        for (int i = 0; i < count; i++)
        {
            list.Add(Product.Create(
                $"id{i}", $"SKU-{i:D6}", $"Товар {i}", units[random.Next(units.Length)], random.Next(0, 1000)));
        }

        return list;
    }
}