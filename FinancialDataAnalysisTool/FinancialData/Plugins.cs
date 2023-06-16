using FinancialDataAnalysisTool.Models;

namespace FinancialDataAnalysisTool.FinancialData;
public class Plugins
{
      public decimal CheckValue(string? v)
    {
        var openValue = v;
        return decimal.TryParse(openValue, out var openResult) ? openResult : 0;

    }

    public DateTime FixDate(string date){
        var d =double.TryParse(date, out double numericDate) ? numericDate : DateTime.Now.ToOADate();
        return DateTime.FromOADate(d);
    }

    public List<ChartData> PrepareChartData(List<StockPrice> stockPrices)
    {
        // Prepare chart data for visualization
        var chartData = new List<ChartData>();
        foreach (var symbol in stockPrices.Select(s => s.Symbol).Distinct())
        {
            var prices = stockPrices.Where(s => s.Symbol == symbol).OrderBy(s => s.Date).ToList();
            var dataPoints = prices.Select(p => new DataPoint(p.Date.ToString("yyyy-MM-dd"), (double)p.Close)).ToList();
            var chartDataset = new ChartData
            {
                Symbol = symbol,
                DataPoints = dataPoints
            };
            chartData.Add(chartDataset);
        }
        return chartData;
    }

    public List<StockPrice> FilterDataByTime(List<StockPrice> stockPrices, string symbol, string startDate, string endDate)
    {
        // Filter data by symbol, start date, and end date
        var filteredData = stockPrices.Where(s =>
            s.Symbol == symbol &&
            s.Date >= DateTime.Parse(startDate) &&
            s.Date <= DateTime.Parse(endDate)
        ).ToList();
        return filteredData;
    }

    public List<StockPrice> FilterDataBySymbol(List<StockPrice> stockPrices, string[] symbols)
    {
        // Filter data by symbols
        var filteredData = stockPrices.Where(s => symbols.Contains(s.Symbol)).ToList();
        return filteredData;
    }


}