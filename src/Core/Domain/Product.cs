using Core.Dto;

namespace Core.Domain;

public class Product
{
    public string Id { get; }
    public string Sku { get; }
    public string Name { get; }
    public string Unit { get; }
    public int Quantity { get; private set; }
    public ProductStatus Status { get; private set; } = ProductStatus.Active;

    // Конструктор приватний: створити товар можна лише через Create
    private Product(string id, string sku, string name, string unit, int quantity)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Unit = unit;
        Quantity = quantity;
    }

    public static Product Create(string id, string sku, string name, string unit, int quantity)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU не може бути порожнім", nameof(sku));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва не може бути порожньою", nameof(name));
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
                "Початковий залишок не може бути від'ємним");

        return new Product(id.Trim(), sku.Trim().ToUpperInvariant(), name.Trim(), unit?.Trim() ?? "шт", quantity);
    }

    public void RegisterArrival(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount,
                "Кількість приходу має бути більшою за нуль");

        Quantity += amount;
    }

    public void Issue(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount,
                "Кількість видачі має бути більшою за нуль");
        if (amount > Quantity)
            throw new InvalidOperationException(
                $"Не можна видати {amount}: залишок {Sku} = {Quantity}");

        Quantity -= amount;
    }

    public void ChangeStatus(ProductStatus newStatus)
    {
        bool allowed = (Status, newStatus) switch
        {
            (ProductStatus.Active, ProductStatus.OutOfStock) => true,
            (ProductStatus.Active, ProductStatus.Discontinued) => true,
            (ProductStatus.OutOfStock, ProductStatus.Active) => true,
            (ProductStatus.OutOfStock, ProductStatus.Discontinued) => true,
            _ => false // решта заборонена, зі стану Discontinued виходу немає
        };

        if (!allowed)
            throw new InvalidOperationException(
                $"Перехід зі стану {Status} у {newStatus} заборонений");

        Status = newStatus;
    }

    public ProductDto ToDto() => new(Id, Sku, Name, Unit, Quantity);

    // Іде через Create, тому правила перевіряються і при відновленні з DTO
    public static Product FromDto(ProductDto dto) =>
        Create(dto.Id, dto.Sku, dto.Name, dto.Unit, dto.Quantity);

    public override string ToString() =>
        $"{Id} [{Sku}] {Name} — {Quantity} {Unit}, статус: {Status}";
}