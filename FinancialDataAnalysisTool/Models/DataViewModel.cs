namespace FinancialDataAnalysisTool.Models;
public class DataViewModel
{
    public List<string> Symbols { get; set; } = new List<string>();
    public List<StockPrice> StockPrices {get; set;} = new List<StockPrice>();

    
    // Add other properties as needed for the views
}