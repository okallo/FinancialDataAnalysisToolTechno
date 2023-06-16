using FinancialDataAnalysisTool.Models;
using OfficeOpenXml;

namespace FinancialDataAnalysisTool.FinancialData;
public class DividendsData
{
    private readonly Plugins _plugins = new Plugins();
      public async Task< List<Dividends>> LoadDividends(MasterDataPath masterDataPath)
    {
        List<Dividends> dividendsList = new List<Dividends>();
         await Task.Run(() =>
    {
        using (var package = new ExcelPackage(new FileInfo(masterDataPath.MasterPath)))
        {
            var sheet = package.Workbook.Worksheets["dividends"];
            var rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++) 
            {
                string symbol = sheet.Cells[row, 1].Value?.ToString() ?? "N/A";
                DateTime date = _plugins.FixDate(sheet.Cells[row, 2].Value?.ToString()??"N/A");
                decimal amount = _plugins.CheckValue(sheet.Cells[row, 3].Value?.ToString());

                Dividends dividend = new Dividends
                {
                    Symbol = symbol,
                    Date = date,
                    Amount = amount
                };

                dividendsList.Add(dividend);
            }
        }
    });
       

        return dividendsList;
    }
   
}