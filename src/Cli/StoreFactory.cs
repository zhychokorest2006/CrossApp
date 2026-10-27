using Core;
using Core.Abstractions;
using Core.Storage;

// Фабрика: єдине місце в Cli, де обираються конкретні сховища
public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

        ICatalogStore store = args.Contains("--file")
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(SampleData.Products());

        // Декоратор обгортає будь-яке з двох сховищ
        if (args.Contains("--cache"))
            store = new CachingCatalogStore(store);

        return store;
    }
}