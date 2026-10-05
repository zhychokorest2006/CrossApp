using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

// Бізнес-операції складу. Сервіс знає лише інтерфейс сховища, а не конкретний клас
public sealed class CatalogService
{
    private readonly ICatalogStore _store;

    public CatalogService(ICatalogStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Product Add(string sku, string name, string unit, int quantity)
    {
        // Інваріанти перевіряє Product.Create
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        _store.Add(product);
        return product;
    }

    public void Receive(string id, int quantity)
    {
        Product product = GetOrThrow(id);
        product.RegisterArrival(quantity);
        _store.Update(product);
    }

    public void Issue(string id, int quantity)
    {
        Product product = GetOrThrow(id);
        product.Issue(quantity);
        _store.Update(product);
    }

    public IReadOnlyList<Product> All() => _store.List();

    public Product? Find(string id) => _store.GetById(id);

    // Пошук за довільною умовою, яку передає той, хто викликає
    public IReadOnlyList<Product> Search(Func<Product, bool> predicate) =>
        _store.List().Where(predicate).ToList();

    private Product GetOrThrow(string id) =>
        _store.GetById(id) ?? throw new InvalidOperationException($"Немає запису з id={id}.");
}