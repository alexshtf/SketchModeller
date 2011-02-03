using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Utils
{
    /// <summary>
    /// Exposes a read-only collection of selected items in a given collection based on a boolean property.
    /// </summary>
    /// <typeparam name="T">The type of the selected items.</typeparam>
    public class SelectionListener<T> : IWeakEventListener
          where T : INotifyPropertyChanged
    {
        private ICollection<T> sourceCollection;
        private readonly Func<T, bool> isSelected;
        private readonly string isSelectedPropertyName;
        private readonly ObservableCollection<T> selectedItemsInternal;

        /// <summary>
        /// Constructs a new instance of the <see cref="SelectionListener{T}"/> type.
        /// </summary>
        /// <param name="sourceCollection">The collection to listen to.</param>
        /// <param name="isSelected">A lambda expression with the selection property of each item.</param>
        /// <remarks>
        /// The expression in <paramref name="isSelected"/> must be a simple expression getter property on an item. That is,
        /// it must look like this: <c>item => item.Property</c>, where <c>Property</c> is the boolean property telling
        /// wether the item is selected or not.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="isSelected"/> is not a simple property
        /// access expression on the item.</exception>
        public SelectionListener(ICollection<T> sourceCollection, Expression<Func<T, bool>> isSelected)
        {
            Contract.Requires(sourceCollection != null);
            Contract.Requires(isSelected != null);

            this.sourceCollection = sourceCollection;
            this.isSelected = isSelected.Compile();
            this.isSelectedPropertyName = GetPropertyName(isSelected);
            selectedItemsInternal = new ObservableCollection<T>();
            SelectedItems = new ReadOnlyObservableCollection<T>(selectedItemsInternal);

            var notifyCollectionChanged = sourceCollection as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
                CollectionChangedEventManager.AddListener(notifyCollectionChanged, this);

            HandleNewItems(sourceCollection);
        }

        /// <summary>
        /// Gets an observable collection of the selected items.
        /// </summary>
        public ReadOnlyObservableCollection<T> SelectedItems { get; private set; }

        /// <summary>
        /// Assigns a new collection to listen to.
        /// </summary>
        /// <param name="newSourceCollection">The new collection to expose its selected items.</param>
        public void Reset(ICollection<T> newSourceCollection)
        {
            Contract.Requires(newSourceCollection != null);
            
            selectedItemsInternal.Clear();
            HandleOldItems(sourceCollection);

            sourceCollection = newSourceCollection;
            HandleNewItems(sourceCollection);

            var notifyCollectionChanged = sourceCollection as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
                CollectionChangedEventManager.AddListener(notifyCollectionChanged, this);
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(CollectionChangedEventManager))
            {
                OnCollectionChanged(sender, (NotifyCollectionChangedEventArgs)e);
                return true;
            }
            if (managerType == typeof(PropertyChangedEventManager))
            {
                OnPropertyChanged(sender, (PropertyChangedEventArgs)e);
                return true;
            }

            return false;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var item = (T)sender;
            var isCurrentlySelected = isSelected(item);
            if (isCurrentlySelected && !selectedItemsInternal.Contains(item))
                selectedItemsInternal.Add(item);

            if (!isCurrentlySelected)
                selectedItemsInternal.Remove(item);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems != null ? e.NewItems.Cast<T>() : System.Linq.Enumerable.Empty<T>();
            var oldItems = e.OldItems != null ? e.OldItems.Cast<T>() : System.Linq.Enumerable.Empty<T>();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    HandleNewItems(newItems);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    HandleOldItems(oldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    HandleOldItems(oldItems);
                    HandleNewItems(newItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    HandleOldItems(oldItems);
                    HandleNewItems(sourceCollection);
                    break;
                default:
                    break;
            }
        }

        private void HandleNewItems(IEnumerable<T> newItems)
        {
            foreach (var item in newItems)
            {
                if (isSelected(item))
                    selectedItemsInternal.Add(item);
                PropertyChangedEventManager.AddListener(item, this, isSelectedPropertyName);
            }
        }

        private void HandleOldItems(IEnumerable<T> oldItems)
        {
            foreach (var item in oldItems)
            {
                selectedItemsInternal.Remove(item);
                PropertyChangedEventManager.RemoveListener(item, this, isSelectedPropertyName);
            }
        }

        private static string GetPropertyName(Expression<Func<T, bool>> propertyGetter)
        {
            var body = propertyGetter.Body;
            if (body.NodeType != ExpressionType.MemberAccess)
                throw new InvalidOperationException("Lambda expression body is not member-access");


            var memberExpression = body as MemberExpression;
            var member = memberExpression.Member as PropertyInfo;
            if (member == null)
                throw new InvalidOperationException("Lambda expression is not a property-access expression");

            var memberParameter = memberExpression.Expression as ParameterExpression;
            if (memberParameter == null)
                throw new InvalidOperationException("Lambda expression is not a property-access on the parameter");

            var lambdaParameter = propertyGetter.Parameters[0];
            if (memberParameter != lambdaParameter)
                throw new InvalidOperationException("Lambda expression does not access a property of its own parameter");

            var propertyName = member.Name;
            return propertyName;
        }
    }
}
