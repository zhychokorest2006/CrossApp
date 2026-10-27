using Core.Abstractions;
using Core.Events;

// Приймач, який друкує сповіщення в консоль
public sealed class ConsoleSink : INotificationSink
{
    public void Notify(CatalogChangedEventArgs e)
    {
        Console.WriteLine(ChangeFormatter.Format(e));
    }
}