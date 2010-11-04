using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;
using NSuperLU;

namespace MultiviewCurvesToCyl
{
    static class QuadraticFunctionHelper
    {
        public static QuadraticFactorsData ExtractQuadraticFactors(Term targetFunction, Variable[] variables)
        {
            var indexOf = new Dictionary<Variable, int>();
            for (int i = 0; i < variables.Length; ++i)
                indexOf.Add(variables[i], i);

            var extractor = new QuadraticFactorsExtractor(indexOf);
            targetFunction.Accept(extractor);

            return extractor.Result;
        }

        public static QuadraticFactorsData ExtractLinearFactors(Term targetFunction, Variable[] variables)
        {
            var indexOf = new Dictionary<Variable, int>();
            for (int i = 0; i < variables.Length; ++i)
                indexOf.Add(variables[i], i);

            var extractor = new QuadraticFactorsExtractor(indexOf, linearOnly: true);
            targetFunction.Accept(extractor);

            return extractor.Result;
        }

        #region QuadraticFactorsExtractor class

        private class QuadraticFactorsExtractor : ITermVisitor
        {
            private readonly Stack<QuadraticFactorsData> computationStack;
            private readonly Dictionary<Variable, int> indexOf;
            private readonly bool linearOnly;

            public QuadraticFactorsExtractor(Dictionary<Variable, int> indexOf, bool linearOnly = false)
            {
                computationStack = new Stack<QuadraticFactorsData>();
                this.indexOf = indexOf;
                this.linearOnly = linearOnly;
            }

            public QuadraticFactorsData Result
            {
                get { return computationStack.Peek(); }
            }

            public void Visit(Constant constant)
            {
                var data = new QuadraticFactorsData(constant.Value);
                computationStack.Push(data);
            }

            public void Visit(Zero zero)
            {
                var data = new QuadraticFactorsData(0);
                computationStack.Push(data);
            }

            public void Visit(Variable variable)
            {
                var index = indexOf[variable];
                var data = new QuadraticFactorsData(index, 1);

                computationStack.Push(data);
            }

            public void Visit(IntPower intPower)
            {
                if (intPower.Exponent != 2)
                    throw new NotSupportedException("We support only square integer powers");

                intPower.Base.Accept(this);
                var baseFactorsData = computationStack.Pop();
                var result = QuadraticFactorsData.Square(baseFactorsData, linearOnly);
                computationStack.Push(result);
            }

            public void Visit(Product product)
            {
                product.Left.Accept(this);
                var leftFactorsData = computationStack.Pop();

                product.Right.Accept(this);
                var rightFactorsData = computationStack.Pop();

                var result = QuadraticFactorsData.Product(leftFactorsData, rightFactorsData, linearOnly);
                computationStack.Push(result);
            }

            public void Visit(Sum sum)
            {
                var sumItems = new List<QuadraticFactorsData>();
                foreach (var item in sum.Terms)
                {
                    item.Accept(this);
                    var itemFactorsData = computationStack.Pop();
                    sumItems.Add(itemFactorsData);
                }

                var result = QuadraticFactorsData.Sum(sumItems);
                computationStack.Push(result);
            }

            public void Visit(Log log)
            {
                throw new NotSupportedException();
            }

            public void Visit(Exp exp)
            {
                throw new NotSupportedException();
            }


            public void Visit(PiecewiseTerm piecewiseTerm)
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region QuadraticFactorsData class

        public class QuadraticFactorsData
        {
            private Tuple<Tuple<int, int>, double>[] quadraticFactors;
            private Tuple<int, double>[] linearFactors;
            private double constantFactor;

            public QuadraticFactorsData(double constantFactor)
            {
                this.quadraticFactors = new Tuple<Tuple<int, int>, double>[0];
                this.linearFactors = new Tuple<int, double>[0];
                this.constantFactor = constantFactor;
            }

            public QuadraticFactorsData(int index, double linearFactor)
            {
                this.quadraticFactors = new Tuple<Tuple<int, int>, double>[0];
                this.linearFactors = new Tuple<int, double>[] { Tuple.Create(index, linearFactor) };
                this.constantFactor = 0;
            }

            public IEnumerable<Tuple<int, int, double>> QuadraticFactors
            {
                get { return quadraticFactors.Select(x => Tuple.Create(x.Item1.Item1, x.Item1.Item2, x.Item2)); }
            }

            public IEnumerable<Tuple<int, double>> LinearFactors
            {
                get { return Array.AsReadOnly(linearFactors); }
            }

