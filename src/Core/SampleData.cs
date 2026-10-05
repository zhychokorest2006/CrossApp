using Core.Domain;

namespace Core;

// Демонстраційні дані для режиму «в пам'яті» (їх же використаємо на тижні 7)
public static class SampleData
{
    public static List<Product> Products() =>
    [
        Product.Create("P-001", "SKU-001", "Цемент М400 25кг", "шт", 120),
        Product.Create("P-002", "SKU-002", "Пісок будівельний", "т", 18),
        Product.Create("P-003", "SKU-003", "Цегла червона", "шт", 4200),
        Product.Create("P-004", "SKU-004", "Фарба водоемульсійна 10л", "шт", 36),
        Product.Create("P-005", "SKU-005", "Дошка обрізна", "шт", 50),
        Product.Create("P-006", "SKU-006", "Вапно гашене", "кг", 300),
        Product.Create("P-007", "SKU-007", "Гіпсокартон 12мм", "шт", 85),
        Product.Create("P-008", "SKU-008", "Шпаклівка фінішна 25кг", "шт", 40),
        Product.Create("P-009", "SKU-009", "Ґрунтовка 10л", "л", 90),
        Product.Create("P-010", "SKU-010", "Утеплювач мінвата 100мм", "м2", 220),
        Product.Create("P-011", "SKU-011", "Арматура 12мм", "м", 1500),
        Product.Create("P-012", "SKU-012", "Саморізи по дереву 4х40", "уп", 200),
        Product.Create("P-013", "SKU-013", "Плитковий клей 25кг", "шт", 64),
        Product.Create("P-014", "SKU-014", "Профіль CD 60", "шт", 130),
        Product.Create("P-015", "SKU-015", "Кабель ВВГ 3х2.5", "м", 600),
        Product.Create("P-016", "SKU-016", "Монтажна піна", "шт", 48),
    ];
}