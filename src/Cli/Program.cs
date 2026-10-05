using System.Text;
using Core.Abstractions;
using Core.Services;

Console.OutputEncoding = Encoding.UTF8;

// Composition root: сховище створює фабрика, сервіс отримує лише інтерфейс
ICatalogStore store = StoreFactory.Create(args);
var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");
Console.WriteLine();

Console.WriteLine("=== Додати товар ===");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
Console.WriteLine(created);

Console.WriteLine();
Console.WriteLine("=== Прихід 25 і видача 10 ===");
service.Receive(created.Id, 25);
service.Issue(created.Id, 10);

Console.WriteLine();
Console.WriteLine("=== Знайти за id ===");
Console.WriteLine(service.Find(created.Id));

Console.WriteLine();
Console.WriteLine("=== Усі товари ===");
foreach (var p in service.All())
    Console.WriteLine($"  {p.Id} {p.Sku,-10} {p.Name,-22} {p.Quantity,5} {p.Unit}");

Console.WriteLine();
Console.WriteLine("=== Пошук: залишок менше 100 ===");
foreach (var p in service.Search(p => p.Quantity < 100))
    Console.WriteLine($"  {p.Id} {p.Sku,-10} {p.Name,-22} {p.Quantity,5} {p.Unit}");

Console.WriteLine();
Console.WriteLine("=== Сценарій відмови ===");

try
{
    service.Receive("NO-SUCH-ID", 5);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  помилка: {ex.Message}");
}

try
{
    service.Issue(created.Id, 9999);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  помилка: {ex.Message}");
}