using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Diagnostics.Contracts;

namespace SketchModeller.Infrastructure.Data.EditConstraints
{
    [Serializable]
    public abstract class EditParameter<T> : NotificationObject, IEditParameter
    {
        private T value;

        public T Value
        {
            get { return value; }
            set 
            { 
                this.value = value; 
                RaisePropertyChanged(() => Value); 
            }
        }

        public static implicit operator T(EditParameter<T> parameter)
        {
            return parameter.value;
        }

        public abstract int Dimension { get; }

        public abstract double[] GetValues();

        public abstract void SetValues(double[] values);
    }
}
