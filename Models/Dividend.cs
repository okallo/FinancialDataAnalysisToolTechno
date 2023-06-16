namespace FinancialDataAnalysisTool.Models;
public class Dividend
{
    public string? Symbol { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}