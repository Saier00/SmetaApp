using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SmetaApp.ExcelParsing
{
    //OpenXML workbook interface and related interface realization
    public class SAXWorkbook : IWorkbook
    {
        SpreadsheetDocument spreadsheetDocument;

        public SAXWorkbook(string FilePath)
        {
            spreadsheetDocument = SpreadsheetDocument.Open(FilePath, false);
        }

        public void Dispose()
        {
            if (spreadsheetDocument != null)
                spreadsheetDocument.Dispose();
        }

        public IEnumerator GetEnumerator()
        {
            IEnumerable<SAXSheet> sheets = spreadsheetDocument.WorkbookPart.Workbook.Descendants<Sheet>().Select(ws => new SAXSheet(
                ((WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(ws.Id)).Worksheet,
                spreadsheetDocument
                ));
            return sheets.GetEnumerator();
        }

        public bool AnySheets()
        {
            return spreadsheetDocument.WorkbookPart.Workbook.Descendants<Sheet>().Any();
        }
    }
    public class SAXSheet : ISheet
    {
        Worksheet worksheet;
        SpreadsheetDocument spreadsheetDocument;
        public SAXSheet(Worksheet ws, SpreadsheetDocument sd)
        {
            worksheet = ws;
            spreadsheetDocument = sd;
        }

        public bool IsEmpty()
        {
            return !worksheet.GetFirstChild<SheetData>().Descendants<Row>().Any();
        }

        public ICell Cell(int row, int col)
        {
            return new SAXCell(GetCell(worksheet, row, col),spreadsheetDocument);
        }

        public int FirstColumnUsed()
        {
            return 0;
        }

        public int FirstRowUsed()
        {
            int fr = (int)worksheet.Descendants<Row>().FirstOrDefault().RowIndex.Value-1;
            return fr>=0?fr:0;
        }

        public int LastColumnUsed()
        {
            return worksheet.Descendants<Row>().Max(
                r => r.Elements<Cell>().Count()
                );
        }

        public int LastRowUsed()
        {
            return (int)worksheet.Descendants<Row>().LastOrDefault().RowIndex.Value-1;
        }

        private Cell GetCell(Worksheet worksheet, int rowIndex, int columnIndex)
        {
            rowIndex++;
            columnIndex++;
            Row row = worksheet.Descendants<Row>().FirstOrDefault(r => r.RowIndex == rowIndex);

            if (row == null)
                return null;

            return row.Elements<Cell>().FirstOrDefault(c => c.CellReference.Value == GetExcelColumnName(columnIndex) + rowIndex);
        }
        private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
    }
    public class SAXCell : ICell
    {
        Cell cell;

        bool valueSet = false;
        string value;

        SpreadsheetDocument spreadsheetDocument;
        public SAXCell(Cell c, SpreadsheetDocument sd)
        {
            cell = c;
            spreadsheetDocument = sd;
        }
        public bool Check()
        {
            return GetStringValue() == string.Empty;
        }

        public string GetValue()
        {
            return GetStringValue();
        }

        private string GetStringValue()
        {
            if (!valueSet)
            {
                string cellValue = string.Empty;
                if (cell != null)
                {
                    cellValue = cell.InnerText;
                    if (cell.DataType != null)
                    {
                        switch (cell.DataType.Value)
                        {
                            case CellValues.SharedString:

                                int id = -1;

                                if (Int32.TryParse(cell.InnerText, out id))
                                {
                                    SharedStringItem item = spreadsheetDocument.WorkbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);

                                    if (item.InnerText != null)
                                    {
                                        cellValue = item.InnerText;
                                    }
                                }
                                break;

                            case CellValues.Boolean:
                                switch (cellValue)
                                {
                                    case "0":
                                        cellValue = "FALSE";
                                        break;
                                    default:
                                        cellValue = "TRUE";
                                        break;
                                }
                                break;
                        }
                    }
                }

                value = cellValue;
                valueSet = true;

                return cellValue;
            }
            else
            {
                return value;
            }
        }
    }
}