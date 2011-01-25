using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Xml.Serialization;

namespace SketchModeller.Infrastructure.Data
{
    public abstract class NewPrimitive : NotificationObject, ICloneable
    {
        #region IsSelected property

        private bool isSelected;

        [XmlIgnore]
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        #endregion

        public abstract NewPrimitive Clone();

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
