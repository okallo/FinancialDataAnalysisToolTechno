using FinancialDataAnalysisTool.Models;
using OfficeOpenXml;

namespace FinancialDataAnalysisTool.FinancialData;
public class EarningsData
{
    private readonly Plugins _plugins = new Plugins();

    public async Task<List<Earnings>> LoadEarnings(MasterDataPath masterDataPath)
    {
        var earnings = new List<Earnings>();
        await Task.Run(() =>
   {
       using (var package = new ExcelPackage(new FileInfo(masterDataPath.MasterPath)))
       {
           var sheet = package.Workbook.Worksheets["earnings"];
           var rowCount = sheet.Dimension.Rows;

           for (int row = 2; row <= rowCount; row++)
           {
               var preQuarter = int.TryParse(sheet.Cells[row, 3].Value?.ToString(), out var openResult) ? openResult : 0;
               var preRelease = TimeSpan.TryParse(sheet.Cells[row, 6].Value?.ToString(), out var res) ? res : TimeSpan.FromHours(1);
               string symbol = sheet.Cells[row, 1].Value?.ToString() ?? "N/A";
               DateTime date = _plugins.FixDate(sheet.Cells[row, 2].Value?.ToString() ?? "N/A");
               int quarter = preQuarter;
               decimal epsEstimate = _plugins.CheckValue(sheet.Cells[row, 4].Value?.ToString());
               decimal eps = _plugins.CheckValue(sheet.Cells[row, 5].Value?.ToString());
               TimeSpan releaseTime = preRelease;

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
   });



        return earnings;
    }
}