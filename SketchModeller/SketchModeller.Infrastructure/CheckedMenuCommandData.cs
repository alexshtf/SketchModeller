using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SketchModeller.Infrastructure
{
    public class CheckedMenuCommandData : MenuCommandData
    {
        public CheckedMenuCommandData(ICommand command, string title = "", bool isChecked = false)
            : base(command, title)
        {
            this.isChecked = isChecked;
        }

        #region IsChecked property

        private bool isChecked;

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                RaisePropertyChanged(() => IsChecked);
            }
        }

        #endregion
    }
}
