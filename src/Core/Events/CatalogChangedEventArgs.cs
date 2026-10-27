namespace Core.Events;

// Дані події: що саме змінилося
public sealed class CatalogChangedEventArgs : EventArgs
{
    public CatalogChangedEventArgs(string id, CatalogChangeKind kind, int delta, int quantityAfter)
    {
        Id = id;
        Kind = kind;
        Delta = delta;
        QuantityAfter = quantityAfter;
    }

    public string Id { get; }
    public CatalogChangeKind Kind { get; }
    public int Delta { get; }          // на скільки змінилась кількість
    public int QuantityAfter { get; }  // залишок після зміни
    public DateTimeOffset At { get; } = DateTimeOffset.Now;
}