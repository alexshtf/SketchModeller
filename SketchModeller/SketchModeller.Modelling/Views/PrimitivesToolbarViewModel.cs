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
    public class PrimitivesToolbarViewModel : NotificationObject
    {
        private UiState uiState;
        private ISnapper snapper;

        public PrimitivesToolbarViewModel()
        {
        }

        [InjectionConstructor]
        public PrimitivesToolbarViewModel(UiState uiState, ISnapper snapper)
            : this()
        {
            Contract.Requires(uiState != null);
            Contract.Requires(snapper != null);

            this.uiState = uiState;
            this.snapper = snapper;

            SnapCommand = new DelegateCommand(SnapExecute, SnapCanExecute);
        }

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
    }
}
