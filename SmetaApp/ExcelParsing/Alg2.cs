using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmetaApp.ExcelParsing
{
    public class Alg2 : IJobParsing
    {
        private bool check(IXLCell c)
        {
            return c.GetValue<string>() == "";
        }
        public JobProxy[] Parse(XLWorkbook wb)
        {
            //Adapted client-side parsing code at server-side
            List<JobProxy> m = new List<JobProxy>();
            if (!wb.Worksheets.Any())
                throw new SyntaxError("Не найдено ни одного листа в документе");
            foreach (IXLWorksheet ws in wb.Worksheets)
            {
                if (ws.FirstColumnUsed() == null)
                    continue;
                int cs = ws.FirstColumnUsed().ColumnNumber();
                int ce = ws.LastColumnUsed().ColumnNumber();
                int rs = ws.FirstRowUsed().RowNumber();
                int re = ws.LastRowUsed().RowNumber();
                while (m.Count == 0 && cs <= ce)
                {
                    List<Task> TasksAfter = new List<Task>();

                    for (int a = rs; a <= re; a++)
                    {

                        var cell = ws.Cell(a, cs);

                        if (check(cell))
                        {
                            continue;
                        }

                        if (Regex.IsMatch(cell.GetValue<string>(), @"^\s*Измеритель"))
                        {
                            string measurer = null;
                            string units;
                            var mu = Regex.Split(Regex.Replace(cell.GetValue<string>(), @"^\s*Измеритель\S*\s+", ""), @"\s+");
                            if (mu.Length == 1)
                            {
                                units = mu[0];
                            }
                            else
                            {
                                measurer = mu[0];
                                units = mu[1];
                            }

                            Func<bool> incr = () =>
                            {
                                a++;
                                if (a > re)
                                    throw new SyntaxError("Неправильный формат загруженного файла:\n\tНе найденно ни одного совпадения после 'Измеритель' с: 'Код р'");
                                cell = ws.Cell(a, cs);
                                return check(cell);
                            };

                            //Try to find first not empty existing cell
                            while (incr()) { }
                            string name = null;
                            while (!Regex.IsMatch(cell.GetValue<string>(), @"^\s*Код р", RegexOptions.IgnoreCase))
                            {
                                if (Regex.IsMatch(cell.GetValue<string>(), @"(\d*-\d*-\d*-\d*)\s+[А-Я][а-яА-Я\s\w\,\.]*"))
                                {
                                    //[CODE][spaces][NAME]
                                    //or [CODE][spaces][NAME][spaces][CODE][spaces][NAME]...
                                    var matches = Regex.Matches(cell.GetValue<string>(), @"(\d*-\d*-\d*-\d*)\s+[А-Я][а-яА-Я\w\,\.]*(\s[а-яА-Я\w\,\.]+)*(?!\d*-\d*-\d*-\d*)");
                                    foreach (Match match in matches)
                                    {
                                        string mch = match.Value;
                                        JobProxy item = new JobProxy();
                                        item.Code = Regex.Match(mch, @"^(\d*-\d*-\d*-\d*)").Value;
                                        item.Name = Regex.Replace(mch, @"^(\d*-\d*-\d*-\d*)\s+", "");
                                        item.Units = units;
                                        item.Measurer = measurer;
                                        lock (m)
                                        {
                                            m.Add(item);
                                        }
                                    }
                                }
                                else if (Regex.IsMatch(cell.GetValue<string>(), @"^\s*[А-Я][а-яА-Я\s\w]*"))
                                {
                                    //[spaces][NAME]
                                    name = Regex.Replace(cell.GetValue<string>(), @"^\s+", "");
                                }
                                else if (Regex.IsMatch(cell.GetValue<string>(), @"(\d*-\d*-\d*-\d*)\s+[а-я\d\(\)][а-яА-Я\s\w\,\.]*"))
                                {
                                    //[CODE][spaces][TYPE]
                                    //name can be undefined
                                    var matches = Regex.Matches(cell.GetValue<string>(), @"(\d*-\d*-\d*-\d*)\s+[а-я\d\(\)][а-яА-Я\w\,\.]*(\s[а-яА-Я\w\,\.]+)*(?!\d*-\d*-\d*-\d*)");
                                    foreach (Match match in matches)
                                    {
                                        string mch = match.Value;
                                        JobProxy item = new JobProxy();
                                        item.Code = Regex.Match(mch, @"^(\d*-\d*-\d*-\d*)").Value;
                                        item.Type = Regex.Replace(mch, @"^(\d*-\d*-\d*-\d*)\s+", "");
                                        item.Name = name;
                                        item.Units = units;
                                        item.Measurer = measurer;
                                        lock (m)
                                        {
                                            m.Add(item);
                                        }
                                    }
                                }

                                while (incr()) { }
                            }

                            Action<int> parseTable = (i) =>
                            {
                                int? namesCol = null, mesCol = null;
                                //Try to find "Наим"
                                for (var k = cs + 1; k <= ce; k++)
                                {
                                    var namesColCell = ws.Cell(i, k);
                                    if (check(namesColCell))
                                        continue;

                                    if (Regex.IsMatch(namesColCell.GetValue<string>(), @"^\s*Наим", RegexOptions.IgnoreCase))
                                    {
                                        namesCol = k;
                                        break;
                                    }
                                }
                                if (namesCol == null)
                                    throw new SyntaxError("Неправильный формат загруженного файла:\n\tВ таблице отсутствует столбец содержащий в начале 'Наим':\n" + ws.Cell(i, ce).GetValue<string>());
                                //Try to find "изм"
                                for (var k = cs + 2; k <= ce; k++)
                                {
                                    if (namesCol == k && k != ce)
                                        continue;
                                    if (namesCol != k)
                                    {
                                        var mesColCell = ws.Cell(i, k);
                                        if (check(mesColCell))
                                            continue;
                                        if (Regex.IsMatch(mesColCell.GetValue<string>(), @"изм", RegexOptions.IgnoreCase))
                                        {
                                            mesCol = k;
                                            break;
                                        }
                                    }
                                }
                                if (mesCol == null)
                                    throw new SyntaxError("Неправильный формат загруженного файла:\n\tВ таблице отсутствует столбец содержащий 'изм':\n" + ws.Cell(i, ce).GetValue<string>());
                                if (namesCol > mesCol)
                                {
                                    throw new SyntaxError("Неправильный формат загруженного файла:\n\tСтолбец 'Наименование' в таблице идет после столбца 'Ед. изм.:'\n" + ws.Cell(i, (int)namesCol).GetValue<string>());
                                }

                                //Try to find "Затраты труда рабочих, Средний разряд работы, Затраты труда машинистов"
                                int? wRow = null, rankRow = null, mRow = null;
                                for (var r = i + 1; r <= re; r++)
                                {
                                    var wRowCell = ws.Cell(r, (int)namesCol);

                                    var leftCell = ws.Cell(r, cs);

                                    if (!check(leftCell))
                                    {
                                        if (Regex.IsMatch(leftCell.GetValue<string>(), @"^\s*Код р", RegexOptions.IgnoreCase) || Regex.IsMatch(leftCell.GetValue<string>(), @"^\s*Измеритель", RegexOptions.IgnoreCase))
                                        {
                                            break;
                                        }
                                    }

                                    if (check(wRowCell))
                                        continue;
                                    if (Regex.IsMatch(wRowCell.GetValue<string>(), @" рабочих", RegexOptions.IgnoreCase))
                                    {
                                        wRow = r;
                                        break;
                                    }
                                }
                                for (var r = i + 1; r <= re; r++)
                                {
                                    var rankRowCell = ws.Cell(r, (int)namesCol);

                                    var leftCell = ws.Cell(r, cs);

                                    if (!check(leftCell))
                                    {
                                        if (Regex.IsMatch(leftCell.GetValue<string>(), @"^\s*Код р", RegexOptions.IgnoreCase) || Regex.IsMatch(leftCell.GetValue<string>(), @"^\s*Измеритель", RegexOptions.IgnoreCase))
                                        {
                                            break;
                                        }
                                    }

                                    if (check(rankRowCell))
                                        continue;
                                    if (Regex.IsMatch(rankRowCell.GetValue<string>(), @"разряд", RegexOptions.IgnoreCase))
                                    {
                                        rankRow = r;
                                        break;
                                    }
                                }
                                for (var r = i + 1; r <= re; r++)
                                {
                                    var mRowCell = ws.Cell(r, (int)namesCol);

                                    var leftCell = ws.Cell(r, cs);

                                    if (!check(leftCell))
                                    {
                                        if (Regex.IsMatch(leftCell.GetValue<string>(), @"^\s*Код р", RegexOptions.IgnoreCase) || Regex.IsMatch(leftCell.GetValue<string>(), @"^\s*Измеритель", RegexOptions.IgnoreCase))
                                        {
                                            break;
                                        }
                                    }

                                    if (check(mRowCell))
                                        continue;
                                    if (Regex.IsMatch(mRowCell.GetValue<string>(), @"машинистов", RegexOptions.IgnoreCase))
                                    {
                                        mRow = r;
                                        break;
                                    }
                                }

                                //Try to find mechs and mats
                                List<RowMech> mechs = new List<RowMech>();
                                List<RowMat> mats = new List<RowMat>();
                                bool write = false;
                                for (int r = i + 1; r <= re; r++)
                                {
                                    var mechRowCell = ws.Cell(r, (int)namesCol);

                                    var leftCell = ws.Cell(r, cs);
                                    if (!check(leftCell))
                                    {
                                        if (Regex.IsMatch(leftCell.GetValue<string>(), @"ПРИЛОЖЕН", RegexOptions.IgnoreCase))
                                            break;
                                        if (Regex.IsMatch(leftCell.GetValue<string>(), @"^\s*Код р", RegexOptions.IgnoreCase) || Regex.IsMatch(leftCell.GetValue<string>(), @"^\s*Измеритель", RegexOptions.IgnoreCase))
                                        {
                                            if (r == re)
                                                break;
                                            var nextCell = ws.Cell(r + 1, cs);
                                            //if after choosen cell no next mechs follow:
                                            if (check(nextCell) || !Regex.IsMatch(nextCell.GetValue<string>(), @"\d+-\d+|\d+\.\d+", RegexOptions.IgnoreCase))
                                                break;
                                            else
                                                continue;
                                        }
                                    }
                                    if (check(mechRowCell))
                                        continue;

                                    if (Regex.IsMatch(mechRowCell.GetValue<string>(), @"МЕХАНИЗМ", RegexOptions.IgnoreCase) && !write)
                                    {
                                        write = true;
                                    }
                                    else if (write && !Regex.IsMatch(mechRowCell.GetValue<string>(), @"МАТЕРИАЛ", RegexOptions.IgnoreCase))
                                    {
                                        //if mech places in two cells, its value will be in the first cell, so we can just add a part of name from the second cell to the first
                                        if (check(leftCell))
                                        {
                                            if (mechs.Count > 0)
                                            {
                                                var prevLeftCell = ws.Cell(r - 1, cs);
                                                if (!check(prevLeftCell))
                                                {
                                                    var prevName = mechs.FirstOrDefault(me => me.Code == prevLeftCell.GetValue<string>()).Name;
                                                    if (Regex.IsMatch(mechRowCell.GetValue<string>(), @"^\s") || Regex.IsMatch(prevName, @"\s$"))
                                                        mechs.FirstOrDefault(me => me.Code == prevLeftCell.GetValue<string>()).Name += mechRowCell.GetValue<string>();
                                                    else
                                                        mechs.FirstOrDefault(me => me.Code == prevLeftCell.GetValue<string>()).Name += " " + mechRowCell.GetValue<string>();
                                                    continue;
                                                }
                                            }
                                        }
                                        var mech = new RowMech
                                        {
                                            Name = mechRowCell.GetValue<string>(),
                                            Code = check(leftCell) ? null : leftCell.GetValue<string>(),
                                            row = r
                                        };
                                        mechs.Add(mech);
                                    }
                                    else if (write && Regex.IsMatch(mechRowCell.GetValue<string>(), @"МАТЕРИАЛ", RegexOptions.IgnoreCase))
                                    {
                                        break;
                                    }
                                }
                                write = false;
                                for (int r = i + 1; r <= re; r++)
                                {
                                    var matRowCell = ws.Cell(r, (int)namesCol);

                                    var leftCell = ws.Cell(r, cs);
                                    if (!check(leftCell))
                                    {
                                        if (Regex.IsMatch(leftCell.GetValue<string>(), @"ПРИЛОЖЕН", RegexOptions.IgnoreCase) || Regex.IsMatch(leftCell.GetValue<string>(), @"^\s*Измеритель", RegexOptions.IgnoreCase))
                                            break;
                                        if (Regex.IsMatch(leftCell.GetValue<string>(), @"^\s*Код р", RegexOptions.IgnoreCase))
                                        {
                                            if (r == re)
                                                break;
                                            var nextCell = ws.Cell(r + 1, cs);
                                            //if after choosen cell no next mats follow:
                                            if (check(nextCell) || !Regex.IsMatch(nextCell.GetValue<string>(), @"\d+-\d+|\d+\.\d+", RegexOptions.IgnoreCase))
                                                break;
                                            else
                                                continue;
                                        }
                                    }
                                    if (check(matRowCell))
                                        continue;

                                    if (Regex.IsMatch(matRowCell.GetValue<string>(), @"МАТЕРИАЛ", RegexOptions.IgnoreCase) && !write)
                                    {
                                        write = true;
                                    }
                                    else if (write && !Regex.IsMatch(matRowCell.GetValue<string>(), @"МЕХАНИЗМ", RegexOptions.IgnoreCase))
                                    {
                                        //if mat places in two cells, its value will be in the first cell, so we can just add a part of name from the second cell to the first
                                        if (check(leftCell))
                                        {
                                            if (mats.Count > 0)
                                            {
                                                var prevLeftCell = ws.Cell(r - 1, cs);
                                                if (!check(prevLeftCell))
                                                {
                                                    var prevName = mats.FirstOrDefault(me => me.Code == prevLeftCell.GetValue<string>()).Name;
                                                    if (Regex.IsMatch(matRowCell.GetValue<string>(), @"^\s") || Regex.IsMatch(prevName, @"\s$"))
                                                        mats.FirstOrDefault(me => me.Code == prevLeftCell.GetValue<string>()).Name += matRowCell.GetValue<string>();
                                                    else
                                                        mats.FirstOrDefault(me => me.Code == prevLeftCell.GetValue<string>()).Name += " " + matRowCell.GetValue<string>();
                                                    continue;
                                                }
                                            }
                                        }

                                        var unitsCell = ws.Cell(r, (int)mesCol);

                                        var mat = new RowMat
                                        {
                                            Name = matRowCell.GetValue<string>(),
                                            Code = check(leftCell) ? null : leftCell.GetValue<string>(),
                                            Units = check(unitsCell) ? null : unitsCell.GetValue<string>(),
                                            row = r
                                        };
                                        mats.Add(mat);
                                    }
                                    else if (write && Regex.IsMatch(matRowCell.GetValue<string>(), @"МЕХАНИЗМ", RegexOptions.IgnoreCase))
                                    {
                                        break;
                                    }
                                }

                                //Filling items:
                                for (int c = (int)mesCol + 1; c <= ce; c++)
                                {
                                    var codeCell = ws.Cell(i, c);
                                    if (check(codeCell))
                                        break;

                                    string code = Regex.Match(codeCell.GetValue<string>(), @"\S+").Value;

                                    JobProxy item;
                                    lock (m)
                                    {

                                        item = m.FirstOrDefault(it => it.Code == code);

                                        //If item doesn't exist, create part of it to fill manually
                                        if (item == null)
                                        {
                                            item = new JobProxy { Code = code };
                                            m.Add(item);
                                        }

                                    }
                                    //Seems better write delegate like action but with ref value for three following code fragments
                                    //wRow:
                                    if (wRow != null)
                                    {
                                        cell = ws.Cell((int)wRow, c);
                                        if (!check(cell))
                                            item.WLaborCosts = cell.GetValue<string>();
                                    }
                                    if (rankRow != null)
                                    {
                                        cell = ws.Cell((int)rankRow, c);
                                        if (!check(cell))
                                            item.AvRank = cell.GetValue<string>();
                                    }
                                    if (mRow != null)
                                    {
                                        cell = ws.Cell((int)mRow, c);
                                        if (!check(cell))
                                            item.MLaborCosts = cell.GetValue<string>();
                                    }

                                    foreach (var mech in mechs)
                                    {
                                        cell = ws.Cell(mech.row, c);
                                        if (check(cell) || cell.GetValue<string>() == "-")
                                            continue;

                                        var newMech = mech as MechProxy;
                                        newMech.Amount = cell.GetValue<string>();
                                        item.Mechs.Add(newMech);
                                    }


                                    foreach (var mat in mats)
                                    {
                                        cell = ws.Cell(mat.row, c);
                                        if (check(cell) || cell.GetValue<string>() == "-")
                                            continue;

                                        var newMat = mat as MatProxy;
                                        newMat.Amount = cell.GetValue<string>();
                                        item.Mats.Add(newMat);
                                    }
                                }
                            };

                            //Parse only when choosen cell is "Код р"
                            for (a -= 1; a < re; a++)
                            {
                                cell = ws.Cell(a + 1, cs);
                                if (check(cell))
                                    continue;
                                //Find next "Код р"
                                if (Regex.IsMatch(cell.GetValue<string>(), @"^\s*Код р", RegexOptions.IgnoreCase))
                                {
                                    a++;
                                    {
                                        int b = a;
                                        TasksAfter.Add(Task.Factory.StartNew(() => parseTable(b)));
                                    }
                                }
                                //then find next "Измеритель"
                                if (Regex.IsMatch(cell.GetValue<string>(), @"^\s*Измеритель"))
                                {
                                    break;
                                }
                            }
                        }

                    }

                    if (m.Count == 0)
                    {
                        cs++;
                    }
                    else
                    {
                        if (TasksAfter.Count > 0)
                            Task.WaitAll(TasksAfter.ToArray());
                    }


                }

            }
            return m.ToArray();
        }
    }
}
