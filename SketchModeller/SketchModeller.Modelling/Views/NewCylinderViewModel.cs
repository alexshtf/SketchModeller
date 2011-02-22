using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Data;
using Microsoft.Practices.Unity;
using SketchModeller.Utilities;

using WpfPoint3D = System.Windows.Media.Media3D.Point3D;
using SketchModeller.Infrastructure;
using Microsoft.Practices.Prism.Commands;
using CollectionUtils = Utils.CollectionUtils;
using MathUtils3D = Utils.MathUtils3D;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Views
{
    public class NewCylinderViewModel : NewPrimitiveViewModel
    {
        private const double MIN_LENGTH = 0.01;
        private const double MIN_DIAMETER = 0.01;

        private NewCylinder model;
        private Dictionary<KeyboardEditModes, CheckedMenuCommandData> editModeToCommand;

        public NewCylinderViewModel()
        {
            diameter = 0.2;
            length = 0.5;
            axis = MathUtils3D.UnitZ;
            model = new NewCylinder();
        }

        [InjectionConstructor]
        public NewCylinderViewModel(UiState uiState)
            : this()
        {
            Contract.Requires(uiState != null);

            this.uiState = uiState;
            
            var editLength = 
                new CheckedMenuCommandData(new DelegateCommand(EditLengthExecute), title: "Edit length", isChecked: true);
            var editDiameter = 
                new CheckedMenuCommandData(new DelegateCommand(EditDiameterExecute), title: "Edit diameter");
            var pitch = 
                new CheckedMenuCommandData(new DelegateCommand(PitchExecute), title: "Edit pitch");
            var roll = 
                new CheckedMenuCommandData(new DelegateCommand(RollExecute), title: "Edit roll");
            editModeToCommand = new Dictionary<KeyboardEditModes, CheckedMenuCommandData>
            {
                { KeyboardEditModes.Length, editLength },
                { KeyboardEditModes.Diameter, editDiameter },
                { KeyboardEditModes.Pitch, pitch },
                { KeyboardEditModes.Roll, roll },
            };

            CollectionUtils.AddMany(ContextMenu, 
                editLength, 
                editDiameter, 
                pitch, 
                roll,
                new MenuCommandData(new DelegateCommand(ResetAxisExecute), "Reset axis"));
        }

        #region Command execute methods

        private void ResetAxisExecute()
        {
            Axis = uiState.SketchPlane.YAxis;
        }

        private void EditLengthExecute()
        {
            ChangeEditMode(KeyboardEditModes.Length);
        }

        private void EditDiameterExecute()
        {
            ChangeEditMode(KeyboardEditModes.Diameter);
        }

        private void PitchExecute()
        {
            ChangeEditMode(KeyboardEditModes.Pitch);
        }

        private void RollExecute()
        {
            ChangeEditMode(KeyboardEditModes.Roll);
        }

        private void ChangeEditMode(KeyboardEditModes newEditMode)
        {
            var matchingCommand = editModeToCommand[newEditMode];
            var isEnabled = matchingCommand.IsChecked;
            if (isEnabled)
            {
                var commandsToDisable = 
                    from command in editModeToCommand.Values
                    where command != matchingCommand
                    select command;

                foreach (var command in commandsToDisable)
                    command.IsChecked = false;
                KeyboardEditMode = newEditMode;
            }
        }

        #endregion

        internal void Initialize(NewCylinder newCylinder)
        {
            Contract.Requires(newCylinder != null);
            Contract.Requires(newCylinder.Axis != MathUtils3D.ZeroVector);
            Contract.Requires(newCylinder.Length > 0);
            Contract.Requires(newCylinder.Diameter > 0);

            Center = newCylinder.Center;
            Axis = newCylinder.Axis;
            Length = newCylinder.Length;
            Diameter = newCylinder.Diameter;
            model = newCylinder;
        }

        #region KeyboardEditMode property

        private KeyboardEditModes keyboardEditMode;

        public KeyboardEditModes KeyboardEditMode
        {
            get { return keyboardEditMode; }
            set
            {
                keyboardEditMode = value;
                RaisePropertyChanged(() => KeyboardEditMode);
            }
        }

        #endregion

        #region Axis property

        private Vector3D axis;

        public Vector3D Axis
        {
            get { return axis; }
            set
            {
                axis = value;
                RaisePropertyChanged(() => Axis);
                model.Axis = value;
            }
        }

        #endregion

        #region Diameter property

        private double diameter;

        public double Diameter
        {
            get { return diameter; }
            set
            {
                diameter = value;
                RaisePropertyChanged(() => Diameter);
                model.Diameter = value;
            }
        }

        #endregion

        #region Center property

        private WpfPoint3D center;

        public WpfPoint3D Center
        {
            get { return center; }
            set
            {
                center = value;
                RaisePropertyChanged(() => Center);
                model.Center = value;
            }
        }

        #endregion

        #region Length property

        private double length;

        public double Length
        {
            get { return length; }
            set
            {
                length = value;
                RaisePropertyChanged(() => Length);
                model.Length = value;
            }
        }

        #endregion
        
        public void Edit(int sign)
        {
            Contract.Requires(sign != 0);
            sign = Math.Sign(sign);

            switch (KeyboardEditMode)
            {
                case KeyboardEditModes.Length:
                    Length = Math.Max(MIN_LENGTH, Length + sign * 0.01);
                    break;
                case KeyboardEditModes.Diameter:
                    Diameter = Math.Max(MIN_DIAMETER, Diameter + sign * 0.01);
                    break;
                case KeyboardEditModes.Pitch:
                    Axis = RotationHelper.RotateVector(vector: Axis, rotateAxis: uiState.SketchPlane.XAxis, degrees: sign);
                    break;
                case KeyboardEditModes.Roll:
                    Axis = RotationHelper.RotateVector(vector: Axis, rotateAxis: uiState.SketchPlane.Normal, degrees: sign);
                    break;
                default:
                    break;
            }
        }

        public enum KeyboardEditModes
        {
            Length,
            Diameter,
            Pitch,
            Roll,
        }

    }
}
