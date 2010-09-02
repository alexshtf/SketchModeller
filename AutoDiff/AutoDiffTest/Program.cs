using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using Utils;
using System.Diagnostics;

namespace AutoDiffTest
{
    class Program
    {
        static void Main(string[] args)
        {
            LogExpTest();
            //SimpleTest();
            //Benchmark();
        }

        private static void LogExpTest()
        {
            throw new NotImplementedException();
        }

        private static void SimpleTest()
        {
            var x1 = new Variable();
            var x2 = new Variable();
            var x3 = new Variable();

            var f = x1 * x2 * x3 + 2 * x1 * x3;

            var diff = Differentiator.Differentiate(f, new Variable[] { x1, x2, x3 }, new double[] { 1, 2, 3 });
            Console.WriteLine(diff.Length);
        }

        private static void Benchmark()
        {
            const int VARIABLES_COUNT = 1700;
            const int DIFF_COUNT = 100;

            var random = new Random();
            Variable[] variables = new Variable[VARIABLES_COUNT];
            for (int i = 0; i < VARIABLES_COUNT; ++i)
                variables[i] = new Variable();

            var firstTerm = TermBuilder.Sum(RandomStream(random).Zip(variables).Select(x => x.Item1 * x.Item2)); // sigme ci * xi
            var secondTerm = // sigma aij * xi * xj
                TermBuilder.Sum(
                    from item in MatrixIndices(VARIABLES_COUNT).Zip(RandomStream(random))
                    let i = item.Item1.Item1
                    let j = item.Item1.Item2
                    let a = item.Item2
                    select a * variables[i] * variables[j]
                );

            var totalTerm = firstTerm + secondTerm;

            var totalTime = TimeSpan.Zero;
            for (int i = 0; i < DIFF_COUNT; ++i)
            {
                var values = RandomStream(random).Take(variables.Length).ToArray();

                var sw = Stopwatch.StartNew();
                var diff = Differentiator.Differentiate(totalTerm, variables, values);
                sw.Stop();

                totalTime += sw.Elapsed;
            }

            Console.WriteLine("It took {0} milliseconds per differentiation", totalTime.TotalMilliseconds / DIFF_COUNT);
            //Console.ReadLine();
        }

        private static IEnumerable<double> RandomStream(Random random)
        {
            return Utils.Enumerable.Generate(random.NextDouble(), x => random.NextDouble());
        }

        private static IEnumerable<Tuple<int, int>> MatrixIndices(int m)
        {
            for (int i = 1; i < m - 1; ++i)
                for (int j = i - 1; j <= i + 1; ++j)
                    yield return Tuple.Create(i, j);
        }
    }
}
