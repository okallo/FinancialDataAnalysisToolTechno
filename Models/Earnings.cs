namespace FinancialDataAnalysisTool.Models;
public class Earnings
{
    public string? Symbol { get; set; }
    public DateTime Date { get; set; }
    public int Quarter { get; set; }
    public decimal EpsEstimate { get; set; }
    public decimal Eps { get; set; }
    public TimeSpan ReleaseTime { get; set; }
}