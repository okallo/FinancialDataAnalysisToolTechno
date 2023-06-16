namespace FinancialDataAnalysisTool.Models;
public class ChartData
{
    public string Symbol { get; set; } = string.Empty;
    public List<DataPoint> DataPoints { get; set; } = new List<DataPoint>();

    
}

public class DataPoint
{
    public string Date { get; set; }
    public double Value { get; set; }

    public DataPoint(string date, double value)
    {
        Date = date;
        Value = value;
    }
}