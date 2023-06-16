using FinancialDataAnalysisTool.Models;

namespace FinancialDataAnalysisTool.FinancialFormulars;
public class FinancialVolatility
{
     public double CalculateVolatility(List<StockPrice> stockPrices, string symbol)
    {
        // Calculate volatility for the given symbol
        var prices = stockPrices.Where(s => s.Symbol == symbol).Select(s => (double)s.Close).ToList();
        var logReturns = new List<double>();

        for (int i = 1; i < prices.Count; i++)
        {
            var logReturn = Math.Log(prices[i] / prices[i - 1]);
            logReturns.Add(logReturn);
        }

        var mean = logReturns.Average();
        var squaredDeviations = logReturns.Select(x => Math.Pow(x - mean, 2));
        var variance = squaredDeviations.Average();
        var volatility = Math.Sqrt(variance);

        return volatility;
    }
}
