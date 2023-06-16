using FinancialDataAnalysisTool.Models;

namespace FinancialDataAnalysisTool.FinancialFormulars;
public class FinancialReturns
{
     public List<ReturnData> CalculateReturns(List<StockPrice> stockPrices, string symbol)
    {
        // Calculate returns for the given symbol
        var returns = new List<ReturnData>();
        var prices = stockPrices.Where(s => s.Symbol == symbol).OrderBy(s => s.Date).ToList();
        for (int i = 1; i < prices.Count; i++)
        {
            var previousPrice = prices[i - 1];
            var currentPrice = prices[i];
            var returnData = new ReturnData
            {
                Date = currentPrice.Date,
                Return = (currentPrice.Close - previousPrice.Close) / previousPrice.Close
            };
            returns.Add(returnData);
        }
        return returns;
    }

}