using Core.Domain;

namespace Core.Abstractions;

// Контракт асинхронного імпорту: звідки саме беруться товари, сервісу байдуже
public interface ICatalogImporter
{
    Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default);
}