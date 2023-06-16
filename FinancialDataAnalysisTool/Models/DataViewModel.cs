namespace FinancialDataAnalysisTool.Models;
public class DataViewModel
{
    public List<string> Symbols { get; set; } = new List<string>();
    public List<StockPrice> StockPrices {get; set;} = new List<StockPrice>();
    public List<Earnings> Earnings {get; set;} = new List<Earnings>();
    public List<Dividends> Dividends {get; set;} = new List<Dividends>();

}