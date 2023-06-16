namespace FinancialDataAnalysisTool.Models;
public class ChartDataViewModel
{
    public List<ChartData> VolatilityData { get; set; } = new List<ChartData>();
    public List<ChartData> CorrelationData { get; set; } = new List<ChartData>();
    public List<ChartData> ReturnsData { get; set; } = new List<ChartData>();
}


