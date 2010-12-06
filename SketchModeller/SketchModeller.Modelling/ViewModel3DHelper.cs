using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling
{
    static class ViewModel3DHelper
    {
        public static void InheritViewModel(this Visual3D root, object viewModel)
        {
            var bindings = GetAllBindings(root).ToArray();

            foreach (var bindingInfo in bindings)
            {
                var target = bindingInfo.Target;
                var dp = bindingInfo.TargetProperty;
                if (bindingInfo.Binding != null)
                {
                    var binding = bindingInfo.Binding;
                    if (IsBindingSourceless(binding))
                    {
                        binding = CloneBinding(binding);
                        binding.Source = viewModel;
                        BindingOperations.SetBinding(target, dp, binding);
                    }
                }
                else if (bindingInfo.MultiBinding != null)
                {
                    var muliBinding = new MultiBinding
                    {
                        Converter = bindingInfo.MultiBinding.Converter,
                        ConverterCulture = bindingInfo.MultiBinding.ConverterCulture,
                        ConverterParameter = bindingInfo.MultiBinding.ConverterParameter,
                        FallbackValue = bindingInfo.MultiBinding.FallbackValue,
                        Mode = bindingInfo.MultiBinding.Mode,
                        TargetNullValue = bindingInfo.MultiBinding.TargetNullValue,
                        StringFormat = bindingInfo.MultiBinding.StringFormat,
                        UpdateSourceTrigger = bindingInfo.MultiBinding.UpdateSourceTrigger,
                    };
                    foreach (var childBinding in bindingInfo.MultiBinding.Bindings)
                    {
                        var clone = CloneBinding(childBinding as Binding);
                        if (IsBindingSourceless(clone))
                            clone.Source = viewModel;
                        muliBinding.Bindings.Add(clone);
                    }
                    BindingOperations.SetBinding(target, dp, muliBinding);
                }
            }
        }

        private static Binding CloneBinding(Binding binding)
        {
            var result = new Binding
            {
                Path = binding.Path,
                Converter = binding.Converter,
                ConverterParameter = binding.ConverterParameter,
                UpdateSourceTrigger = binding.UpdateSourceTrigger,
                NotifyOnSourceUpdated = binding.NotifyOnSourceUpdated,
                NotifyOnTargetUpdated = binding.NotifyOnTargetUpdated,
                NotifyOnValidationError = binding.NotifyOnValidationError,
                FallbackValue = binding.FallbackValue,
                Mode = binding.Mode,
                XPath = binding.XPath,
                StringFormat = binding.StringFormat,
                AsyncState = binding.AsyncState,
                ConverterCulture = binding.ConverterCulture,
                TargetNullValue = binding.TargetNullValue,
            };
            if (binding.Source != null)
                result.Source = binding.Source;
            if (binding.RelativeSource != null)
                result.RelativeSource = binding.RelativeSource;
            if (!string.IsNullOrEmpty(binding.ElementName))
                result.ElementName = binding.ElementName;

            return result;
        }

        private static IEnumerable<BindingInfo> GetAllBindings(Visual3D root)
        {
            var freezableAndLocal = GetFreezableAndLocalBindings(root);
            var childrenBindings = VisualChildrenBindings(root).SelectMany(x => x);
            var result = freezableAndLocal.Concat(childrenBindings);
            return result;
        }

        private static IEnumerable<IEnumerable<BindingInfo>> VisualChildrenBindings(Visual3D root)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); ++i)
            {
                var visualChild = VisualTreeHelper.GetChild(root, i) as Visual3D;
                if (visualChild != null)
                    yield return GetAllBindings(visualChild);
            }
        }

        private static IEnumerable<BindingInfo> GetFreezableAndLocalBindings(DependencyObject root)
        {
            var localBindings =
                from pair in GetLocalBindings(root)
                select new BindingInfo
                {
                    Binding = pair.Item1 as Binding,
                    MultiBinding = pair.Item1 as MultiBinding,
                    Target = root,
                    TargetProperty = pair.Item2,
                };
            var childrenBindings = GetFreezableChildrenBindings(root).SelectMany(x => x);
            var result = localBindings.Concat(childrenBindings);
            return result;
        }

        private static IEnumerable<IEnumerable<BindingInfo>> GetFreezableChildrenBindings(DependencyObject root)
        {
            var lve = root.GetLocalValueEnumerator();
            while (lve.MoveNext())
            {
                var freezable = lve.Current.Value as Freezable;
                if (freezable != null)
                    yield return GetFreezableAndLocalBindings(freezable);
            }
        }

        private static IEnumerable<Tuple<BindingBase, DependencyProperty>> GetLocalBindings(DependencyObject root)
        {
            var localValueEnumerator = root.GetLocalValueEnumerator();
            while (localValueEnumerator.MoveNext())
            {
                var dp = localValueEnumerator.Current.Property;
                var binding = BindingOperations.GetBindingBase(root, dp);
                if (binding != null)
                    yield return Tuple.Create(binding, dp);
            }
        }

        private static bool IsBindingSourceless(Binding binding)
        {
            return
                binding.Source == null &&
                binding.RelativeSource == null &&
                string.IsNullOrEmpty(binding.ElementName);
        }

        private class BindingInfo
        {
            [ContractInvariantMethod]
            private void ObjectInvariantsMethod()
            {
                Contract.Invariant(Binding == null || MultiBinding == null);
            }

            public Binding Binding { get; set; }
            public MultiBinding MultiBinding { get; set; }
            public DependencyObject Target { get; set; }
            public DependencyProperty TargetProperty { get; set; }
        }

    }
}
