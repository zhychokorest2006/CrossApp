using Core;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

public class ImportTests
{
    private const string ValidJson = """
        [
          { "Id": "a1", "Sku": "SKU-A1", "Name": "Перший", "Unit": "шт", "Quantity": 10 },
          { "Id": "a2", "Sku": "SKU-A2", "Name": "Другий", "Unit": "м", "Quantity": 20 }
        ]
        """;

    private const string BadJson = """
        [
          { "Id": "a1", "Sku": "SKU-A1", "Name": "Перший", "Unit": "шт", "Quantity": 10 },
          { "Id": "a2", "Sku": "", "Name": "Без артикулу", "Unit": "м", "Quantity": 20 }
        ]
        """;

    // Записуємо тимчасовий файл, виконуємо дію і гарантовано видаляємо файл
    private static async Task WithTempFileAsync(string json, Func<string, Task> action)
    {
        var path = Path.Combine(Path.GetTempPath(), $"import-test-{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, json);
        try
        {
            await action(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact] // 8) скасування
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var service = new CatalogService(new InMemoryCatalogStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), "any.json", cts.Token));
    }

    [Fact] // 9) погані дані: доменна помилка з номером запису
    public async Task ImportAsync_BadRecord_ThrowsDomainException_WithRecordNumber()
    {
        await WithTempFileAsync(BadJson, async path =>
        {
            var service = new CatalogService(new InMemoryCatalogStore());

            var ex = await Assert.ThrowsAsync<DomainException>(
                () => service.ImportAsync(new JsonCatalogImporter(), path));

            Assert.Contains("Запис 2", ex.Message);
        });
    }

    [Fact] // 10) успішний імпорт: записи додано, подія для кожного
    public async Task ImportAsync_ValidFile_AddsRecords_AndRaisesEventPerRecord()
    {
        await WithTempFileAsync(ValidJson, async path =>
        {
            var store = new InMemoryCatalogStore();
            var service = new CatalogService(store);
            var events = new List<CatalogChangedEventArgs>();
            service.Changed += (_, e) => events.Add(e);

            var added = await service.ImportAsync(new JsonCatalogImporter(), path);

            Assert.Equal(2, added);
            Assert.Equal(2, store.List().Count);
            Assert.Equal(2, events.Count);
        });
    }

    [Fact] // 11) повторний імпорт нічого не дублює
    public async Task ImportAsync_SecondTime_AddsNothing()
    {
        await WithTempFileAsync(ValidJson, async path =>
        {
            var service = new CatalogService(new InMemoryCatalogStore());
            await service.ImportAsync(new JsonCatalogImporter(), path);

            var addedAgain = await service.ImportAsync(new JsonCatalogImporter(), path);

            Assert.Equal(0, addedAgain);
        });
    }
}