using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Unity;
using System.Collections.Specialized;
using System.Windows.Input;
using SketchModeller.Utilities;
using Microsoft.Practices.Prism.Commands;
using Utils;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Views
{
    public class SnappedPanelViewModel : NotificationObject
    {
        private readonly SessionData sessionData;

        public SnappedPanelViewModel()
        {
            SnappedPrimitives = ReadOnlyObservableCollection.Empty<SnappedPrimitive>();;
            Delete = new DelegateCommand(DeleteExecute, DeleteCanExecute);
            Duplicate = new DelegateCommand(DuplicateExecute, DuplicateCanExecute);
        }

        [InjectionConstructor]
        public SnappedPanelViewModel(SessionData sessionData)
            : this()
        {
            this.sessionData = sessionData;
            SnappedPrimitives = new ReadOnlyObservableCollection<SnappedPrimitive>(sessionData.SnappedPrimitives);
        }

        public ReadOnlyObservableCollection<SnappedPrimitive> SnappedPrimitives { get; private set; }

        #region SelectedSnappedPrimitive property

        private SnappedPrimitive selectedSnappedPrimitive;

        public SnappedPrimitive SelectedSnappedPrimitive
        {
            get { return selectedSnappedPrimitive; }
            set
            {
                if (selectedSnappedPrimitive != null)
                    selectedSnappedPrimitive.IsMarked = false;

                selectedSnappedPrimitive = value;
                RaisePropertyChanged(() => SelectedSnappedPrimitive);

                if (selectedSnappedPrimitive != null)
                    selectedSnappedPrimitive.IsMarked = true;

                ((DelegateCommand)Delete).RaiseCanExecuteChanged();
                ((DelegateCommand)Duplicate).RaiseCanExecuteChanged();
            }
        }

        #endregion

        public ICommand Delete { get; private set; }
        public ICommand Duplicate { get; private set; }

        #region Command handlers

        private void DeleteExecute()
        {
            sessionData.SnappedPrimitives.Remove(SelectedSnappedPrimitive);
        }

        private bool DeleteCanExecute()
        {
            return sessionData != null && SelectedSnappedPrimitive != null;
        }

        private void DuplicateExecute()
        {
            NewPrimitive newPrimitve = null;
            SelectedSnappedPrimitive.MatchClass<SnappedCylinder>(snappedCylinder =>
                {
                    newPrimitve = new NewCylinder
                    {
                        Axis = snappedCylinder.AxisResult,
                        Diameter = 2 * snappedCylinder.RadiusResult,
                        Length = snappedCylinder.LengthResult,
                        Center = MathUtils3D.Lerp(snappedCylinder.TopCenterResult, snappedCylinder.BottomCenterResult, 0.5),
                    };
                });
            Contract.Assert(newPrimitve != null);
            sessionData.NewPrimitives.Add(newPrimitve);
        }

        private bool DuplicateCanExecute()
        {
            return sessionData != null && SelectedSnappedPrimitive != null;
        }

        #endregion
    }
}
