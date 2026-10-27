using Core;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

public class CatalogServiceTests
{
    [Fact] // 1) інваріант: відмова і незмінний стан
    public void Receive_WithNegativeQuantity_Throws_AndKeepsState()
    {
        // Arrange
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);
        var product = service.Add("SKU-1", "Кабель", "м", 10);

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Receive(product.Id, -5));
        Assert.Equal(10, store.GetById(product.Id)!.Quantity);
    }

    [Fact] // 2) подія з тижня 6 як спосіб перевірки
    public void Add_RaisesChangedExactlyOnce()
    {
        var service = new CatalogService(new InMemoryCatalogStore());
        var events = new List<CatalogChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        service.Add("SKU-2", "Гвинт", "шт", 7);

        Assert.Single(events);
        Assert.Equal(CatalogChangeKind.Added, events[0].Kind);
    }

    [Fact] // 3) доменна помилка: події немає, стан не змінився
    public void Issue_MoreThanStock_ThrowsDomainException_AndRaisesNoEvent()
    {
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);
        var product = service.Add("SKU-3", "Болт", "шт", 5);

        var events = new List<CatalogChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        Assert.Throws<DomainException>(() => service.Issue(product.Id, 6));
        Assert.Empty(events);
        Assert.Equal(5, store.GetById(product.Id)!.Quantity);
    }

    [Fact] // 4) неіснуючий id: доменна помилка, а не NullReferenceException
    public void Receive_UnknownId_ThrowsDomainException()
    {
        var service = new CatalogService(new InMemoryCatalogStore());

        var ex = Assert.Throws<DomainException>(() => service.Receive("no-such-id", 5));

        Assert.Contains("no-such-id", ex.Message);
    }
}