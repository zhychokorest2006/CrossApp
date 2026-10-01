using System;
using Core.Dto;

namespace Core.Domain;

public class Product
{
    private int _quantity;

    public string Id { get; }
    public string Sku { get; }
    public string Name { get; }
    public string Unit { get; }
    public int Quantity => _quantity;
    public ProductStatus Status { get; private set; }

    // Приватний конструктор — пряме створення ззовні неможливе
    private Product(string id, string sku, string name, string unit, int quantity, ProductStatus status = ProductStatus.Active)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Unit = unit;
        _quantity = quantity;
        Status = status;
    }

    // Фабричний метод для безпечного створення об'єкта
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

    // Прихід товару
    public void RegisterArrival(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount,
                "Кількість приходу має бути більшою за нуль");

        _quantity += amount;
    }

    // Видача товару
    public void Issue(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount,
                "Кількість видачі має бути більшою за нуль");

        if (amount > _quantity)
            throw new InvalidOperationException(
                $"Не можна видати {amount}: залишок {Sku} = {_quantity}");

        _quantity -= amount;
    }

    // Зміна статусу (State Machine через switch expression з патерном кортежу)
    public void ChangeStatus(ProductStatus newStatus)
    {
        bool allowed = (Status, newStatus) switch
        {
            (ProductStatus.Active, ProductStatus.OutOfStock) => true,
            (ProductStatus.Active, ProductStatus.Discontinued) => true,
            (ProductStatus.OutOfStock, ProductStatus.Active) => true,
            (ProductStatus.OutOfStock, ProductStatus.Discontinued) => true,
            (ProductStatus.Discontinued, _) => false, // фінальний стан
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException(
                $"Перехід зі стану {Status} у {newStatus} заборонений");

        Status = newStatus;
    }

    // Перетворення у DTO (для експорту або сумісності з ЛР3)
    public ProductDto ToDto()
    {
        return new ProductDto(Id, Sku, Name, Unit, _quantity);
    }

    // Відновлення сутності з DTO з обов'язковою повторною валідацією інваріантів
    public static Product FromDto(ProductDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        return Create(dto.Id, dto.Sku, dto.Name, dto.Unit, dto.Quantity);
    }

    public override string ToString()
    {
        return $"{Id} [{Sku}] {Name} — {Quantity} {Unit}, статус: {Status}";
    }
}