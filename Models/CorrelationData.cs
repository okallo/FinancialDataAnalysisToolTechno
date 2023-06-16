namespace FinancialDataAnalysisTool.Models;
public class CorrelationData
{
    public string? SymbolA { get; set; }
    public string? SymbolB { get; set; }
    public decimal Correlation { get; set; }
}