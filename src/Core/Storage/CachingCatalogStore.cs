using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

// Декоратор: бере будь-яке інше сховище, не змінює контракт, але запам'ятовує список
public sealed class CachingCatalogStore : ICatalogStore
{
    private readonly ICatalogStore _inner;
    private IReadOnlyList<Product>? _listCache;

    public CachingCatalogStore(ICatalogStore inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    // Список беремо зі справжнього сховища лише раз, потім віддаємо запам'ятований
    public IReadOnlyList<Product> List() => _listCache ??= _inner.List();

    public Product? GetById(string id) => _inner.GetById(id);

    // Після будь-якої зміни запам'ятований список застарів, тому скидаємо його
    public void Add(Product item)
    {
        _inner.Add(item);
        _listCache = null;
    }

    public void Update(Product item)
    {
        _inner.Update(item);
        _listCache = null;
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);
        _listCache = null;
        return removed;
    }
}