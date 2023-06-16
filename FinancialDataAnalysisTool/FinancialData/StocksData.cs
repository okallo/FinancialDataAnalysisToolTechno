using FinancialDataAnalysisTool.Models;
using OfficeOpenXml;

namespace FinancialDataAnalysisTool.FinancialData;
public class StockPricesData
{
    private readonly Plugins _plugins = new Plugins();

    public async Task<List<StockPrice>> LoadStockPrices(MasterDataPath masterDataPath)
    {
        var stockPrices = new List<StockPrice>();
        await Task.Run(() =>
   {
       using (var package = new ExcelPackage(new FileInfo(masterDataPath.MasterPath)))
       {
           var sheet = package.Workbook.Worksheets["stock_prices_latest"];
           var rowCount = sheet.Dimension.Rows;

           for (int row = 2; row <= rowCount; row++)
           {
               // var dd = sheet.Cells[row, 2].Value?.ToString();
               // var d = double.TryParse(dd, out double numericDate) ? numericDate : DateTime.Now.ToOADate();

               var preVolume = int.TryParse(sheet.Cells[row, 8].Value?.ToString(), out var openResult) ? openResult : 0;
               var symbol = sheet.Cells[row, 1].Value?.ToString() ?? "N/A";
               var date = _plugins.FixDate(sheet.Cells[row, 2].Value?.ToString() ?? "N/A");
               var open = _plugins.CheckValue(sheet.Cells[row, 3].Value?.ToString());
               var high = _plugins.CheckValue(sheet.Cells[row, 4].Value?.ToString());
               var low = _plugins.CheckValue(sheet.Cells[row, 5].Value?.ToString());
               var close = _plugins.CheckValue(sheet.Cells[row, 6].Value?.ToString());
               var closeAdjusted = _plugins.CheckValue(sheet.Cells[row, 7].Value?.ToString());
               var volume = preVolume;
               var splitCoefficient = _plugins.CheckValue(sheet.Cells[row, 9].Value?.ToString());

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

   });


        return stockPrices;
    }
    public async Task<List<string>> LoadSymbols(MasterDataPath masterDataPath)
    {

        var symbols = new List<string>();
        await Task.Run(() =>
        {
            using (var package = new ExcelPackage(new FileInfo(masterDataPath.MasterPath)))
            {
                var sheet = package.Workbook.Worksheets["stock_prices_latest"];

                if (sheet != null && sheet.Dimension != null)
                {
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


            }
        });
        return symbols;
    }

}