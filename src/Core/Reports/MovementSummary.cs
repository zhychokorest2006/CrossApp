namespace Core.Reports;

// Зведення по товару: скільки було переміщень і яка загальна зміна кількості
public sealed record MovementSummary(string Sku, string Name, int Movements, int NetChange);