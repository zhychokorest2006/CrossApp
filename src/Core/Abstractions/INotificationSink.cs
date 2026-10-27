using Core.Events;

namespace Core.Abstractions;

// Приймач сповіщень: що з ними робити (показати, записати), вирішує конкретна реалізація
public interface INotificationSink
{
    void Notify(CatalogChangedEventArgs e);
}