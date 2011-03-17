using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling
{
    /// <summary>
    /// Implemented by visual 3D objcts that 
    /// </summary>
    [ContractClass(typeof(FeatureCurveVisualContract))]
    interface IFeatureCurveVisual
    {
        /// <summary>
        /// Gets the feature curve this visual displays.
        /// </summary>
        FeatureCurve FeatureCurve { get; }

        /// <summary>
        /// Notifies the visual that it should update its appearance as a result of data change inside <see cref="FeatureCurve"/>.
        /// </summary>
        void Update();
    }

    [ContractClassFor(typeof(IFeatureCurveVisual))]
    abstract class FeatureCurveVisualContract : IFeatureCurveVisual
    {
        public FeatureCurve FeatureCurve
        {
            get 
            { 
                Contract.Ensures(Contract.Result<FeatureCurve>() != null); 
                return null; 
            }
        }

        public void Update()
        {
        }
    }

}
