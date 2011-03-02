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
        void DragStart(LineRange startRay);
        void Drag(LineRange currRay);
        void DragEnd();
        bool IsDragging { get; }
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

        public void DragStart(LineRange startRay)
        {
            Contract.Requires(IsDragging == false);
            Contract.Ensures(IsDragging == true);
        }

        public void Drag(LineRange currRay)
        {
            Contract.Requires(IsDragging == true);
            throw new NotImplementedException();
        }

        public void DragEnd()
        {
            Contract.Requires(IsDragging == true);
            Contract.Ensures(IsDragging == false);
        }

        public bool IsDragging
        {
            get { return default(bool); }
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
