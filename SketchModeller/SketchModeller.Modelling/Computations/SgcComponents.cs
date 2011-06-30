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

        public static CylinderComponent[] GenerateComponents(
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
                };

                // if the distance between current and next is too high 
                // we insert a new component
                if (distance > maxDistance)
                {

                }
            }

            return list.ToArray();
        }
    }
}
