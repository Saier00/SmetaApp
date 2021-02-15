using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ClosedXML.Excel;

namespace SmetaApp.ExcelParsing
{
    //ClosedXML workbook interface and related interface realization
    public class DOMWorkbook : IWorkbook
    {
        XLWorkbook workbook;

        public DOMWorkbook(string FilePath)
        {
            workbook = new XLWorkbook(FilePath, XLEventTracking.Disabled);
        }

        public void Dispose()
        {
            if (workbook != null)
                workbook.Dispose();
        }

        public IEnumerator GetEnumerator()
        {
            IEnumerable<DOMSheet> sheets = workbook.Worksheets.Select(ws => new DOMSheet(ws));
            return sheets.GetEnumerator();
        }

        public bool AnySheets()
        {
            return workbook.Worksheets.Any();
        }
    }
    public class DOMSheet : ISheet
    {
        IXLWorksheet worksheet;
        public DOMSheet(IXLWorksheet ws)
        {
            worksheet = ws;
        }

        public bool IsEmpty()
        {
            return worksheet.FirstColumnUsed() == null;
        }

        public ICell Cell(int row, int col)
        {
            return new DOMCell(worksheet.Cell(row, col));
        }

        public int FirstColumnUsed()
        {
            return worksheet.FirstColumnUsed().ColumnNumber();
        }

        public int FirstRowUsed()
        {
            return worksheet.FirstRowUsed().RowNumber();
        }

        public int LastColumnUsed()
        {
            return worksheet.LastColumnUsed().ColumnNumber();
        }

        public int LastRowUsed()
        {
            return worksheet.LastRowUsed().RowNumber();
        }
    }
    public class DOMCell : ICell
    {
        IXLCell cell;
        public DOMCell(IXLCell c)
        {
            cell = c;
        }
        public bool Check()
        {
            return cell.GetValue<string>() == "";
        }

        public string GetValue()
        {
            return cell.GetValue<string>();
        }
    }
}