using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

// Файлове сховище: дані тримаємо в пам'яті (кеш), а після кожної зміни записуємо у JSON-файл
public sealed class FileCatalogStore : ICatalogStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly Dictionary<string, Product> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path;
    private bool _loaded;

    public FileCatalogStore(string path)
    {
        _path = Path.GetFullPath(path);
    }

    // Читаємо файл лише один раз, при першому зверненні
    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (File.Exists(_path))
        {
            var dtos = JsonSerializer.Deserialize<List<ProductDto>>(File.ReadAllText(_path)) ?? [];
            foreach (ProductDto dto in dtos)
            {
                Product product = Product.FromDto(dto);
                _cache[product.Id] = product;
            }
        }

        _loaded = true;
    }

    // Записуємо весь кеш у файл (у вигляді DTO)
    private void Flush()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

        var dtos = _cache.Values.Select(p => p.ToDto()).ToList();
        File.WriteAllText(_path, JsonSerializer.Serialize(dtos, Options));
    }

    public IReadOnlyList<Product> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Product? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");

        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(Product item)
    {
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();

        if (!_cache.Remove(id)) return false;

        Flush();
        return true;
    }
}