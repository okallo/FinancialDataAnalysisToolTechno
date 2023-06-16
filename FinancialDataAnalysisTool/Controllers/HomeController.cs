using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using FinancialDataAnalysisTool.Models;
using OfficeOpenXml;
using System.Linq;
using System.Text.Json;
using System.Globalization;

namespace FinancialDataAnalysisTool.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly string stockPricesPath = "./stock_prices.xlsx";
    private readonly string dividendsPath = "./dividents.xlsx";
    private readonly string earningsPath = "./earnings.xlsx";
    private readonly string masterDataPath = "./stock_prices_latest.xlsx";

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var model = new DataViewModel();
        model.Symbols = LoadSymbols();
        model.StockPrices = LoadStockPrices();
        return View(model);
    }

    public IActionResult PlotChart()
    {
        var stockPrices = LoadStockPrices();
        foreach (var c in stockPrices)
        {
            Console.WriteLine(c.Date);

        }
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
            DataPoint d = new DataPoint("", volatility)
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
                    .Select(c => new DataPoint(c.SymbolA ?? symbol, (double)c.Correlation))
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
    [HttpGet]
    public IActionResult LoadStockPricesData()
    {
        var stockPrices = LoadStockPrices(); // Implement this method to retrieve stock prices data
        return Json(stockPrices);
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
                    symbols.Add(symbol);
                }
            }
        }
        return symbols;
    }
    // Implement other data loading methods for dividends and earnings
    private List<Earnings> LoadEarnings()
    {
        var earnings = new List<Earnings>();

        using (var package = new ExcelPackage(new FileInfo(stockPricesPath)))
        {
            var sheet = package.Workbook.Worksheets["earnings"];
            var rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var preQuarter = int.TryParse(sheet.Cells[row, 3].Value?.ToString(), out var openResult) ? openResult : 0;
                var preRelease = TimeSpan.TryParse(sheet.Cells[row, 6].Value?.ToString(), out var res)? res:TimeSpan.FromHours(1);
                string symbol = sheet.Cells[row, 1].Value?.ToString() ?? "N/A";
                DateTime date = FixDate(sheet.Cells[row, 2].Value?.ToString()??"N/A");
                int quarter = preQuarter;
                decimal epsEstimate = CheckValue(sheet.Cells[row, 4].Value?.ToString());
                decimal eps = CheckValue(sheet.Cells[row, 5].Value?.ToString());
                TimeSpan releaseTime= preRelease;

                var earning = new Earnings
                {
                    Symbol = symbol,
                    Date = date,
                    Quarter = quarter,
                    EpsEstimate = epsEstimate,
                    Eps = eps,
                    ReleaseTime = releaseTime
                };

                earnings.Add(earning);
            }
        }

        return earnings;
    }
    private List<Dividends> LoadDividends()
    {
        List<Dividends> dividendsList = new List<Dividends>();

        using (var package = new ExcelPackage(new FileInfo(masterDataPath)))
        {
            var sheet = package.Workbook.Worksheets["dividends"];
            var rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++) // Assuming data starts from the second row
            {
                string symbol = sheet.Cells[row, 1].Value?.ToString() ?? "N/A";
                DateTime date = FixDate(sheet.Cells[row, 2].Value?.ToString()??"N/A");
                decimal amount = CheckValue(sheet.Cells[row, 3].Value?.ToString());

                Dividends dividend = new Dividends
                {
                    Symbol = symbol,
                    Date = date,
                    Amount = amount
                };

                dividendsList.Add(dividend);
            }
        }

        return dividendsList;
    }
    private List<StockPrice> LoadStockPrices()
    {
        var stockPrices = new List<StockPrice>();

        using (var package = new ExcelPackage(new FileInfo(masterDataPath)))
        {
            var sheet = package.Workbook.Worksheets["stock_prices_latest"];
            var rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                // var dd = sheet.Cells[row, 2].Value?.ToString();
                // var d = double.TryParse(dd, out double numericDate) ? numericDate : DateTime.Now.ToOADate();

                var preVolume = int.TryParse(sheet.Cells[row, 8].Value?.ToString(), out var openResult) ? openResult : 0;
                var symbol = sheet.Cells[row, 1].Value?.ToString() ?? "N/A";
                var date = FixDate(sheet.Cells[row, 2].Value?.ToString()??"N/A");
                var open = CheckValue(sheet.Cells[row, 3].Value?.ToString());
                var high = CheckValue(sheet.Cells[row, 4].Value?.ToString());
                var low = CheckValue(sheet.Cells[row, 5].Value?.ToString());
                var close = CheckValue(sheet.Cells[row, 6].Value?.ToString());
                var closeAdjusted = CheckValue(sheet.Cells[row, 7].Value?.ToString());
                var volume = preVolume;
                var splitCoefficient = CheckValue(sheet.Cells[row, 9].Value?.ToString());

                var stockPrice = new StockPrice
                {
                    Symbol = symbol,
                    Date = date,
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close,
                    CloseAdjusted = closeAdjusted,
                    Volume = volume,
                    SplitCoefficient = splitCoefficient
                };

                stockPrices.Add(stockPrice);
            }
        }

        return stockPrices;
    }
    private decimal CheckValue(string? v)
    {
        var openValue = v;
        return decimal.TryParse(openValue, out var openResult) ? openResult : 0;

    }

    private DateTime FixDate(string date){
        var d =double.TryParse(date, out double numericDate) ? numericDate : DateTime.Now.ToOADate();
        return DateTime.FromOADate(d);
    }

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

