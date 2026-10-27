namespace Core.Reports;

// Переміщення товару на складі: артикул і на скільки змінилась кількість (+ прихід, - видача)
public sealed record StockMovement(string Sku, int Delta);