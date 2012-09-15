using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SketchModeller.Infrastructure.Data.EditConstraints
{
    [ContractClass(typeof(EditParameterContracts))]
    public interface IEditParameter
    {
        int Dimension { get; }

        double[] GetValues();

        void SetValues(double[] values);
    }

    [ContractClassFor(typeof(IEditParameter))]
    abstract class EditParameterContracts : IEditParameter
    {

        public int Dimension
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return default(int);
            }
        }

        public double[] GetValues()
        {
            Contract.Ensures(Contract.Result<double[]>() != null);
            Contract.Ensures(Contract.Result<double[]>().Length == Dimension);
            return default(double[]);
        }

        public void SetValues(double[] values)
        {
            Contract.Requires(values != null);
            Contract.Requires(values.Length == Dimension);
        }
    }
}
