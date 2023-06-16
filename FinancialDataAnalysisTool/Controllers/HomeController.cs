using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using FinancialDataAnalysisTool.Models;
using OfficeOpenXml;
using System.Linq;
using System.Text.Json;

namespace FinancialDataAnalysisTool.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly string stockPricesPath = "./stock_prices.xlsx";
    private readonly string dividendsPath = "./dividents.xlsx";
    private readonly string earningsPath = "./earnings.xlsx";

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var model = new DataViewModel();
        model.Symbols = LoadSymbols();
        return View(model);
    }

    public IActionResult PlotChart()
    {
        var stockPrices = LoadStockPrices();
        var symbols = stockPrices.Select(s => s.Symbol).Distinct().ToList();

        var volatilityData = new List<ChartData>();
        var correlationData = new List<ChartData>();
        var returnsData = new List<ChartData>();

        foreach (var symbol in symbols)
        {
            var returns = CalculateReturns(stockPrices, symbol);
            var volatility = CalculateVolatility(stockPrices, symbol);
            var correlations = CalculateCorrelations(stockPrices, symbols.ToArray());

            var returnsChartData = new ChartData
            {
                Symbol = symbol,
                DataPoints = returns.Select(r => new DataPoint(r.Date.ToString("yyyy-MM-dd"), (double)r.Return)).ToList()
            };
            List<DataPoint> data = new List<DataPoint>();
             DataPoint d = new DataPoint("",volatility)
             {
                    Date = "",
                    Value = volatility
                    };
                    data.Add(d);
            var volatilityChartData = new ChartData
            {
                Symbol = symbol,
                DataPoints = data
               //.Select(v => new DataPoint(v.Date.ToString("yyyy-MM-dd"), (double)v.Value)).ToList()
            };
            var correlationChartData = new ChartData
            {
                Symbol = symbol,
                DataPoints = correlations
                    .Where(c => c.SymbolA == symbol)
                    .Select(c => new DataPoint(c.SymbolA, (double)c.Correlation))
                    .ToList()
            };

            returnsData.Add(returnsChartData);
            volatilityData.Add(volatilityChartData);
            correlationData.Add(correlationChartData);
        }

        var chartData = new ChartDataViewModel
        {
            VolatilityData = volatilityData,
            CorrelationData = correlationData,
            ReturnsData = returnsData
        };

        return View(chartData);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


    [HttpPost]
    public IActionResult CalculateReturn(string symbol)
    {
        var stockPrices = LoadStockPrices();
        var returns = CalculateReturns(stockPrices, symbol);
        return Json(returns);
    }

    [HttpPost]
    public IActionResult CalculateVolatility(string symbol)
    {
        var stockPrices = LoadStockPrices();
        var volatility = CalculateVolatility(stockPrices, symbol);
        return Json(volatility);
    }

    [HttpPost]
    public IActionResult CalculateCorrelations(string[] symbols)
    {
        var stockPrices = LoadStockPrices();
        var correlations = CalculateCorrelations(stockPrices, symbols);
        return Json(correlations);
    }

    [HttpPost]
    public IActionResult FilterByTime(string symbol, string startDate, string endDate)
    {
        var stockPrices = LoadStockPrices();
        var filteredData = FilterDataByTime(stockPrices, symbol, startDate, endDate);
        return Json(filteredData);
    }

    [HttpPost]
    public IActionResult FilterBySymbol(string[] symbols)
    {
        var stockPrices = LoadStockPrices();
        var filteredData = FilterDataBySymbol(stockPrices, symbols);
        return Json(filteredData);
    }

    public IActionResult RenderChart()
    {
        var stockPrices = LoadStockPrices();
        var chartData = PrepareChartData(stockPrices);
        var serializedData = JsonSerializer.Serialize(chartData);
        ViewData["ChartData"] = serializedData;
        return PartialView("_Chart");
    }
    private double CalculateVolatility(List<StockPrice> stockPrices, string symbol)
    {
        // Calculate volatility for the given symbol
        var prices = stockPrices.Where(s => s.Symbol == symbol).Select(s => (double)s.Close).ToList();
        var logReturns = new List<double>();

        for (int i = 1; i < prices.Count; i++)
        {
            var logReturn = Math.Log(prices[i] / prices[i - 1]);
            logReturns.Add(logReturn);
        }

        var mean = logReturns.Average();
        var squaredDeviations = logReturns.Select(x => Math.Pow(x - mean, 2));
        var variance = squaredDeviations.Average();
        var volatility = Math.Sqrt(variance);

        return volatility;
    }

    private List<CorrelationData> CalculateCorrelations(List<StockPrice> stockPrices, string[] symbols)
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

    private List<string> LoadSymbols()
    {
        var symbols = new List<string>();

        using (var package = new ExcelPackage(new FileInfo(stockPricesPath)))
        {
            var sheet = package.Workbook.Worksheets["stock_prices"];
            // Get all worksheets in the workbook
            // var worksheets = package.Workbook.Worksheets;

            // // Iterate over each worksheet and retrieve the name
            // foreach (var worksheet in worksheets)
            // {
            //     var worksheetName = worksheet.Name;
            //     // Do something with the worksheet name
            //     Console.WriteLine(" reeding worksheet  ' " + worksheetName);
            // }

            if (sheet.Dimension == null)
            {
                // Handle the case where the worksheet is empty
                // or the data range is not set
                return symbols;
            }

            var rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var symbol = sheet.Cells[row, 1].Value?.ToString();

                if (!string.IsNullOrEmpty(symbol) && !symbols.Contains(symbol))
                {
                    Console.WriteLine(" reeding worksheet symbols ' " + symbol);
                    symbols.Add(symbol);
                }
            }
        }
        return symbols;
    }


    // private List<string> LoadSymbols()
    // {
    //     // Load symbols from stock_prices sheet
    //     var symbols = new List<string>();
    //     using (var package = new ExcelPackage(new FileInfo(stockPricesPath)))
    //     {
    //         var sheet = package.Workbook.Worksheets["stock_prices"];
    //          Console.WriteLine("Check!  @");
    //         var rowCount = sheet.Dimension.Rows;
    //         for (int i = 2; i <= rowCount; i++)
    //         {
    //             var symbol = sheet.Cells[i, 1].Value?.ToString();
    //             if (!string.IsNullOrEmpty(symbol) && !symbols.Contains(symbol))
    //             {
    //                 symbols.Add(symbol);
    //             }
    //         }
    //     }
    //     return symbols;
    // }

    private List<StockPrice> LoadStockPrices()
    {
        // Load stock prices from stock_prices sheet

        var stockPrices = new List<StockPrice>();
        var stock = new StockPrice();
        try
        {

            // using (var package = new ExcelPackage(new FileInfo(stockPricesPath)))
            // {
            //     var sheet = package.Workbook.Worksheets["stock_prices"];
            //     var rowCount = sheet.Dimension.Rows;
            //     for (int i = 2; i <= rowCount; i++)
            //     {
            //         var stockPrice = new StockPrice
            //         {
            //             Symbol = sheet.Cells[i, 1].Value?.ToString(),
            //             Date = DateTime.Parse(sheet.Cells[i, 2].Value?.ToString()),
            //             Open = decimal.Parse(sheet.Cells[i, 3].Value?.ToString()),
            //             High = decimal.Parse(sheet.Cells[i, 4].Value?.ToString()),
            //             Low = decimal.Parse(sheet.Cells[i, 5].Value?.ToString()),
            //             Close = decimal.Parse(sheet.Cells[i, 6].Value?.ToString()),
            //             CloseAdjusted = decimal.Parse(sheet.Cells[i, 7].Value?.ToString()),
            //             Volume = int.Parse(sheet.Cells[i, 8].Value?.ToString()),
            //             SplitCoefficient = decimal.Parse(sheet.Cells[i, 9].Value?.ToString())
            //         };

            //         stockPrices.Add(stockPrice);
            //     }
            // }


            using (var package = new ExcelPackage(new FileInfo(stockPricesPath)))
            {
                var sheet = package.Workbook.Worksheets["stock_prices"];

                var rowCount = sheet.Dimension.Rows;
                for (int i = 2; i <= rowCount; i++)
                {
                    var symbol = sheet.Cells[i, 1].Value?.ToString();
                    var dateValue = sheet.Cells[i, 2].Value;
                    var openValue = sheet.Cells[i, 3].Value;
                    var highValue = sheet.Cells[i, 4].Value;
                    var lowValue = sheet.Cells[i, 5].Value;
                    var closeValue = sheet.Cells[i, 6].Value;
                    var closeAdjustedValue = sheet.Cells[i, 7].Value;
                    var volumeValue = sheet.Cells[i, 8].Value;
                    var splitCoefficientValue = sheet.Cells[i, 9].Value;

                    // Skip the row if any of the required values are null or cannot be parsed
                    if (string.IsNullOrEmpty(symbol) ||
                        dateValue == null ||
                        openValue == null ||
                        highValue == null ||
                        lowValue == null ||
                        closeValue == null ||
                        closeAdjustedValue == null ||
                        volumeValue == null ||
                        splitCoefficientValue == null)
                    {
                        continue;
                    }
                    // var stockPrice = new StockPrice();
                    // if(dateValue.ToString() == "36528"){
                    //     stockPrice  = new StockPrice{
                    //         Symbol = symbol,
                    //     Date = DateTime.Parse("01/01/1900"),
                    //     Open = decimal.Parse(openValue.ToString()),
                    //     High = decimal.Parse(highValue.ToString()),
                    //     Low = decimal.Parse(lowValue.ToString()),
                    //     Close = decimal.Parse(closeValue.ToString()),
                    //     CloseAdjusted = decimal.Parse(closeAdjustedValue.ToString()),
                    //     Volume = int.Parse(volumeValue.ToString()),
                    //     SplitCoefficient = decimal.Parse(splitCoefficientValue.ToString())
                    //     } ;
                    //     //stock = faultyStock;
                    // }
                    // else {
                    var stockPrice = new StockPrice
                    {
                        Symbol = symbol,
                        Date = DateTime.Parse(dateValue.ToString()),
                        Open = decimal.Parse(openValue.ToString()),
                        High = decimal.Parse(highValue.ToString()),
                        Low = decimal.Parse(lowValue.ToString()),
                        Close = decimal.Parse(closeValue.ToString()),
                        CloseAdjusted = decimal.Parse(closeAdjustedValue.ToString()),
                        Volume = int.Parse(volumeValue.ToString()),
                        SplitCoefficient = decimal.Parse(splitCoefficientValue.ToString())
                    };

                    // Parse the values and create a StockPrice object

                    Console.WriteLine(stockPrice);
                    stockPrices.Add(stockPrice);
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine("Symbol: " + stock.Symbol + "Date: " + stock.Date);
        }
        return stockPrices;
    }

    // Implement other data loading methods for dividends and earnings

    private List<ReturnData> CalculateReturns(List<StockPrice> stockPrices, string symbol)
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


    private List<ChartData> PrepareChartData(List<StockPrice> stockPrices)
    {
        // Prepare chart data for visualization
        var chartData = new List<ChartData>();
        foreach (var symbol in stockPrices.Select(s => s.Symbol).Distinct())
        {
            var prices = stockPrices.Where(s => s.Symbol == symbol).OrderBy(s => s.Date).ToList();
            var dataPoints = prices.Select(p => new DataPoint(p.Date.ToString("yyyy-MM-dd"), (double)p.Close)).ToList();
            var chartDataset = new ChartData
            {
                Symbol = symbol,
                DataPoints = dataPoints
            };
            chartData.Add(chartDataset);
        }
        return chartData;
    }

    private List<StockPrice> FilterDataByTime(List<StockPrice> stockPrices, string symbol, string startDate, string endDate)
    {
        // Filter data by symbol, start date, and end date
        var filteredData = stockPrices.Where(s =>
            s.Symbol == symbol &&
            s.Date >= DateTime.Parse(startDate) &&
            s.Date <= DateTime.Parse(endDate)
        ).ToList();
        return filteredData;
    }

    private List<StockPrice> FilterDataBySymbol(List<StockPrice> stockPrices, string[] symbols)
    {
        // Filter data by symbols
        var filteredData = stockPrices.Where(s => symbols.Contains(s.Symbol)).ToList();
        return filteredData;
    }

}

