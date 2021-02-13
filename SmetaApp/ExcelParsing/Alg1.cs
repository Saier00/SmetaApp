using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmetaApp.ExcelParsing
{
    public class Alg1 : IJobParsing
    {
        public JobProxy[] Parse(IWorkbook wb)
        {
            //Adapted client-side parsing code at server-side
            List<JobProxy> m = new List<JobProxy>();
            if (!wb.AnySheets())
                throw new SyntaxError("Не найдено ни одного листа в документе");
            foreach (ISheet ws in wb)
            {
                if (ws.IsEmpty())
                    continue;
                int cs = ws.FirstColumnUsed();
                int ce = ws.LastColumnUsed();
                int rs = ws.FirstRowUsed();
                int re = ws.LastRowUsed();
                while (m.Count == 0 && cs <= ce)
                {
                    List<Task> TasksAfter = new List<Task>();

                    for (int a = rs; a <= re; a++)
                    {

                        var cell = ws.Cell(a, cs);

                        if (cell.Check())
                        {
                            continue;
                        }

                        if (Regex.IsMatch(cell.GetValue(), @"^\s*Измеритель"))
                        {
                            string measurer = null;
                            string units;
                            var mu = Regex.Split(Regex.Replace(cell.GetValue(), @"^\s*Измеритель\S*\s+", ""), @"\s+");
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
                                return cell.Check();
                            };

                            //Try to find first not empty existing cell
                            while (incr()) { }
                            string name = null;
                            while (!Regex.IsMatch(cell.GetValue(), @"^\s*Код р", RegexOptions.IgnoreCase))
                            {
                                if (Regex.IsMatch(cell.GetValue(), @"(\d*-\d*-\d*-\d*)\s+[А-Я][а-яА-Я\s\w\,\.]*"))
                                {
                                    //[CODE][spaces][NAME]
                                    //or [CODE][spaces][NAME][spaces][CODE][spaces][NAME]...
                                    var matches = Regex.Matches(cell.GetValue(), @"(\d*-\d*-\d*-\d*)\s+[А-Я][а-яА-Я\w\,\.]*(\s[а-яА-Я\w\,\.]+)*(?!\d*-\d*-\d*-\d*)");
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
                            else if (Regex.IsMatch(cell.GetValue(), @"^\s*[А-Я][а-яА-Я\s\w]*")) {
                                //[spaces][NAME]
                                name = Regex.Replace(cell.GetValue() ,@"^\s+", "");
                            }
                            else if (Regex.IsMatch(cell.GetValue(),@"(\d*-\d*-\d*-\d*)\s+[а-я\d\(\)][а-яА-Я\s\w\,\.]*")) {
                                //[CODE][spaces][TYPE]
                                //name can be undefined
                                var matches = Regex.Matches(cell.GetValue(), @"(\d*-\d*-\d*-\d*)\s+[а-я\d\(\)][а-яА-Я\w\,\.]*(\s[а-яА-Я\w\,\.]+)*(?!\d*-\d*-\d*-\d*)");
                                foreach (Match match in matches) {
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
                                if (namesColCell.Check())
                                    continue;

                                if (Regex.IsMatch(namesColCell.GetValue(), @"^\s*Наим", RegexOptions.IgnoreCase))
                                {
                                    namesCol = k;
                                    break;
                                }
                            }
                            if (namesCol == null)
                                throw new SyntaxError("Неправильный формат загруженного файла:\n\tВ таблице отсутствует столбец содержащий в начале 'Наим':\n" + ws.Cell(i, ce).GetValue());
                            //Try to find "изм"
                            for (var k = cs + 2; k <= ce; k++)
                            {
                                if (namesCol == k && k != ce)
                                    continue;
                                if (namesCol != k)
                                {
                                    var mesColCell = ws.Cell(i, k);
                                    if (mesColCell.Check())
                                        continue;
                                    if (Regex.IsMatch(mesColCell.GetValue(), @"изм", RegexOptions.IgnoreCase))
                                    {
                                        mesCol = k;
                                        break;
                                    }
                                }
                            }
                            if (mesCol == null)
                                throw new SyntaxError("Неправильный формат загруженного файла:\n\tВ таблице отсутствует столбец содержащий 'изм':\n" + ws.Cell(i, ce).GetValue());
                            if (namesCol > mesCol)
                            {
                                throw new SyntaxError("Неправильный формат загруженного файла:\n\tСтолбец 'Наименование' в таблице идет после столбца 'Ед. изм.:'\n" + ws.Cell(i, (int)namesCol).GetValue());
                            }

                            //Try to find "Затраты труда рабочих, Средний разряд работы, Затраты труда машинистов"
                            int? wRow = null, rankRow = null, mRow = null;
                            for (var r = i + 2; r <= re; r++)
                            {
                                var wRowCell = ws.Cell(r, (int)namesCol);

                                var leftCell = ws.Cell(r, cs);

                                if (!leftCell.Check())
                                {
                                    if (Regex.IsMatch(leftCell.GetValue(), @"^\s*Код р", RegexOptions.IgnoreCase) || Regex.IsMatch(leftCell.GetValue(), @"^\s*Измеритель", RegexOptions.IgnoreCase))
                                    {
                                        break;
                                    }
                                }

                                if (wRowCell.Check())
                                    continue;
                                if (Regex.IsMatch(wRowCell.GetValue(), @" рабочих", RegexOptions.IgnoreCase))
                                {
                                    wRow = r;
                                    break;
                                }
                            }
                            for (var r = i + 2; r <= re; r++)
                            {
                                var rankRowCell = ws.Cell(r, (int)namesCol);

                                var leftCell = ws.Cell(r, cs);

                                if (!leftCell.Check())
                                {
                                    if (Regex.IsMatch(leftCell.GetValue(), @"^\s*Код р", RegexOptions.IgnoreCase) || Regex.IsMatch(leftCell.GetValue(), @"^\s*Измеритель", RegexOptions.IgnoreCase))
                                    {
                                        break;
                                    }
                                }

                                if (rankRowCell.Check())
                                    continue;
                                if (Regex.IsMatch(rankRowCell.GetValue(), @"разряд", RegexOptions.IgnoreCase))
                                {
                                    rankRow = r;
                                    break;
                                }
                            }
                            for (var r = i + 2; r <= re; r++)
                            {
                                var mRowCell = ws.Cell(r, (int)namesCol);

                                var leftCell = ws.Cell(r, cs);

                                if (!leftCell.Check())
                                {
                                    if (Regex.IsMatch(leftCell.GetValue(), @"^\s*Код р", RegexOptions.IgnoreCase) || Regex.IsMatch(leftCell.GetValue(), @"^\s*Измеритель", RegexOptions.IgnoreCase))
                                    {
                                        break;
                                    }
                                }

                                if (mRowCell.Check())
                                    continue;
                                if (Regex.IsMatch(mRowCell.GetValue(), @"машинистов", RegexOptions.IgnoreCase))
                                {
                                    mRow = r;
                                    break;
                                }
                            }

                            //Try to find mechs and mats
                            List<RowMech> mechs = new List<RowMech>();
                            List<RowMat> mats = new List<RowMat>();
                            bool write = false;
                            for (int r = i + 2; r <= re; r++)
                            {
                                var mechRowCell = ws.Cell(r, (int)namesCol);

                                var leftCell = ws.Cell(r, cs);
                                if (!leftCell.Check())
                                {
                                    if (Regex.IsMatch(leftCell.GetValue(), @"ПРИЛОЖЕН", RegexOptions.IgnoreCase))
                                        break;
                                    if (Regex.IsMatch(leftCell.GetValue(), @"^\s*Код р", RegexOptions.IgnoreCase) || Regex.IsMatch(leftCell.GetValue(), @"^\s*Измеритель", RegexOptions.IgnoreCase))
                                    {
                                        if (r == re)
                                            break;
                                        var nextCell = ws.Cell(r + 2, cs);
                                        //if after choosen cell no next mechs follow:
                                        if (nextCell.Check() || !Regex.IsMatch(nextCell.GetValue(), @"\d+-\d+|\d+\.\d+", RegexOptions.IgnoreCase))
                                            break;
                                        else
                                            continue;
                                    }
                                }
                                if (mechRowCell.Check())
                                    continue;

                                if (Regex.IsMatch(mechRowCell.GetValue(), @"МЕХАНИЗМ", RegexOptions.IgnoreCase) && !write)
                                {
                                    write = true;
                                }
                                else if (write && !Regex.IsMatch(mechRowCell.GetValue(), @"МАТЕРИАЛ", RegexOptions.IgnoreCase))
                                {
                                    //if mech places in two cells, its value will be in the first cell, so we can just add a part of name from the second cell to the first
                                    if (leftCell.Check())
                                    {
                                        if (mechs.Count > 0)
                                        {
                                            var prevLeftCell = ws.Cell(r - 1, cs);
                                            if (!prevLeftCell.Check())
                                            {
                                                var prevName = mechs.FirstOrDefault(me => me.Code == prevLeftCell.GetValue()).Name;
                                                if (Regex.IsMatch(mechRowCell.GetValue(), @"^\s") || Regex.IsMatch(prevName, @"\s$"))
                                                    mechs.FirstOrDefault(me => me.Code == prevLeftCell.GetValue()).Name += mechRowCell.GetValue();
                                                else
                                                    mechs.FirstOrDefault(me => me.Code == prevLeftCell.GetValue()).Name += " " + mechRowCell.GetValue();
                                                continue;
                                            }
                                        }
                                    }
                                    var mech = new RowMech
                                    {
                                        Name = mechRowCell.GetValue(),
                                        Code = leftCell.Check() ? null : leftCell.GetValue(),
                                        row = r
                                    };
                                    mechs.Add(mech);
                                }
                                else if (write && Regex.IsMatch(mechRowCell.GetValue(), @"МАТЕРИАЛ", RegexOptions.IgnoreCase))
                                {
                                    break;
                                }
                            }
                            write = false;
                            for (int r = i + 2; r <= re; r++)
                            {
                                var matRowCell = ws.Cell(r, (int)namesCol);

                                var leftCell = ws.Cell(r, cs);
                                if (!leftCell.Check())
                                {
                                    if (Regex.IsMatch(leftCell.GetValue(), @"ПРИЛОЖЕН", RegexOptions.IgnoreCase) || Regex.IsMatch(leftCell.GetValue(), @"^\s*Измеритель", RegexOptions.IgnoreCase))
                                        break;
                                    if (Regex.IsMatch(leftCell.GetValue(), @"^\s*Код р", RegexOptions.IgnoreCase))
                                    {
                                        if (r == re)
                                            break;
                                        var nextCell = ws.Cell(r + 2, cs);
                                        //if after choosen cell no next mats follow:
                                        if (nextCell.Check() || !Regex.IsMatch(nextCell.GetValue(), @"\d+-\d+|\d+\.\d+", RegexOptions.IgnoreCase))
                                            break;
                                        else
                                            continue;
                                    }
                                }
                                if (matRowCell.Check())
                                    continue;

                                if (Regex.IsMatch(matRowCell.GetValue(), @"МАТЕРИАЛ", RegexOptions.IgnoreCase) && !write)
                                {
                                    write = true;
                                }
                                else if (write && !Regex.IsMatch(matRowCell.GetValue(), @"МЕХАНИЗМ", RegexOptions.IgnoreCase))
                                {
                                    //if mat places in two cells, its value will be in the first cell, so we can just add a part of name from the second cell to the first
                                    if (leftCell.Check())
                                    {
                                        if (mats.Count > 0)
                                        {
                                            var prevLeftCell = ws.Cell(r - 1, cs);
                                            if (!prevLeftCell.Check())
                                            {
                                                var prevName = mats.FirstOrDefault(me => me.Code == prevLeftCell.GetValue()).Name;
                                                if (Regex.IsMatch(matRowCell.GetValue(), @"^\s") || Regex.IsMatch(prevName, @"\s$"))
                                                    mats.FirstOrDefault(me => me.Code == prevLeftCell.GetValue()).Name += matRowCell.GetValue();
                                                else
                                                    mats.FirstOrDefault(me => me.Code == prevLeftCell.GetValue()).Name += " " + matRowCell.GetValue();
                                                continue;
                                            }
                                        }
                                    }

                                    var unitsCell = ws.Cell(r, (int)mesCol);

                                    var mat = new RowMat
                                    {
                                        Name = matRowCell.GetValue(),
                                        Code = leftCell.Check() ? null : leftCell.GetValue(),
                                        Units = unitsCell.Check() ? null : unitsCell.GetValue(),
                                        row = r
                                    };
                                    mats.Add(mat);
                                }
                                else if (write && Regex.IsMatch(matRowCell.GetValue(), @"МЕХАНИЗМ", RegexOptions.IgnoreCase))
                                {
                                    break;
                                }
                            }

                            //Filling items:
                            for (int c = (int)mesCol + 1; c <= ce; c++)
                            {
                                var codeCells = new ICell[2] { ws.Cell(i, c), ws.Cell(i + 1, c) };
                                if (codeCells[0].Check() || codeCells[1].Check())
                                    break;

                                string code = Regex.Match(codeCells[0].GetValue(), @"\S+").Value + Regex.Match(codeCells[1].GetValue(), @"\S+").Value;

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
                                    if (!cell.Check())
                                        item.WLaborCosts = cell.GetValue();
                                }
                                if (rankRow != null)
                                {
                                    cell = ws.Cell((int)rankRow, c);
                                    if (!cell.Check())
                                        item.AvRank = cell.GetValue();
                                }
                                if (mRow != null)
                                {
                                    cell = ws.Cell((int)mRow, c);
                                    if (!cell.Check())
                                        item.MLaborCosts = cell.GetValue();
                                }

                                foreach (var mech in mechs)
                                {
                                    cell = ws.Cell(mech.row, c);
                                    if (cell.Check() || cell.GetValue() == "-")
                                        continue;

                                    var newMech = mech as MechProxy;
                                    newMech.Amount = cell.GetValue();
                                    item.Mechs.Add(newMech);
                                }


                                foreach (var mat in mats)
                                {
                                    cell = ws.Cell(mat.row, c);
                                    if (cell.Check() || cell.GetValue() == "-")
                                        continue;

                                    var newMat = mat as MatProxy;
                                    newMat.Amount = cell.GetValue();
                                    item.Mats.Add(newMat);
                                }
                            }
                        };

                            //Parse only when choosen cell is "Код р"
                            for (a -= 1; a < re; a++)
                            {
                                cell = ws.Cell(a + 1, cs);
                                if (cell.Check())
                                    continue;
                                //Find next "Код р"
                                if (Regex.IsMatch(cell.GetValue(), @"^\s*Код р", RegexOptions.IgnoreCase))
                                {
                                    a++;
                                    {
                                        int b = a;
                                        TasksAfter.Add(Task.Factory.StartNew(() => parseTable(b)));
                                    }
                                }
                                //then find next "Измеритель"
                                if (Regex.IsMatch(cell.GetValue(), @"^\s*Измеритель"))
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
