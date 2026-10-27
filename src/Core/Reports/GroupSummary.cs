namespace Core.Reports;

// Підсумок по одній групі: ключ групи, скільки позицій, сумарний залишок
public sealed record GroupSummary(string Key, int Count, int TotalQuantity);