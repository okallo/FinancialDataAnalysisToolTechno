namespace FinancialDataAnalysisTool.Models;using System;
using System.ComponentModel.DataAnnotations;
public class StockPrice
{
    public string Symbol { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal CloseAdjusted { get; set; }
    public int Volume { get; set; }
    public decimal SplitCoefficient { get; set; }
}