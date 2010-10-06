using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiviewCurvesToCyl.MeshGeneration
{
    class IndexedAttributeUpdateEventArgs : EventArgs
    {
        public int Index { get; private set; }

        public IndexedAttributeUpdateEventArgs(int index)
        {
            Index = index;
        }
    }
}
