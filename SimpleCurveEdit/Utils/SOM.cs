using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Utils
{
    public interface ISOMNode<TVector>
    {
        TVector Position { get; set; }
    }

    public interface IOperations<TVector>
    {
        TVector Zero { get; }
        TVector Add(TVector v1, TVector v2);
        TVector Scale(double scalar, TVector v);
        double InnerProduct(TVector v1, TVector v2);
    }

    internal class VectorWrapper<TVector>
    {
        private TVector v;
        private IOperations<TVector> ops;

        public VectorWrapper(TVector v, IOperations<TVector> ops)
        {
            this.v = v;
            this.ops = ops;
        }

        public double NormSquared
        {
            get { return ops.InnerProduct(v, v); }
        }

        public static implicit operator TVector(VectorWrapper<TVector> v)
        {
            return v.v;
        }

        public static VectorWrapper<TVector> operator*(double scale, VectorWrapper<TVector> vec)
        {
            var ops = vec.ops;
            return new VectorWrapper<TVector>(ops.Scale(scale, vec), vec.ops);
        }

        public static double operator*(VectorWrapper<TVector> v1, VectorWrapper<TVector> v2)
        {
            Contract.Assert(v1.ops == v2.ops);

            var ops = v1.ops;
            return ops.InnerProduct(v1, v2);
        }

        public static VectorWrapper<TVector> operator+(VectorWrapper<TVector> v1, VectorWrapper<TVector> v2)
        {
            Contract.Assert(v1.ops == v2.ops);

            var ops = v1.ops;
            var vec = ops.Add(v1, v2);
            return new VectorWrapper<TVector>(vec, ops);
        }

        public static VectorWrapper<TVector> operator-(VectorWrapper<TVector> v)
        {
            return -1 * v;
        }

        public static VectorWrapper<TVector> operator-(VectorWrapper<TVector> v1, VectorWrapper<TVector> v2)
        {
            return v1 + (-v2);
        }
    }

    public class SOM<TVector, TNode>
        where TNode : class, ISOMNode<TVector>, new()
    {
        private readonly TNode[] nodes;

        public SOM(int numOfNodes)
        {
            nodes = new TNode[numOfNodes];
            for (int i = 0; i < numOfNodes; ++i)
                nodes[i] = new TNode();
        }

        public int Count
        {
            get { return nodes.Length; }
        }

        public TNode this[int index]
        {
            get { return nodes[index]; }
        }

        /// <summary>
        /// Trains the self organizing map according to kohonen algorithm.
        /// </summary>
        /// <param name="data">A finite sequence of data points to train with. Enumerated once per training iteration.</param>
        /// <param name="ops">The vector operations interface.</param>
        /// <param name="topoDistanceWeight">Topological distance function between nodes.</param>
        /// <param name="learningRates">Infinite enumeration of learning rates. Enumerated once for the whole training process.</param>
        /// <param name="threshold">Stopping theshold for the training algorithm. When the total change in a training iteration drops
        /// below this value, the training process stops.</param>
        public void Train(
            IEnumerable<TVector> data, 
            IOperations<TVector> ops, 
            Func<TNode, TNode, double> topoDistanceWeight, 
            IEnumerable<double> learningRates,
            double threshold)
        {
            Func<TVector, VectorWrapper<TVector>> wrap = v => new VectorWrapper<TVector>(v, ops);
            
            // perform learning
            using (var learningRatesEnumerator = learningRates.GetEnumerator())
            {
                // initialize values to ensure the first time we will get the learning rate from the enumerator.s
                double lastTotalChange = 0;
                double totalChange = double.MaxValue;
                double alpha = 0;

                while (true) // we loop until the break statement inside the loop is executed (threshold based).
                {
                    // we get the next (smaller) learning rate if the total change is too big.
                    if (totalChange >= 0.9 * lastTotalChange)
                    {
                        learningRatesEnumerator.MoveNext();
                        alpha = learningRatesEnumerator.Current;
                    }
                    lastTotalChange = totalChange;

                    totalChange = 0; // the stopping criterion. Checked after the following loop.
                    foreach (var sample in data) // feed the samples one by one
                    {
                        var wSample = wrap(sample);

                        // find best-matching node
                        var bestMatchingNode = nodes.Minimizer(node =>
                        {
                            var wPos = wrap(node.Position);
                            return (wPos - wSample).NormSquared;
                        });

                        // change positions of all nodes according to their distance from the bmu.
                        foreach (var node in nodes)
                        {
                            var theta = topoDistanceWeight(node, bestMatchingNode);
                            var wPos = wrap(node.Position);
                            var change = theta * alpha * (wSample - wPos);
                            node.Position = wPos + change;
                            totalChange += Math.Sqrt(change.NormSquared);
                        }
                    }

                    // check the stopping criterion and stop if needed.
                    if (totalChange < threshold)
                        break;
                }
            }
        }
    }
}
