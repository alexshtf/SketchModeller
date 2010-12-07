using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Regions;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace SketchModeller
{
    abstract class MultiItemRegionAdapterBase<TControl, TItem> : RegionAdapterBase<TControl>
        where TControl : class
        where TItem : class
    {
        private Func<TControl, IList<TItem>> getControlItems;

        protected MultiItemRegionAdapterBase(Func<TControl, IList<TItem>> getControlItems, IRegionBehaviorFactory regionBehaviorFactory)
            : base(regionBehaviorFactory)
        {
            this.getControlItems = getControlItems;
        }

        protected override void Adapt(IRegion region, TControl regionTarget)
        {
            region.Views.CollectionChanged += (sender, eventArgs) =>
            {
                var controlItems = getControlItems(regionTarget);
                switch (eventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        for (int i = 0; i < eventArgs.NewItems.Count; ++i)
                        {
                            var targetIdx = i + eventArgs.NewStartingIndex;
                            var child = (TItem)eventArgs.NewItems[i];
                            controlItems.Insert(targetIdx, child);
                        };
                        break;
                    case NotifyCollectionChangedAction.Move:
                        throw new NotSupportedException("Move operation is not supported");
                    case NotifyCollectionChangedAction.Remove:
                        for (int i = 0; i < eventArgs.OldItems.Count; ++i)
                            controlItems.RemoveAt(eventArgs.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        for (int i = 0; i < eventArgs.NewItems.Count; ++i)
                            controlItems[i + eventArgs.NewStartingIndex] =
                                (TItem)eventArgs.NewItems[i];
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        controlItems.Clear();
                        foreach (var item in region.Views.Cast<TItem>())
                            controlItems.Add(item);
                        break;
                    default:
                        break;
                }
            };
        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }
    }
}
