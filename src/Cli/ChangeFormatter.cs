using Core.Events;

// Один рядок тексту для події: його використовують усі, хто показує або записує зміни
public static class ChangeFormatter
{
    public static string Format(CatalogChangedEventArgs e) =>
        $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} → залишок {e.QuantityAfter}";
}