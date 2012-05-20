using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Computations
{
    static class SgcComponents
    {
        public const double MIN_COMPONENTS_DISTANCE = 0.02;
        public const double MAX_COMPONENTS_DISTANCE = 0.05;

        public static CylinderComponent[] CreateNonLinear(int count, double r1, double r2, double r3)
        {
            Contract.Requires(count >= 2);
            Contract.Requires(r1 > 0);
            Contract.Requires(r2 > 0);
            Contract.Requires(r3 > 0);
            Contract.Ensures(Contract.Result<CylinderComponent[]>() != null);
            Contract.Ensures(Contract.Result<CylinderComponent[]>().Length == count);
            Contract.Ensures(Contract.Result<CylinderComponent[]>()[0].Radius == r1);
            Contract.Ensures(Contract.Result<CylinderComponent[]>().Last().Radius == r3);

            // coefficients of a quadratic function r(t) = a + bt + ct² such that
            // r(0) = r1, r(0.5) = r2, r(1) = r3.
            var a = r1;
            var b = -r3 + 4 * r2 - 3 * r1;
            var c = 2 * r3 - 4 * r2 + 2 * r1;

            // create a set of radii according to the above function.
            var resultQuery =
                from i in Enumerable.Range(0, count)
                let t = i / (double)(count - 1)
                let r = a + b * t + c * t * t
                select new CylinderComponent(r, t);

            return resultQuery.ToArray();
        }

        public static CylinderComponent[] Create(int count, double r1, double r2)
        {
            Contract.Requires(count >= 2);
            Contract.Requires(r1 > 0);
            Contract.Requires(r2 > 0);
            Contract.Ensures(Contract.Result<CylinderComponent[]>() != null);
            Contract.Ensures(Contract.Result<CylinderComponent[]>().Length == count);
            Contract.Ensures(Contract.Result<CylinderComponent[]>()[0].Radius == r1);
            Contract.Ensures(Contract.Result<CylinderComponent[]>().Last().Radius == r2);

            var resultQuery =
                from i in Enumerable.Range(0, count)
                let t = i / (double)(count - 1)
                let r = (1 - t) * r1 + t * r2
                select new CylinderComponent(r, t);

            return resultQuery.ToArray();
        }

        public static CylinderComponent[] Update(
            double length,
            CylinderComponent[] oldComponents,
            double minDistance = MIN_COMPONENTS_DISTANCE,
            double maxDistance = MAX_COMPONENTS_DISTANCE)
        {
            Contract.Requires(length > 0);
            Contract.Requires(oldComponents != null);
            Contract.Requires(oldComponents.Length >= 2);
            Contract.Ensures(Contract.Result<CylinderComponent[]>() != null);
            Contract.Ensures(Contract.Result<CylinderComponent[]>().Length >= 2);

            var list = new LinkedList<CylinderComponent>(oldComponents);
            var currentNode = list.First;
            while (currentNode != list.Last)
            {
                var current = currentNode.Value;
                var next = currentNode.Next.Value;

                var distance = (next.Progress - current.Progress) * length;

                // if the distance between current and next is too low 
                // we remove next (unless it's the last node)
                if (distance < minDistance && currentNode.Next != list.Last)
                {
                    throw new NotImplementedException("TODO");
                };

                // if the distance between current and next is too high 
                // we insert a new component
                if (distance > maxDistance)
                {
                    throw new NotImplementedException("TODO");
                }
            }

            return list.ToArray();
        }
    }
}
