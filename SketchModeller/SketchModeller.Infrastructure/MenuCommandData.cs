using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Input;

namespace SketchModeller.Infrastructure
{
    public class MenuCommandData : NotificationObject
    {
        public MenuCommandData(ICommand command, string title = "")
        {
            Command = command;
            Title = title;
        }

        public ICommand Command { get; private set; }

        #region Title property

        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        #endregion
    }
}
