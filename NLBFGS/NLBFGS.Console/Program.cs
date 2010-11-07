using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLBFGS.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var optimum = Optimizer.Optimize(2, Evaluate, new double[2]);
            System.Console.WriteLine("{0}, {1}", optimum[0], optimum[1]);
        }

        private static Tuple<double, double[]> Evaluate(double[] x, double step)
        {
            var fx = Math.Pow(x[0] - 5, 2) + Math.Pow(x[1] - 4, 2) + 2 * x[0] + 2 * x[1];
            var grad = new double[] 
            { 
                2 * (x[0] - 5) + 2, 
                2 * (x[1] - 4) + 2 
            };

            return Tuple.Create(fx, grad);
        }
    }
}
