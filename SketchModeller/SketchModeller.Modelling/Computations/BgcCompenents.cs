using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Computations
{
    static class BgcComponents
    {
        public const double MIN_COMPONENTS_DISTANCE = 0.02;
        public const double MAX_COMPONENTS_DISTANCE = 0.05;

        public static BendedCylinderComponent[] Create(int count, double r1, double r2, Point3D Center, Vector3D Axis, double Length)
        {
            Contract.Requires(count >= 2);
            Contract.Requires(r1 > 0);
            Contract.Requires(r2 > 0);
            Contract.Ensures(Contract.Result<BendedCylinderComponent[]>() != null);
            Contract.Ensures(Contract.Result<BendedCylinderComponent[]>().Length == count);
            Contract.Ensures(Contract.Result<BendedCylinderComponent[]>()[0].Radius == r1);
            Contract.Ensures(Contract.Result<BendedCylinderComponent[]>().Last().Radius == r2);
            //MessageBox.Show("InsideBgcComponents");
            var resultQuery =
                from i in Enumerable.Range(0, count)
                let t = i / (double)(count - 1)
                let r = (1 - t) * r1 + t * r2
                let Start = Center - 0.5*Length*Axis 
                let End = Center + 0.5*Length*Axis
                let Pt = (1-t)*(new Vector3D(Start.X, Start.Y, Start.Z)) + t*(new Vector3D(End.X, End.Y, End.Z))
                select new BendedCylinderComponent(r, t, new Point3D(Pt.X, Pt.Y, Pt.Z), new Point(Pt.X, Pt.Y));

            return resultQuery.ToArray();
        }

        public static BendedCylinderComponent[] Update(
            double length,
            BendedCylinderComponent[] oldComponents,
            double minDistance = MIN_COMPONENTS_DISTANCE,
            double maxDistance = MAX_COMPONENTS_DISTANCE)
        {
            Contract.Requires(length > 0);
            Contract.Requires(oldComponents != null);
            Contract.Requires(oldComponents.Length >= 2);
            Contract.Ensures(Contract.Result<BendedCylinderComponent[]>() != null);
            Contract.Ensures(Contract.Result<BendedCylinderComponent[]>().Length >= 2);

            var list = new LinkedList<BendedCylinderComponent>(oldComponents);
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