            private QuadraticFactorsData(
                Tuple<Tuple<int, int>, double>[] quadraticFactors,
                Tuple<int, double>[] linearFactors,
                double constantFactor)
            {
                this.quadraticFactors = quadraticFactors;
                this.linearFactors = linearFactors;
                this.constantFactor = constantFactor;
            }

            public static QuadraticFactorsData Sum(IEnumerable<QuadraticFactorsData> input)
            {
                var first = input.First();
                var rest = input.Skip(1);

                var quadraticFactors = first.quadraticFactors;
                var linearFactors = first.linearFactors;
                var constantFactor = first.constantFactor;

                foreach (var item in rest)
                {
                    quadraticFactors = SumMerge(quadraticFactors, item.quadraticFactors);
                    linearFactors = SumMerge(linearFactors, item.linearFactors);
                    constantFactor += item.constantFactor;
                }

                return new QuadraticFactorsData(quadraticFactors, linearFactors, constantFactor);
            }

            public static QuadraticFactorsData Square(QuadraticFactorsData squareBase, bool linearOnly)
            {
                //Contract.Requires(squareBase.quadraticFactors.Length == 0, "Cannot square an already-quadratic term");

                var linearFactorsCount = squareBase.linearFactors.Length;
                int i;

                // generate the new quadratic factors
                Tuple<Tuple<int, int>, double>[] quadraticFactors;
                if (!linearOnly)
                {
                    quadraticFactors = new Tuple<Tuple<int, int>, double>[linearFactorsCount * linearFactorsCount];
                    i = 0;
                    foreach (var left in squareBase.linearFactors)
                        foreach (var right in squareBase.linearFactors)
                            quadraticFactors[i++] = Tuple.Create(Tuple.Create(left.Item1, right.Item1), left.Item2 * right.Item2);
                }
                else
                    quadraticFactors = new Tuple<Tuple<int, int>, double>[0];

                // generate the new linear factors
                var linearFactors = new Tuple<int, double>[linearFactorsCount];
                i = 0;
                foreach (var tuple in squareBase.linearFactors)
                {
                    var newValue = 2 * squareBase.constantFactor * tuple.Item2;
                    linearFactors[i++] = Tuple.Create(tuple.Item1, newValue);
                }

                // generate the new constant factor
                var constantFactor = Math.Pow(squareBase.constantFactor, 2);

                return new QuadraticFactorsData(quadraticFactors, linearFactors, constantFactor);
            }

            public static QuadraticFactorsData Product(QuadraticFactorsData left, QuadraticFactorsData right, bool linearOnly)
            {
                Contract.Requires(MaxPower(left) + MaxPower(right) <= 2); // the product of both terms will be quadratic

                if (MaxPower(left) == 0) // the left is a constant. So we just multiply right values by this constant
                    return Scale(right, left.constantFactor);
                else if (MaxPower(right) == 0) // the right is a constant. So we just multiply left values by this constant
                    return Scale(left, right.constantFactor);
                else // both are linear. The contract requirements ensured us
                    return Convolve(left, right, linearOnly);
            }

            // Computes the convolution of two linear terms to create a quadratic one.
            private static QuadraticFactorsData Convolve(QuadraticFactorsData left, QuadraticFactorsData right, bool linearOnly)
            {
                // generate quadratic factors
                Tuple<Tuple<int, int>, double>[] quadraticFactors;
                if (!linearOnly)
                {
                    int i = 0;
                    quadraticFactors = new Tuple<Tuple<int, int>, double>[left.linearFactors.Length * right.linearFactors.Length];
                    foreach (var leftItem in left.linearFactors)
                        foreach (var rightItem in right.linearFactors)
                            quadraticFactors[i++] = Tuple.Create(Tuple.Create(leftItem.Item1, rightItem.Item1), leftItem.Item2 * rightItem.Item2);
                }
                else
                    quadraticFactors = new Tuple<Tuple<int, int>, double>[0];

                // generate linear factors
                var leftScaled = Scale(left.linearFactors, right.constantFactor); 
                var rightScaled = Scale(right.linearFactors, left.constantFactor);
                var linearFactors = SumMerge(leftScaled, rightScaled); // the sum will be stored in leftScaled

                // generate constant factor
                var constantFactor = left.constantFactor * right.constantFactor;

                return new QuadraticFactorsData(quadraticFactors, linearFactors, constantFactor);
            }

            [Pure]
            private static Tuple<T, double>[] Scale<T>(Tuple<T, double>[] input, double factor)
            {
                var result = new Tuple<T, double>[input.Length];
                for (int i = 0; i < input.Length; i++)
                    result[i] = Tuple.Create(input[i].Item1, factor * input[i].Item2);

                return result;
            }

