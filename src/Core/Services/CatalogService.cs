using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class CatalogService
{
    private readonly ICatalogStore _store;
    private readonly List<INotificationSink> _sinks;

    // Приймачі сповіщень необов'язкові: Cli може передати їх через конструктор
    public CatalogService(ICatalogStore store, IEnumerable<INotificationSink>? sinks = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _sinks = sinks?.ToList() ?? new List<INotificationSink>();
    }

    // Подія: ззовні можна лише підписатися (+=) і відписатися (-=)
    public event EventHandler<CatalogChangedEventArgs>? Changed;

    // Повідомляє про зміну: піднімає подію і викликає приймачі.
    // Знак ? потрібен, щоб не впасти, коли підписників немає
    private void OnChanged(string id, CatalogChangeKind kind, int delta, int quantityAfter)
    {
        var e = new CatalogChangedEventArgs(id, kind, delta, quantityAfter);

        Changed?.Invoke(this, e);

        foreach (INotificationSink sink in _sinks)
            sink.Notify(e);
    }

    // Подію піднімаємо лише після успішної зміни: якщо виникне виняток, до неї не дійде

    public Product Add(string sku, string name, string unit, int quantity)
    {
        string id = Guid.NewGuid().ToString("N").Substring(0, 8);
        Product product = Product.Create(id, sku, name, unit, quantity);

        _store.Add(product);
        OnChanged(product.Id, CatalogChangeKind.Added, quantity, product.Quantity);
        return product;
    }

    public void Receive(string id, int quantity)
    {
        Product product = GetOrThrow(id);

        product.RegisterArrival(quantity);
        _store.Update(product);
        OnChanged(product.Id, CatalogChangeKind.Received, quantity, product.Quantity);
    }

    public void Issue(string id, int quantity)
    {
        Product product = GetOrThrow(id);

        product.Issue(quantity);
        _store.Update(product);
        OnChanged(product.Id, CatalogChangeKind.Issued, -quantity, product.Quantity);
    }

    public bool Remove(string id)
    {
        Product? product = _store.GetById(id);
        if (product == null)
            return false;

        _store.Remove(id);
        OnChanged(id, CatalogChangeKind.Removed, -product.Quantity, 0);
        return true;
    }

    // Імпорт: імпортер приходить параметром (ручний DI), а подія піднімається для кожного доданого запису
    public async Task<int> ImportAsync(ICatalogImporter importer, string path, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(importer);
        ct.ThrowIfCancellationRequested();

        var items = await importer.LoadAsync(path, ct);

        var added = 0;
        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();

            if (_store.GetById(item.Id) is not null)
                continue;   // такий запис уже є: повторний імпорт нічого не дублює

            _store.Add(item);
            OnChanged(item.Id, CatalogChangeKind.Added, item.Quantity, item.Quantity);
            added++;
        }

        return added;
    }

    public IReadOnlyList<Product> All() => _store.List();

    public Product? Find(string id) => _store.GetById(id);

    // Func: умову пошуку задає той, хто викликає, а результат повертається значенням
    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate) =>
        _store.List().Where(predicate).ToList();

    private Product GetOrThrow(string id) =>
        _store.GetById(id) ?? throw new DomainException($"Немає запису з id={id}.");
}