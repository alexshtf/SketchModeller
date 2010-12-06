using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Unity;
using System.Diagnostics.Contracts;
using System.Windows;
using System.ComponentModel;
using SketchModeller.Infrastructure;

namespace SketchModeller.Modelling.Views
{
    public class PrimitivesToolbarViewModel : NotificationObject, IWeakEventListener
    {
        private UiState uiState;

        public PrimitivesToolbarViewModel()
        {
            manipulationBlocker = new Blocker();
            cylinderBlocker = new Blocker();
            isManipulationMode = true;
        }

        [InjectionConstructor]
        public PrimitivesToolbarViewModel(UiState uiState)
            : this()
        {
            Contract.Requires(uiState != null);

            this.uiState = uiState;
            Update();

            uiState.AddListener(this, () => uiState.Tool);
        }

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(FlagsCount == 1);
        }

        #region IsManipulationMode property

        private bool isManipulationMode;
        private Blocker manipulationBlocker;

        public bool IsManipulationMode
        {
            get { return isManipulationMode; }
            set
            {
                manipulationBlocker.Do(() =>
                    {
                        isManipulationMode = value;
                        RaisePropertyChanged(() => IsManipulationMode);
                        uiState.Tool = Tool.Manipulation;
                    });
            }
        }

        #endregion

        #region IsCylinderMode property

        private bool isCylinderMode;
        private Blocker cylinderBlocker;

        public bool IsCylinderMode
        {
            get { return isCylinderMode; }
            set
            {
                cylinderBlocker.Do(() => 
                    {
                        isCylinderMode = value;
                        RaisePropertyChanged(() => IsCylinderMode);
                        uiState.Tool = Tool.InsertCylinder;
                    });
            }
        }

        #endregion

        [Pure] private int FlagsCount
        {
            get
            {
                bool[] flags = { IsManipulationMode, IsCylinderMode };
                return flags.Count(flag => flag == true);
            }
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
                return false;

            Update();

            manipulationBlocker.Do(() => RaisePropertyChanged(() => IsManipulationMode));
            cylinderBlocker.Do(() => RaisePropertyChanged(() => IsCylinderMode));

            return true;
        }

        private void Update()
        {
            isManipulationMode = uiState.Tool == Tool.Manipulation;
            isCylinderMode = uiState.Tool == Tool.InsertCylinder;
        }
    }
}
