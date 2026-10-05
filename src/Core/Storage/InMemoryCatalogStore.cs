using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

// Сховище в оперативній пам'яті: після закриття програми дані зникають
public sealed class InMemoryCatalogStore : ICatalogStore
{
    private readonly Dictionary<string, Product> _items = new(StringComparer.OrdinalIgnoreCase);

    // Початкові дані можна передати, а можна й ні
    public InMemoryCatalogStore(IEnumerable<Product>? seed = null)
    {
        if (seed is null) return;

        foreach (Product product in seed)
            _items[product.Id] = product;
    }

    public IReadOnlyList<Product> List() => _items.Values.ToList();

    public Product? GetById(string id) => _items.GetValueOrDefault(id);

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");

        _items.Add(item.Id, item);
    }

    public void Update(Product item) => _items[item.Id] = item;

    public bool Remove(string id) => _items.Remove(id);
}