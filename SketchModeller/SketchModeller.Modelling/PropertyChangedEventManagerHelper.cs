using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Linq.Expressions;
using Microsoft.Practices.Prism.ViewModel;

namespace SketchModeller.Modelling
{
    public static class PropertyChangedEventManagerHelper
    {
        public static void AddListener<TProperty>(
            this INotifyPropertyChanged source, 
            IWeakEventListener listener,
            Expression<Func<TProperty>> expression)
        {
            PropertyChangedEventManager.AddListener(source, listener, PropertySupport.ExtractPropertyName(expression));
        }
    }
}
