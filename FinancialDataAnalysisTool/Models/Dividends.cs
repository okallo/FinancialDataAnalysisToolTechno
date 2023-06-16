namespace FinancialDataAnalysisTool.Models;
public class Dividends
{
    public string? Symbol { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}