namespace Core.Reports;

// Рядок звіту: простий незмінний запис, а не доменна сутність Product
public sealed record StockLine(string Sku, string Name, int Quantity, string Unit);