using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    public class VariableVectorsWriter
    {
        private readonly List<Variable> variables;

        public VariableVectorsWriter()
        {
            variables = new List<Variable>();
        }

        public int Length { get { return variables.Count; } }

        public Variable[] ToArray()
        {
            return variables.ToArray();
        }

        public VariableVectorsWriter Write(Variable variable)
        {
            Contract.Requires(variable != null);
            Contract.Ensures(Length - Contract.OldValue(Length) == 1);
            Contract.Ensures(Contract.Result<VariableVectorsWriter>() == this);

            variables.Add(variable);
            return this;
        }

        public VariableVectorsWriter Write(TVec vec)
        {
            Contract.Requires(vec != null);
            Contract.Requires(Contract.ForAll(0, vec.Dimension, i => vec[i] != null));
            Contract.Requires(Contract.ForAll(0, vec.Dimension, i => vec[i] is Variable));
            Contract.Ensures(Length - Contract.OldValue(Length) == vec.Dimension);
            Contract.Ensures(Contract.Result<VariableVectorsWriter>() == this);

            variables.AddRange(vec.GetTerms().Cast<Variable>());
            return this;
        }

        public VariableVectorsWriter WriteRange(IEnumerable<TVec> vectors)
        {
            Contract.Requires(vectors != null);
            Contract.Requires(Contract.ForAll(vectors, vec => vec != null));
            Contract.Ensures(Contract.Result<VariableVectorsWriter>() == this);

            foreach (var vec in vectors)
                Write(vec);
            return this;
        }
    }
}
