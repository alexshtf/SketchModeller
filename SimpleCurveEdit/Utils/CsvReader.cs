using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Utils
{
    /// <summary>
    /// Utilities for reading CSV from files.
    /// </summary>
    public static class CsvReader
    {
        /// <summary>
        /// Reads CSV data from a file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>An enumeration of individual parsed lines.</returns>
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
