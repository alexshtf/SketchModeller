using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Utils;
using System.Diagnostics.Contracts;
using MultiviewCurvesToCyl.Base;

namespace MultiviewCurvesToCyl
{
    class MenuCommandItem : BaseMenuViewModel
    {
        public MenuCommandItem(ICommand command, string title, KeyGesture keyGesture)
            : base(title)
        {
            Command = command;
            KeyGesture = keyGesture;
        }

        public ICommand Command { get; private set; }

        public KeyGesture KeyGesture { get; private set; }

        /// <summary>
        /// Creates a menu command item as delegate command
        /// </summary>
        /// <param name="title">the title of the command</param>
        /// <param name="action">The action that the delegate command will perform</param>
        /// <returns>A new menu command item</returns>
        public static MenuCommandItem Create(string title, Action<object> action, KeyGesture keyGesture = null)
        {
            Contract.Requires(!string.IsNullOrEmpty(title));
            Contract.Requires(action != null);
            Contract.Ensures(Contract.Result<MenuCommandItem>() != null);

            return new MenuCommandItem(new DelegateCommand(action), title, keyGesture);
        }
    }
}
