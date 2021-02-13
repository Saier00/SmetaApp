using System;
using System.Collections;

namespace SmetaApp.ExcelParsing
{
    public interface IJobParsing
    {
        JobProxy[] Parse(IWorkbook wb);
    }

    public interface IWorkbook:IEnumerable,IDisposable
    {
        bool AnySheets();
    }
    public interface ISheet
    {
        bool IsEmpty();
        int FirstColumnUsed();
        int LastColumnUsed();
        int FirstRowUsed();
        int LastRowUsed();
        ICell Cell(int row, int col);
    }
    public interface ICell
    {
        string GetValue();
        bool Check();
    }

    public class SyntaxError : Exception
    {
        public SyntaxError(string message)
            : base(message)
        { }
    }
}
