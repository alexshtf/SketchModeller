using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Input;

namespace SketchModeller.Utilities
{
    public class ShortcutTextExtension : MarkupExtension
    {
        public ShortcutTextExtension()
        {
            Text = string.Empty;
        }

        public ShortcutTextExtension(string text)
        {
            Text = text;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return string.Format("{0} ({1})", Text, GetShortcutText());
        }

        private string GetShortcutText()
        {
            string shortcutText;
            if (Modifiers == ModifierKeys.None)
                shortcutText = new KeyConverter().ConvertToString(Key);
            else
                shortcutText = new KeyGestureConverter().ConvertToString(new KeyGesture(Key, Modifiers));
            return shortcutText;
        }

        public string Text { get; set; }
        public Key Key { get; set; }
        public ModifierKeys Modifiers { get; set; }
    }
}
