using Core.Domain;

// Маленький фіксований набір товарів для тестів (не SampleData і не файли з data/)
public static class TestData
{
    public static List<Product> Items() =>
    [
        Product.Create("t1", "SKU-BIG", "Великий запас", "шт", 100),
        Product.Create("t2", "SKU-MID", "Середній запас", "шт", 50),
        Product.Create("t3", "SKU-LOW", "Малий запас", "м", 9),
        Product.Create("t4", "SKU-MIN", "Майже закінчився", "м", 3),
    ];
}