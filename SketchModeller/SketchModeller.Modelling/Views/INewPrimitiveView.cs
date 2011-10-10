using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils;
using Petzold.Media3D;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Views
{
    [ContractClass(typeof(NewPrimitiveViewContracts))]
    internal interface INewPrimitiveView
    {
        NewPrimitiveViewModel ViewModel { get; }
    }

    #region INewPrimitiveView contracts

    [ContractClassFor(typeof(INewPrimitiveView))]
    internal abstract class NewPrimitiveViewContracts : INewPrimitiveView
    {
        public NewPrimitiveViewModel ViewModel
        {
            get
            {
                Contract.Ensures(Contract.Result<NewPrimitiveViewModel>() != null);
                return null;
            }
        }

        public IEditor StartEdit(Point startPos, LineRange startRay)
        {
            Contract.Ensures(Contract.Result<IEditor>() != null);
            return default(IEditor);
        }
    }

    #endregion

    static class NewPrimitiveViewExtensions
    {
        public static INewPrimitiveView PrimitiveViewParent(this DependencyObject source)
        {
            return source.VisualPathUp().OfType<INewPrimitiveView>().FirstOrDefault();
        }
    }
}
