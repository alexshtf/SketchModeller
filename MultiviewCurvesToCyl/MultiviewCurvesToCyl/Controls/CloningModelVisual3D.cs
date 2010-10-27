using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Media.Media3D;
using Utils;
using System.Windows;
using System.Diagnostics.Contracts;

namespace MultiviewCurvesToCyl.Controls
{
    class CloningModelVisual3D : ModelVisual3D
    {
        private readonly Model3DGroup group;
        private WeakDictionary<object, Model3D> dataToModel;
        private ICollectionView itemsView;

        public CloningModelVisual3D()
        {
            group = new Model3DGroup();
            itemsView = CollectionViewSource.GetDefaultView(System.Linq.Enumerable.Empty<object>());
            Content = group;
            dataToModel = new WeakDictionary<object, Model3D>();
        }

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(group != null);
            Contract.Invariant(dataToModel != null);

            // itemsView != null ==> the count of items and models is the same.
            Contract.Invariant(itemsView == null || itemsView.Cast<object>().Count() == group.Children.Count);

            // itemsView != null ==> for all items we have a mapping to the item's model in the dictionary.
            Contract.Invariant(itemsView == null || Contract.ForAll(itemsView.Cast<object>(), item => dataToModel.ContainsKey(item)));

            // for all items in data->model mapping, the model actually exists in the group children
            Contract.Invariant(Contract.ForAll(dataToModel, item => group.Children.Contains(item.Value)));

            // itemsView == null ==> both data-model mapping and group children have no items.
            Contract.Invariant(itemsView != null || (dataToModel.Count == 0 && group.Children.Count == 0));
        }

        #region ModelFactory property

        public static readonly DependencyProperty ModelFactoryProperty =
            DependencyProperty.Register("ModelFactory", typeof(IModelFactory), typeof(CloningModelVisual3D), new PropertyMetadata(OnModelFactoryChanged));

        public IModelFactory ModelFactory
        {
            get { return (IModelFactory)GetValue(ModelFactoryProperty); }
            set { SetValue(ModelFactoryProperty, value); }
        }

        private static void OnModelFactoryChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            sender.MatchClass<CloningModelVisual3D>(concrete => concrete.OnModelFactoryChanged());
        }

        #endregion

        #region ItemsSource property

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CloningModelVisual3D), new PropertyMetadata(OnItemsSourceChanged));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnItemsSourceChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            sender.MatchClass<CloningModelVisual3D>(concrete => concrete.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue));
        }

        #endregion

        #region property change notifications

        private void OnModelFactoryChanged()
        {
            HandleReset();
        }

        private void OnItemsSourceChanged(IEnumerable oldItemsSource, IEnumerable newItemsSource)
        {
            if (itemsView != null)
                itemsView.CollectionChanged -= new NotifyCollectionChangedEventHandler(ItemsSourceCollectionChanged);

            itemsView = CollectionViewSource.GetDefaultView(newItemsSource);

            if (itemsView != null)
            {
                itemsView.CollectionChanged += new NotifyCollectionChangedEventHandler(ItemsSourceCollectionChanged);
                HandleReset();
            }
            else // clear everything. We have no items source.
                Clear();
        }

        #endregion

        #region Items source collection change handling

        private void ItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // the count of items and models is the same.
            Contract.Ensures(itemsView.Cast<object>().Count() == group.Children.Count);
            // for all items we have a mapping to the item's model in the dictionary.
            Contract.Ensures(Contract.ForAll(itemsView.Cast<object>(), item => dataToModel.ContainsKey(item)));
            // for all items in data->model mapping, the model actually exists in the group children
            Contract.Ensures(Contract.ForAll(dataToModel, item => group.Children.Contains(item.Value)));

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveItems(e.OldItems);
                    AddItems(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    HandleReset();
                    break;
                default:
                    throw new NotSupportedException();
            }
        } 

        #endregion

        #region Items managment private methods
        
        private void HandleReset()
        {
            Clear();
            AddItems(itemsView);
        }

        private void Clear()
        {
            group.Children.Clear();
            dataToModel.Clear();
        }

        private void AddItems(IEnumerable items)
        {
            if (ModelFactory != null)
            {
                foreach (var item in items)
                {
                    var childModel = ModelFactory.Create(item);
                    group.Children.Add(childModel);
                    dataToModel.Add(item, childModel);
                }
            }
        }

        private void RemoveItems(IEnumerable items)
        {
            foreach (var item in items)
            {
                var model = dataToModel[item];
                dataToModel.Remove(item);
                group.Children.Remove(model);
            }
        } 

        #endregion
    }
}
