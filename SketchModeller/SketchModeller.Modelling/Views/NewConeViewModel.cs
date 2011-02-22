using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure;
using System.Windows.Media.Media3D;
using Utils;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Commands;
using System.Diagnostics.Contracts;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Views
{
    public class NewConeViewModel : NewPrimitiveViewModel
    {
        private const double MIN_LENGTH = 0.01;
        private const double MIN_DIAMETER = 0.01;

        private readonly Dictionary<KeyboardEditModes, CheckedMenuCommandData> editModeToCommand;
        private NewCone model;

        [InjectionConstructor]
        public NewConeViewModel(UiState uiState = null)
            : base(uiState)
        {
            topRadius = 0.2;
            bottomRadius = 0.2;
            length = 0.5;
            axis = MathUtils3D.UnitZ;
            center = MathUtils3D.Origin;

            model = new NewCone();
            UpdateModel();

            var editLength =
                new CheckedMenuCommandData(new DelegateCommand(EditLengthExecute), title: "Edit length", isChecked: true);
            var editTopRadius =
                new CheckedMenuCommandData(new DelegateCommand(EditTopRadius), title: "Edit top radius");
            var editBottomRadius =
                new CheckedMenuCommandData(new DelegateCommand(EditBottomRadius), title: "Edit bottom radius");
            var editPitch =
                new CheckedMenuCommandData(new DelegateCommand(EditPitchExecute), title: "Edit pitch");
            var editRoll =
                new CheckedMenuCommandData(new DelegateCommand(EditRollExecute), title: "Edit roll");

            editModeToCommand = new Dictionary<KeyboardEditModes, CheckedMenuCommandData>
            {
                { KeyboardEditModes.Length, editLength },
                { KeyboardEditModes.TopRadius, editTopRadius },
                { KeyboardEditModes.BottomRadius, editBottomRadius},
                { KeyboardEditModes.Pitch, editPitch },
                { KeyboardEditModes.Roll, editRoll },
            };

            CollectionUtils.AddMany(ContextMenu,
                editLength,
                editTopRadius,
                editBottomRadius,
                editPitch,
                editRoll,
                new MenuCommandData(new DelegateCommand(ResetAxisExecute), "Reset axis"));
        }

        public void Init(NewCone newModel)
        {
            this.model = newModel;
            Axis = model.Axis;
            Center = model.Center;
            Length = model.Length;
            TopRadius = model.TopRadius;
            BottomRadius = model.BottomRadius;
        }

        #region KeyboardEditMode property

        private KeyboardEditModes keyboardEditMode;

        internal KeyboardEditModes KeyboardEditMode
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
            }
        }

        #endregion

        #region TopRadius property

        private double topRadius;

        public double TopRadius
        {
            get { return topRadius; }
            set
            {
                topRadius = value;
                RaisePropertyChanged(() => TopRadius);
            }
        }

        #endregion

        #region BottomRadius property

        private double bottomRadius;

        public double BottomRadius
        {
            get { return bottomRadius; }
            set
            {
                bottomRadius = value;
                RaisePropertyChanged(() => BottomRadius);
            }
        }

        #endregion

        #region Command execute methods

        private void EditLengthExecute()
        {
            ChangeEditMode(KeyboardEditModes.Length);
        }

        private void EditTopRadius()
        {
            ChangeEditMode(KeyboardEditModes.TopRadius);
        }

        private void EditBottomRadius()
        {
            ChangeEditMode(KeyboardEditModes.BottomRadius);
        }

        private void EditPitchExecute()
        {
            ChangeEditMode(KeyboardEditModes.Pitch);
        }

        private void EditRollExecute()
        {
            ChangeEditMode(KeyboardEditModes.Roll);
        }

        private void ResetAxisExecute()
        {
            Axis = uiState.SketchPlane.YAxis;
        }

        #endregion

        public void Edit(int sign)
        {
            Contract.Requires(sign != 0);

            switch (KeyboardEditMode)
            {
                case KeyboardEditModes.Length:
                    Length = Math.Max(MIN_LENGTH, Length + sign * 0.01);
                    break;
                case KeyboardEditModes.TopRadius:
                    TopRadius = Math.Max(MIN_DIAMETER, TopRadius + sign * 0.01);
                    break;
                case KeyboardEditModes.BottomRadius:
                    BottomRadius = Math.Max(MIN_DIAMETER, BottomRadius + sign * 0.01);
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

        private void ChangeEditMode(KeyboardEditModes keyboardEditModes)
        {
            var matchingCommand = editModeToCommand[keyboardEditModes];
            var isEnabled = matchingCommand.IsChecked;
            if (isEnabled)
            {
                var commandsToDisable =
                    from command in editModeToCommand.Values
                    where command != matchingCommand
                    select command;

                foreach (var command in commandsToDisable)
                    command.IsChecked = false;
                KeyboardEditMode = keyboardEditModes;
            }
        }

        protected override void RaisePropertyChanged(string propertyName)
        {
            base.RaisePropertyChanged(propertyName);
            UpdateModel();
        }

        private void UpdateModel()
        {
            model.Axis = axis;
            model.Center = center;
            model.Length = length;
            model.TopRadius = topRadius;
            model.BottomRadius = bottomRadius;
        }

        internal enum KeyboardEditModes
        {
            Length,
            TopRadius,
            BottomRadius,
            Pitch,
            Roll,
        }
    }
}
