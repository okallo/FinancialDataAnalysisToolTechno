using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using FinancialDataAnalysisTool.Models;
using OfficeOpenXml;
using System.Linq;
using System.Text.Json;
using System.Globalization;
using FinancialDataAnalysisTool.FinancialData;
using FinancialDataAnalysisTool.FinancialFormulars;

namespace FinancialDataAnalysisTool.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MasterDataPath masterDataPath = new MasterDataPath(){
        MasterPath = "./stock_prices_latest.xlsx"
    };
    private readonly StockPricesData stocksData = new StockPricesData();
    private readonly FinancialReturns fReturns = new FinancialReturns();
    private readonly FinancialVolatility fVolatility = new FinancialVolatility();
    private readonly FinancialCorrelations fCorrelations = new FinancialCorrelations();

    private readonly Plugins plugins = new Plugins();
    

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var model = new DataViewModel();
        model.Symbols = stocksData.LoadSymbols(masterDataPath);
        
        model.StockPrices = stocksData.LoadStockPrices(masterDataPath);
        return View(model);
    }

    public IActionResult PlotChart()
    {
        var stockPrices = stocksData.LoadStockPrices(masterDataPath);
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
            var returns = fReturns.CalculateReturns(stockPrices, symbol);
            var volatility = fVolatility.CalculateVolatility(stockPrices, symbol);
            var correlations = fCorrelations.CalculateCorrelations(stockPrices, symbols.ToArray());

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
        var stockPrices =stocksData.LoadStockPrices(masterDataPath); // Implement this method to retrieve stock prices data
        return Json(stockPrices);
    }

    [HttpPost]
    public IActionResult CalculateReturn(string symbol)
    {
        var stockPrices = stocksData.LoadStockPrices(masterDataPath);
        var returns = fReturns.CalculateReturns(stockPrices, symbol);
        return Json(returns);
    }

    [HttpPost]
    public IActionResult CalculateVolatility(string symbol)
    {
        var stockPrices = stocksData.LoadStockPrices(masterDataPath);
        var volatility = fVolatility.CalculateVolatility(stockPrices, symbol);
        return Json(volatility);
    }

    [HttpPost]
    public IActionResult CalculateCorrelations(string[] symbols)
    {
        var stockPrices = stocksData.LoadStockPrices(masterDataPath);
        var correlations = fCorrelations.CalculateCorrelations(stockPrices, symbols);
        return Json(correlations);
    }

    [HttpPost]
    public IActionResult FilterByTime(string symbol, string startDate, string endDate)
    {
        var stockPrices = stocksData.LoadStockPrices(masterDataPath);
        var filteredData = plugins.FilterDataByTime(stockPrices, symbol, startDate, endDate);
        return Json(filteredData);
    }

    [HttpPost]
    public IActionResult FilterBySymbol(string[] symbols)
    {
        var stockPrices = stocksData.LoadStockPrices(masterDataPath);
        var filteredData = plugins.FilterDataBySymbol(stockPrices, symbols);
        return Json(filteredData);
    }

    public IActionResult RenderChart()
    {
        var stockPrices = stocksData.LoadStockPrices(masterDataPath);
        var chartData = plugins.PrepareChartData(stockPrices);
        var serializedData = JsonSerializer.Serialize(chartData);
        ViewData["ChartData"] = serializedData;
        return PartialView("_Chart");
    }    
}

