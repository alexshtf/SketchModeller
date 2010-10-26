using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Utils;
using System.ComponentModel;
using System.Windows.Data;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Collections;
using System.Collections.Specialized;

namespace MultiviewCurvesToCyl
{
    class CloningVisual3D : ModelVisual3D
    {
        private readonly ModelVisual3D childrenRoot;
        private readonly WeakDictionary<object, Visual3D> dataToVisual;
        private ICollectionView itemsView;

        public CloningVisual3D()
        {
            childrenRoot = new ModelVisual3D();
            itemsView = CollectionViewSource.GetDefaultView(System.Linq.Enumerable.Empty<object>());
            dataToVisual = new WeakDictionary<object, Visual3D>();

            Children.Add(childrenRoot);
        }

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(childrenRoot != null);
            Contract.Invariant(dataToVisual != null);

            // itemsView != null ==> the count of items and models is the same.
            Contract.Invariant(itemsView == null || itemsView.Cast<object>().Count() == childrenRoot.Children.Count);

            // itemsView != null ==> for all items we have a mapping to the item's model in the dictionary.
            Contract.Invariant(itemsView == null || Contract.ForAll(itemsView.Cast<object>(), item => dataToVisual.ContainsKey(item)));

            // for all items in data->model mapping, the model actually exists in the group children
            Contract.Invariant(Contract.ForAll(dataToVisual, item => childrenRoot.Children.Contains(item.Value)));

            // itemsView == null ==> both data-model mapping and group children have no items.
            Contract.Invariant(itemsView != null || (dataToVisual.Count == 0 && childrenRoot.Children.Count == 0));
        }

        #region Visual3DFactory property

        public static readonly DependencyProperty Visual3DFactoryProperty =
            DependencyProperty.Register("Visual3DFactory", typeof(IVisual3DFactory), typeof(CloningVisual3D), new PropertyMetadata(OnVisual3DFactoryChanged));

        public IVisual3DFactory Visual3DFactory
        {
            get { return (IVisual3DFactory)GetValue(Visual3DFactoryProperty); }
            set { SetValue(Visual3DFactoryProperty, value); }
        }

        private static void OnVisual3DFactoryChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            sender.MatchClass<CloningVisual3D>(concrete => concrete.OnVisual3DFactoryChanged());
        }

        #endregion

        #region ItemsSource property

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CloningVisual3D), new PropertyMetadata(OnItemsSourceChanged));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnItemsSourceChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            sender.MatchClass<CloningVisual3D>(concrete => concrete.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue));
        }

        #endregion

        #region Property change notifications

        private void OnVisual3DFactoryChanged()
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
            Contract.Ensures(itemsView.Cast<object>().Count() == childrenRoot.Children.Count);
            // for all items we have a mapping to the item's model in the dictionary.
            Contract.Ensures(Contract.ForAll(itemsView.Cast<object>(), item => dataToVisual.ContainsKey(item)));
            // for all items in data->model mapping, the model actually exists in the group children
            Contract.Ensures(Contract.ForAll(dataToVisual, item => childrenRoot.Children.Contains(item.Value)));

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
            childrenRoot.Children.Clear();
            dataToVisual.Clear();
        }

        private void AddItems(IEnumerable items)
        {
            if (Visual3DFactory != null)
            {
                foreach (var item in items)
                {
                    var childVisual = Visual3DFactory.Create(item);
                    childrenRoot.Children.Add(childVisual);
                    dataToVisual.Add(item, childVisual);
                }
            }
        }

        private void RemoveItems(IEnumerable items)
        {
            foreach (var item in items)
            {
                var model = dataToVisual[item];
                dataToVisual.Remove(item);
                childrenRoot.Children.Remove(model);
            }
        }

        #endregion
    }
}
