using Core.Domain;

namespace Core.Abstractions;

// Контракт сховища: що можна робити з товарами, але не як вони зберігаються
public interface ICatalogStore
{
    IReadOnlyList<Product> List();
    Product? GetById(string id);
    void Add(Product item);
    void Update(Product item);
    bool Remove(string id);
}