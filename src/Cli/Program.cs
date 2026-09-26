using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

string extension = Path.GetExtension(path).ToLowerInvariant();

// Додаткове завдання 1: вибір імпортера через switch expression
ImportResult<ProductDto>? result = extension switch
{
    ".csv"  => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    _       => null
};

if (result is null)
{
    Console.WriteLine($"Непідтримуваний формат файлу '{extension}'. Підтримуються лише .csv та .json.");
    return 1;
}

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine($"  {p.Id,-6} {p.Sku,-10} {p.Name,-26} {p.Quantity,5} {p.Unit}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($"  ! {e}");
    }
}

// Додаткове завдання 3: Статистика одним рядком
int total = result.Items.Count + result.Errors.Count;
double errorRate = total > 0 ? (double)result.Errors.Count / total * 100 : 0;
Console.WriteLine($"\n[Статистика]: Усього: {total} | Прийнято: {result.Items.Count} | Пропущено: {result.Errors.Count} | Помилок: {errorRate:F1}%");

return 0;