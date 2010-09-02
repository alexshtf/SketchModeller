using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Utils
{
    public static class CsvReader
    {
        public static IEnumerable<string[]> ReadCsv(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        var items = from item in line.Split(',')
                                    select item.Trim();
                        yield return items.ToArray();
                    }
                }
            }
        }
    }
}
