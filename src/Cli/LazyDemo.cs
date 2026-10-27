using Core.Domain;
using Core.Services;

// Демонстрація відкладеного виконання LINQ
public static class LazyDemo
{
    public static void Run(CatalogService service)
    {
        // Беремо знімок у звичайний список: з ним можна гратися, не чіпаючи сховище
        var source = service.All().ToList();

        // Запит ПОБУДОВАНО, але ще НЕ виконано: нічого не пораховано
        var query = source.Where(p => p.Quantity < 10);

        // Count() запускає запит саме зараз
        Console.WriteLine($"Дефіцитних до змін: {query.Count()}");

        // Змінюємо джерело вже після побудови запиту
        source.Add(Product.Create("tmp00001", "SKU-999", "Скоба", "шт", 3));

        // Той самий запит виконується знову і бачить новий товар
        Console.WriteLine($"Дефіцитних після змін: {query.Count()}");

        // ToList() фіксує результат: це вже готовий список, а не запит
        var snapshot = query.ToList();
        source.Clear();

        Console.WriteLine($"У знімку лишилось: {snapshot.Count}");               // не змінилось
        Console.WriteLine($"Той самий запит після очищення джерела: {query.Count()}");   // тепер 0
    }
}