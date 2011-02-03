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
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using SketchModeller.Infrastructure.Services;

namespace SketchModeller.Modelling.Views
{
    public class PrimitivesToolbarViewModel : NotificationObject, IWeakEventListener
    {
        private UiState uiState;
        private ISnapper snapper;

        public PrimitivesToolbarViewModel()
        {
            manipulationBlocker = new Blocker();
            cylinderBlocker = new Blocker();
            halfSphereBlocker = new Blocker();
            duplicateBlocker = new Blocker();
            isManipulationMode = true;
        }

        [InjectionConstructor]
        public PrimitivesToolbarViewModel(UiState uiState, ISnapper snapper)
            : this()
        {
            Contract.Requires(uiState != null);
            Contract.Requires(snapper != null);

            this.uiState = uiState;
            this.snapper = snapper;
            Update();

            uiState.AddListener(this, () => uiState.Tool);
            SnapCommand = new DelegateCommand(SnapExecute, SnapCanExecute);
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

        #region IsHalfSphereMode property

        private bool isHalfSphereMode;
        private Blocker halfSphereBlocker;

        public bool IsHalfSphereMode
        {
            get { return isHalfSphereMode; }
            set
            {
                halfSphereBlocker.Do(() =>
                {
                    isHalfSphereMode = value;
                    RaisePropertyChanged(() => IsHalfSphereMode);
                    uiState.Tool = Tool.InsertHalfSphere;
                });
            }
        }

        #endregion

        #region IsDuplicateMode property

        private bool isDuplicateMode;
        private Blocker duplicateBlocker;

        public bool IsDuplicateMode
        {
            get { return isDuplicateMode; }
            set
            {
                duplicateBlocker.Do(() =>
                    {
                        isDuplicateMode = value;
                        RaisePropertyChanged(() => IsDuplicateMode);
                        uiState.Tool = Tool.Duplicate;
                    });
            }
        }

        #endregion

        public ICommand SnapCommand { get; set; }

        #region snap command handlers

        private void SnapExecute()
        {
            snapper.Snap();
        }

        private bool SnapCanExecute()
        {
            // TODO: Ask the snapper?
            return true; 
        }

        #endregion

        [Pure] private int FlagsCount
        {
            get
            {
                bool[] flags = { IsManipulationMode, IsCylinderMode, IsHalfSphereMode, IsDuplicateMode };
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
            halfSphereBlocker.Do(() => RaisePropertyChanged(() => IsHalfSphereMode));
            duplicateBlocker.Do(() => RaisePropertyChanged(() => IsDuplicateMode));

            return true;
        }

        private void Update()
        {
            isManipulationMode = uiState.Tool == Tool.Manipulation;
            isCylinderMode = uiState.Tool == Tool.InsertCylinder;
            isHalfSphereMode = uiState.Tool == Tool.InsertHalfSphere;
            isDuplicateMode = uiState.Tool == Tool.Duplicate;
        }
    }
}
