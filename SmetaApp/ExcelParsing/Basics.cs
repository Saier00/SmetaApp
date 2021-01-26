using System;
using System.Collections.Generic;
using ClosedXML.Excel;

namespace SmetaApp.ExcelParsing
{
    public class SyntaxError : Exception
    {
        public SyntaxError(string message)
            : base(message)
        { }
    }

    public class JobProxy
    {
        public JobProxy()
        {
            Mechs = new List<MechProxy>();
            Mats = new List<MatProxy>();
        }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string Measurer { get; set; }
        public string Units { get; set; }
        public string WLaborCosts { get; set; }
        public string AvRank { get; set; }
        public string MLaborCosts { get; set; }
        public string Mass { get; set; }
        public ICollection<MechProxy> Mechs { get; set; }
        public ICollection<MatProxy> Mats { get; set; }
    }

    public class MechProxy
    {
        public string Code { get; set; }
        public string Amount { get; set; }
        public string Name { get; set; }
    }
    public class MatProxy
    {
        public string Code { get; set; }
        public string Units { get; set; }
        public string Name { get; set; }
        public string Amount { get; set; }
    }

    public class RowMat : MatProxy
    {
        public int row { get; set; }
    }
    public class RowMech : MechProxy
    {
        public int row { get; set; }
    }
    public interface IJobParsing
    {
        JobProxy[] Parse(XLWorkbook wb);
    }
}
