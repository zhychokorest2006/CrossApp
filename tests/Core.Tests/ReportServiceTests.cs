using Core.Reports;
using Xunit;

public class ReportServiceTests
{
    [Theory] // 5) звіт на кількох наборах вхідних даних
    [InlineData(10, 2)]
    [InlineData(1, 0)]
    public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected) =>
        Assert.Equal(expected, ReportService.LowStock(TestData.Items(), threshold).Count);

    [Fact] // 6) детермінований топ-N з тижня 7
    public void TopByQuantity_IsDeterministic() =>
        Assert.Equal(
            new[] { "SKU-BIG", "SKU-MID" },
            ReportService.TopByQuantity(TestData.Items(), 2).Select(l => l.Sku));

    [Fact] // 7) порожнє джерело не призводить до винятку
    public void Totals_OnEmptySource_ReturnsZeros()
    {
        var totals = ReportService.Totals([]);

        Assert.Equal(0, totals.Items);
        Assert.Equal(0, totals.Max);
    }
}