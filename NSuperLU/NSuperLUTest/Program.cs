using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSuperLU;

namespace NSuperLUTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press ENTER");
            Console.ReadLine();
            var matrix = new FactoredSparseMatrix(new Tuple<int, int, double>[]
                {
                    Tuple.Create(0, 0, 1.0),
                    Tuple.Create(0, 1, 2.0),
                    Tuple.Create(1, 0, 2.0),
                    Tuple.Create(1, 1, 1.0),
                    Tuple.Create(2, 2, 1.0),
                }, 3);

            var result = matrix.Solve(new double[] { 1, 2, 3 });
            Console.WriteLine("{0}, {1}, {2}", result[0], result[1], result[2]);

            matrix.Dispose();
        }
    }
}
