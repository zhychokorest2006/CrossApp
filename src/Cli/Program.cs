using System.Text;
using Core;
using Core.Abstractions;
using Core.Reports;
using Core.Services;

Console.OutputEncoding = Encoding.UTF8;

// Сховище і сервіс (composition root)
ICatalogStore store = StoreFactory.Create(args);
var service = new CatalogService(store);

// Знімок усіх товарів: звіти рахуються по ньому
var items = service.All();

// Команда = перший аргумент, що не починається з "--" (тому --file не плутається з назвою звіту)
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

switch (command)
{
    case "report-top":
        Console.WriteLine("Топ-5 за залишком:");
        Print(ReportService.TopByQuantity(items, 5));
        break;

    case "report-low":
        Console.WriteLine("Дефіцит (залишок менше 10):");
        Print(ReportService.LowStock(items, 10));
        break;

    case "report-groups":
        Console.WriteLine("Групування за одиницею виміру:");
        PrintGroups(ReportService.ByUnit(items));
        break;

    case "report-sum":
        var t = ReportService.Totals(items);
        Console.WriteLine($" позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
        break;

    // --- додаткові завдання ---
    case "report-top-units":
        Console.WriteLine("Топ-3 одиниці виміру за сумарним залишком:");
        PrintGroups(ReportService.TopUnits(items, 3));
        break;

    case "report-movements":
        Console.WriteLine("Топ-5 товарів за кількістю переміщень (GroupJoin):");
        foreach (var m in ReportService.TopMovements(items, SampleData.Movements(), 5))
            Console.WriteLine($" {m.Sku,-10} {m.Name,-22} переміщень: {m.Movements,2}  зміна: {m.NetChange,5}");
        break;

    case "report-groups-query":
        Console.WriteLine("Групування за одиницею виміру (синтаксис запитів):");
        PrintGroups(ReportService.ByUnitQuerySyntax(items));
        break;

    // --- демонстрації ---
    case "lazy":
        LazyDemo.Run(service);
        break;

    case "bench":
        Benchmark.Run();
        break;

    case "events":
        EventsDemo.Run(service, store);
        break;

    default:
        Console.WriteLine("Команди: report-top | report-low | report-groups | report-sum | lazy | events");
        Console.WriteLine("Додаткові: report-top-units | report-movements | report-groups-query | bench");
        break;
}

static void Print(IReadOnlyList<StockLine> lines)
{
    if (lines.Count == 0)
    {
        Console.WriteLine("  (порожньо)");
        return;
    }

    foreach (var l in lines)
        Console.WriteLine($" {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
}

static void PrintGroups(IReadOnlyList<GroupSummary> groups)
{
    if (groups.Count == 0)
    {
        Console.WriteLine("  (порожньо)");
        return;
    }

    foreach (var g in groups)
        Console.WriteLine($" {g.Key,-6} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
}