using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Utils;

namespace MultiviewCurvesToCyl.MeshGeneration
{
    class ConstrainedMesh
    {
        protected HashSet<int> constrainedPositionIndices;

        public ConstrainedMesh()
        {
            Positions = new Collection<Point3D>();
            Normals = new Collection<Vector3D>();
            TriangleIndices = new Collection<Tuple<int, int, int>>();
            ConstrainedPositionIndices = (constrainedPositionIndices = new HashSet<int>()).AsReadOnly();
        }

        public Collection<Point3D> Positions { get; private set; }
        public Collection<Vector3D> Normals { get; private set; }
        public Collection<Tuple<int, int, int>> TriangleIndices { get; private set; }
        public ReadOnlySet<int> ConstrainedPositionIndices { get; private set; }

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            // basic invariants
            Contract.Invariant(Positions != null);
            Contract.Invariant(Normals != null);
            Contract.Invariant(TriangleIndices != null);
            Contract.Invariant(ConstrainedPositionIndices != null);
        }
    }
}
