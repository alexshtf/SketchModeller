using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure;
using Microsoft.Practices.Unity;
using System.Diagnostics.Contracts;
using Utils;
using SketchModeller.Utilities;

using NewHalfSphere = SketchModeller.Infrastructure.Data.NewHalfSphere;
using SketchPlane = SketchModeller.Infrastructure.Data.SketchPlane;
using Microsoft.Practices.Prism.Commands;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Views
{
    public class NewHalfSphereViewModel : NewPrimitiveViewModel
    {
        private const double MIN_LENGTH = 0.01;
        private const double MIN_RADIUS = 0.01 / 2; // minimum diameter is 0.01

        private NewHalfSphere model;
        private Dictionary<KeyboardEditModes, CheckedMenuCommandData> editModeToCommand;

        public NewHalfSphereViewModel()
        {
            radius = 0.5;
            length = 0.5;
            axis = MathUtils3D.UnitZ;
        }

        [InjectionConstructor]
        public NewHalfSphereViewModel(UiState uiState)
        {
            Contract.Requires(uiState != null);

            this.uiState = uiState;
            model = new NewHalfSphere();

            // add edit mode commands
            var commandsData = new Tuple<KeyboardEditModes, string>[]
            {
                Tuple.Create(KeyboardEditModes.Length, "Edit length"),
                Tuple.Create(KeyboardEditModes.Radius, "Edit radius"),
                Tuple.Create(KeyboardEditModes.Pitch, "Edit pitch"),
                Tuple.Create(KeyboardEditModes.Roll, "Edit roll"),
            };

            editModeToCommand = new Dictionary<KeyboardEditModes, CheckedMenuCommandData>();
            bool isChecked = true;
            foreach (var tuple in commandsData)
            {
                var mode = tuple.Item1;
                var title = tuple.Item2;
                var cmd = new CheckedMenuCommandData(new DelegateCommand(() => ChangeEditMode(mode)), title, isChecked);
                isChecked = false; // only the first command will be checked
                editModeToCommand[mode] = cmd;
                ContextMenu.Add(cmd);
            }

            ContextMenu.Add(new MenuCommandData(new DelegateCommand(ResetAxisExecute), title: "Reset axis"));
        }

        internal void Initialize(NewHalfSphere halfSphere)
        {
            Contract.Requires(halfSphere != null);
            Contract.Requires(halfSphere.Axis != MathUtils3D.ZeroVector);
            Contract.Requires(halfSphere.Length > 0);
            Contract.Requires(halfSphere.Radius > 0);

            Center = halfSphere.Center;
            Axis = halfSphere.Axis;
            Length = halfSphere.Length;
            Radius = halfSphere.Radius;
            this.model = halfSphere;
        }

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

        #region Radius property

        private double radius;

        public double Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                RaisePropertyChanged(() => Radius);
                model.Radius = value;
            }
        }

        #endregion

        #region Center property

        private Point3D center;

        public Point3D Center
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

        public void Edit(int sign)
        {
            switch (KeyboardEditMode)
            {
                case KeyboardEditModes.Length:
                    Length = Math.Max(MIN_LENGTH, Length + sign * 0.01);
                    break;
                case KeyboardEditModes.Radius:
                    Radius = Math.Max(MIN_RADIUS, Radius + sign * 0.01);
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

        private void ResetAxisExecute()
        {
            Axis = uiState.SketchPlane.YAxis;
        }

        public enum KeyboardEditModes
        {
            Length,
            Radius,
            Pitch,
            Roll,
        }
    }
}
