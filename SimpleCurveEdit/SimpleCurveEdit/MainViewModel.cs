using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SimpleCurveEdit
{
    class MainViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<ITool> tools;
        private ITool currentTool;

        public MainViewModel()
        {
            tools = new ObservableCollection<ITool>();
        }

        public ObservableCollection<ITool> Tools
        {
            get { return tools; }
        }

        public ITool CurrentTool
        {
            get { return currentTool; }
            set
            {
                currentTool = value;
                NotifyPropertyChanged("CurrentTool");
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
