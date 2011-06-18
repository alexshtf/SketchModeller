using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Modelling.Computations
{
    struct IntervalsSample
    {
        public double Value;
        public double Derivative;
    }

    static class IntervalsSampler
    {
        public static IntervalsSample SampleIntervals(double[][] coefficients, double input, params int[] breakIndices)
        {
            var intervalIndex = GetIntervalIndex(input, breakIndices);
            var intervalCoefficients = coefficients[intervalIndex];

            // polynomial of 3rd degree
            var value =
                intervalCoefficients[0] +
                intervalCoefficients[1] * input +
                intervalCoefficients[2] * input * input +
                intervalCoefficients[3] * input * input * input;

            var derivative =
                1 * intervalCoefficients[1] +
                2 * intervalCoefficients[2] * input +
                3 * intervalCoefficients[3] * input * input;

            return new IntervalsSample { Value = value, Derivative = derivative };
        }

        private static int GetIntervalIndex(double value, int[] breakIndices)
        {
            for (int i = 0; i < breakIndices.Length; ++i)
                if (value < breakIndices[i])
                    return i;

            return breakIndices.Length - 1;
        }
    }
}