            [Pure]
            private static QuadraticFactorsData Scale(QuadraticFactorsData data, double factor)
            {
                var quadraticFactors = Scale(data.quadraticFactors, factor);
                var linearFactors = Scale(data.linearFactors, factor);
                var constantFactor = data.constantFactor * factor;

                return new QuadraticFactorsData(quadraticFactors, linearFactors, constantFactor);
            }

            [Pure]
            public static int MaxPower(QuadraticFactorsData data)
            {
                if (data.quadraticFactors.Length > 0)
                    return 2;
                else if (data.linearFactors.Length > 0)
                    return 1;
                else
                    return 0;
            }

            [Pure]
            private static Tuple<int, double>[] SumMerge(Tuple<int, double>[] first, Tuple<int, double>[] second)
            {
                var result = new Tuple<int, double>[first.Length + second.Length];
                int resultIndex = 0;

                int firstIndex = 0;
                int secondIndex = 0;
                while (firstIndex < first.Length && secondIndex < second.Length)
                {
                    if (first[firstIndex].Item1 < second[secondIndex].Item1) // first[firstIndex] has smaller index
                    {
                        result[resultIndex] = first[firstIndex];
                        ++firstIndex;
                    }
                    else if (first[firstIndex].Item1 > second[secondIndex].Item1) // second[secondIndex] has smaller index
                    {
                        result[resultIndex] = second[secondIndex];
                        ++secondIndex;
                    }
                    else // both have same index
                    {
                        result[resultIndex] =
                            // we re-use the same index and sum the values
                            Tuple.Create(first[firstIndex].Item1, first[firstIndex].Item2 + second[secondIndex].Item2);
                        ++firstIndex;
                        ++secondIndex;
                    }
                    ++resultIndex;
                }

                // copy remaining elements in the first array (if they exist - they are bigger than all elements in the second one).
                while (firstIndex < first.Length)
                {
                    result[resultIndex] = first[firstIndex];
                    ++firstIndex;
                    ++resultIndex;
                }

                // copy remaining elements in the second array (if they exist - they are bigger than all elements in the first one).
                while (secondIndex < second.Length)
                {
                    result[resultIndex] = second[secondIndex];
                    ++secondIndex;
                    ++resultIndex;
                }

                Array.Resize(ref result, resultIndex); // we resize the array to get rid of all the remaining elements

                return result;
            }

            [Pure]
            private static Tuple<Tuple<int, int>, double>[] SumMerge(
                Tuple<Tuple<int, int>, double>[] first, 
                Tuple<Tuple<int, int>, double>[] second)
            {
                var result = new Tuple<Tuple<int, int>, double>[first.Length + second.Length];
                int resultIndex = 0;

                int firstIndex = 0;
                int secondIndex = 0;
                while (firstIndex < first.Length && secondIndex < second.Length)
                {
                    if (Compare(first[firstIndex].Item1, second[secondIndex].Item1) < 0) // first[i] has smaller index
                    {
                        result[resultIndex] = first[firstIndex];
                        ++firstIndex;
                    }
                    else if (Compare(first[firstIndex].Item1, second[secondIndex].Item1) > 0) // second[i] has smaller index
                    {
                        result[resultIndex] = second[secondIndex];
                        ++secondIndex;
                    }
                    else // both have same index
                    {
                        result[resultIndex] =
                            // we re-use the same index and sum the values
                            Tuple.Create(first[firstIndex].Item1, first[firstIndex].Item2 + second[secondIndex].Item2);  
                        ++firstIndex;
                        ++secondIndex;
                    }
                    ++resultIndex;
                }

                // copy remaining elements in the first array (if they exist - they are bigger than all elements in the second one).
                while (firstIndex < first.Length)
                {
                    result[resultIndex] = first[firstIndex];
                    ++firstIndex;
                    ++resultIndex;
                }

                // copy remaining elements in the second array (if they exist - they are bigger than all elements in the first one).
                while (secondIndex < second.Length)
                {
                    result[resultIndex] = second[secondIndex];
                    ++secondIndex;
                    ++resultIndex;
                }

                Array.Resize(ref result, resultIndex); // we resize the array to get rid of all the remaining elements

                return result;
            }

            private static int Compare(Tuple<int, int> first, Tuple<int, int> second)
            {
                var firstRes = first.Item1 - second.Item1;
                if (firstRes == 0)
                {
                    var secondRes = first.Item2 - second.Item2;
                    return secondRes;
                }
                else
                    return firstRes;
            }
        }
        #endregion
    }
}
