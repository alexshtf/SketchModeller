using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using MultiviewCurvesToCyl.Base;

namespace MultiviewCurvesToCyl
{
    class MenuCategoryItem : BaseMenuViewModel, IEnumerable<BaseMenuViewModel> // we must implement IEnumerable for collection initializers to work.
    {
        public MenuCategoryItem(string title)
            : base(title)
        {
            Children = new ObservableCollection<BaseMenuViewModel>();
        }

        public ObservableCollection<BaseMenuViewModel> Children { get; private set; }

        public void Add(BaseMenuViewModel child)
        {
            Children.Add(child);
        }

        IEnumerator<BaseMenuViewModel> IEnumerable<BaseMenuViewModel>.GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Children.GetEnumerator();
        }
    }
}
