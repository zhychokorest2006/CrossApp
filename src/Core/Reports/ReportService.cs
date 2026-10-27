using Core.Domain;

namespace Core.Reports;

// Усі запити LINQ. Приймає звичайну колекцію товарів, нічого не знає про сховище і консоль
public static class ReportService
{
    // 1) Фільтр + сортування + проєкція в DTO: що закінчується?
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)                         // спочатку відбираємо
              .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)   // потім сортуємо
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))          // перетворюємо в DTO
              .ToList();                                                  // тут запит виконується

    // 2) Топ-N зі стабільним порядком: чого найбільше?
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) =>
        source.OrderByDescending(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Take(n)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 3) Групування з підрахунком і сумою: скільки позицій і яка сума за кожною одиницею?
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)    // g.Key = одиниця виміру
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
              .OrderByDescending(s => s.TotalQuantity)
              .ToList();

    // 4) Агрегати з обробкою порожньої колекції: підсумок по складу
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();

        // Average і Max на порожній колекції кидають виняток, тому перевіряємо наперед
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, list.Sum(p => p.Quantity), list.Average(p => p.Quantity), list.Max(p => p.Quantity));
    }

    // ===== Додаткові завдання =====

    // Додаткове 1: GroupJoin двох колекцій (товари і переміщення) з проєкцією в зведений DTO
    public static IReadOnlyList<MovementSummary> MovementsByProduct(
        IEnumerable<Product> products, IEnumerable<StockMovement> movements) =>
        products.GroupJoin(
                movements,
                p => p.Sku,                  // ключ у товарів
                m => m.Sku,                  // ключ у переміщень
                (p, ms) => new MovementSummary(p.Sku, p.Name, ms.Count(), ms.Sum(m => m.Delta)))
            .OrderBy(s => s.Sku, StringComparer.Ordinal)
            .ToList();

    // Додаткове 2 (використання TopBy №1): топ товарів за кількістю переміщень
    public static IReadOnlyList<MovementSummary> TopMovements(
        IEnumerable<Product> products, IEnumerable<StockMovement> movements, int n) =>
        MovementsByProduct(products, movements)
            .TopBy(s => s.Movements, n, s => s.Sku)
            .ToList();

    // Додаткове 2 (використання TopBy №2): топ одиниць виміру за сумарним залишком
    public static IReadOnlyList<GroupSummary> TopUnits(IEnumerable<Product> source, int n) =>
        ByUnit(source)
            .TopBy(s => s.TotalQuantity, n, s => s.Key)
            .ToList();

    // Додаткове 3: те саме групування, але в синтаксисі запитів (from ... group ... into ...)
    public static IReadOnlyList<GroupSummary> ByUnitQuerySyntax(IEnumerable<Product> source)
    {
        var query = from p in source
                    group p by p.Unit.ToLowerInvariant() into g
                    orderby g.Sum(p => p.Quantity) descending
                    select new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity));

        return query.ToList();
    }
}