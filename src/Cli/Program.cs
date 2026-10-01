using System.Text;
using Core.Domain;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("=== Сценарій 1: успіх ===");
var product = Product.Create("P-001", "SKU-001", "Цемент М400 25кг", "шт", 100);
Console.WriteLine(product);

product.RegisterArrival(20);
Console.WriteLine(product);
Console.WriteLine();

Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");

try
{
    product.Issue(1000);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  видача більша за залишок: {ex.GetType().Name} — {ex.Message}");
}

try
{
    Product.Create("P-002", "  ", "Пісок річковий", "т", 10);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"  порожній SKU: {ex.GetType().Name} — {ex.Message}");
}

try
{
    Product.Create("P-003", "SKU-003", "Вапно", "кг", -5);
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"  від'ємний залишок: {ex.GetType().Name} — {ex.Message}");
}
Console.WriteLine();

Console.WriteLine("=== Сценарій 3: перехід станів (додаткове завдання) ===");
product.ChangeStatus(ProductStatus.OutOfStock);
Console.WriteLine($"Новий статус: {product.Status}");

product.ChangeStatus(ProductStatus.Discontinued);

try
{
    product.ChangeStatus(ProductStatus.Active);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  заборонений перехід зі знятого з обліку: {ex.GetType().Name} — {ex.Message}");
}
Console.WriteLine();

Console.WriteLine("=== Сценарій 4: інваріант на дві сутності — Warehouse (додаткове завдання) ===");
var wood = Product.Create("P-100", "SKU-100", "Дошка обрізна", "шт", 50);
var warehouse = new Warehouse();
string customerId = "CUST-1";

warehouse.IssueToCustomer(wood, customerId, 5);
warehouse.IssueToCustomer(wood, customerId, 5);
warehouse.IssueToCustomer(wood, customerId, 5);
Console.WriteLine($"Після трьох видач: {wood}");

try
{
    warehouse.IssueToCustomer(wood, customerId, 5);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  четверта видача тому самому клієнту (ліміт 3): {ex.GetType().Name} — {ex.Message}");
}