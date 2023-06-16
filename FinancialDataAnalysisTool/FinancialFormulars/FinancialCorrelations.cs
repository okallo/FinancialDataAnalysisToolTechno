using FinancialDataAnalysisTool.Models;

namespace FinancialDataAnalysisTool.FinancialFormulars;
public class FinancialCorrelations
{
        public List<CorrelationData> CalculateCorrelations(List<StockPrice> stockPrices, string[] symbols)
    {
        // Calculate correlations between symbols
        var correlations = new List<CorrelationData>();
        foreach (var symbolA in symbols)
        {
            foreach (var symbolB in symbols)
            {
                if (symbolA != symbolB)
                {
                    var pricesA = stockPrices.Where(s => s.Symbol == symbolA).OrderBy(s => s.Date).ToList();
                    var pricesB = stockPrices.Where(s => s.Symbol == symbolB).OrderBy(s => s.Date).ToList();

                    var correlationData = new CorrelationData
                    {
                        SymbolA = symbolA,
                        SymbolB = symbolB,
                        Correlation = CalculateCorrelation(pricesA, pricesB)
                    };
                    correlations.Add(correlationData);
                }
            }
        }
        return correlations;
    }
      private decimal CalculateCorrelation(List<StockPrice> pricesA, List<StockPrice> pricesB)
    {
        // Calculate correlation between two series of prices
        var returnsA = CalculateReturns(pricesA);
        var returnsB = CalculateReturns(pricesB);

        var averageA = returnsA.Average();
        var averageB = returnsB.Average();

        var deviationsA = returnsA.Select(r => r - averageA);
        var deviationsB = returnsB.Select(r => r - averageB);

        var productSum = deviationsA.Zip(deviationsB, (deviationA, deviationB) => deviationA * deviationB).Sum();

        var squaredDeviationsA = deviationsA.Select(d => d * d).Sum();
        var squaredDeviationsB = deviationsB.Select(d => d * d).Sum();

        var correlation = productSum / (decimal)Math.Sqrt((double)(squaredDeviationsA * squaredDeviationsB));

        return correlation;
    }


    private List<decimal> CalculateReturns(List<StockPrice> prices)
    {
        // Calculate returns for a series of prices
        var returns = new List<decimal>();
        for (int i = 1; i < prices.Count; i++)
        {
            var previousPrice = prices[i - 1];
            var currentPrice = prices[i];
            var priceReturn = (currentPrice.Close - previousPrice.Close) / previousPrice.Close;
            returns.Add(priceReturn);
        }
        return returns;
    }
}