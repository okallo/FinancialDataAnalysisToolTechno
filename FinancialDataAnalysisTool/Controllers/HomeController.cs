using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using FinancialDataAnalysisTool.Models;
using OfficeOpenXml;
using System.Linq;
using System.Text.Json;
using System.Globalization;
using FinancialDataAnalysisTool.FinancialData;
using FinancialDataAnalysisTool.FinancialFormulars;
using System.Drawing;
using System.Drawing.Imaging;
using MathNet.Numerics.Statistics;

namespace FinancialDataAnalysisTool.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MasterDataPath masterDataPath = new MasterDataPath()
    {
        MasterPath = "./stock_prices_latest.xlsx"
    };
    private readonly StockPricesData stocksData = new StockPricesData();
    private readonly FinancialReturns fReturns = new FinancialReturns();
    private readonly FinancialVolatility fVolatility = new FinancialVolatility();
    private readonly FinancialCorrelations fCorrelations = new FinancialCorrelations();

    private readonly DividendsData dividendsData = new DividendsData();
    private readonly EarningsData earningsData = new EarningsData();
    private readonly Plugins plugins = new Plugins();


    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DataViewModel();
        model.Symbols =await stocksData.LoadSymbols(masterDataPath);
        model.Dividends =await dividendsData.LoadDividends(masterDataPath);
        model.Earnings =await earningsData.LoadEarnings(masterDataPath);
        model.StockPrices =await stocksData.LoadStockPrices(masterDataPath);
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    [HttpGet]
    public IActionResult LoadStockPricesData()
    {
        var stockPrices = stocksData.LoadStockPrices(masterDataPath); // Implement this method to retrieve stock prices data
        return Json(stockPrices);
    }

    [HttpPost]
    public IActionResult CalculateReturn(string symbol)
    {
        var stockPrices = stocksData.LoadStockPrices(masterDataPath);
        var returns = fReturns.CalculateReturns(stockPrices.Result, symbol);
        return Json(returns);
    }

    [HttpPost]
    public IActionResult CalculateVolatility(string symbol)
    {
        var stockPrices = stocksData.LoadStockPrices(masterDataPath);
        var volatility = fVolatility.CalculateVolatility(stockPrices.Result, symbol);
        return Json(volatility);
    }

    [HttpPost]
    public IActionResult FilterByTime(string symbol, string startDate, string endDate)
    {
        var stockPrices = stocksData.LoadStockPrices(masterDataPath);
        var filteredData = plugins.FilterDataByTime(stockPrices.Result, symbol, startDate, endDate);
        return Json(filteredData);
    }

    [HttpPost]
    public IActionResult FilterBySymbol(string[] symbols)
    {
        var stockPrices = stocksData.LoadStockPrices(masterDataPath);
        var filteredData = plugins.FilterDataBySymbol(stockPrices.Result, symbols);
        return Json(filteredData);
    }

    public IActionResult RenderChart()
    {
        var stockPrices = stocksData.LoadStockPrices(masterDataPath);
        var chartData = plugins.PrepareChartData(stockPrices.Result);
        var serializedData = JsonSerializer.Serialize(chartData);
        ViewData["ChartData"] = serializedData;
        Console.WriteLine("***bugs** "+ serializedData);
        return PartialView("_Chart");
    }

}

