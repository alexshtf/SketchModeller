using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.Collections.ObjectModel;
using System.Windows;
using System.Diagnostics.Contracts;
using System.Collections.Specialized;
using System.Collections;

namespace SketchModeller.Modelling
{
    class ViewModelCollectionGenerator<T> : IWeakEventListener
    {
        private IList<T> viewModels;
        private ObservableCollection<object> models;
        private Func<object, T> viewModelFactory;

        public ViewModelCollectionGenerator(
            IList<T> viewModels, 
            ObservableCollection<object> models,
            Func<object, T> viewModelFactory)
        {
            this.viewModels = viewModels;
            this.models = models;
            this.viewModelFactory = viewModelFactory;
            Reset();
            CollectionChangedEventManager.AddListener(models, this);
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(CollectionChangedEventManager))
                return false;

            var eventArgs = (NotifyCollectionChangedEventArgs)e;
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(eventArgs.NewStartingIndex, eventArgs.NewItems);
                    break;
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException("Move operations are not supported");
                case NotifyCollectionChangedAction.Remove:
                    Remove(eventArgs.OldStartingIndex, eventArgs.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Replace(eventArgs.NewStartingIndex, eventArgs.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;
                default:
                    break;
            }

            return true;
        }

        private void Replace(int index, IList items)
        {
            foreach (var item in items)
            {
                var viewModel = viewModelFactory(item);
                viewModels[index] = viewModel;
                ++index;
            }
        }

        private void Remove(int index, int count)
        {
            for (int i = 0; i < count; ++i)
                viewModels.RemoveAt(index);
        }

        private void Reset()
        {
            viewModels.Clear();
            foreach (var model in models)
            {
                var viewModel = viewModelFactory(model);
                viewModels.Add(viewModel);
            }
        }

        private void Add(int index, IList items)
        {
            foreach (var item in items)
            {
                var viewModel = viewModelFactory(item);
                viewModels.Insert(index, viewModel);
                ++index;
            }
        }
    }
}
