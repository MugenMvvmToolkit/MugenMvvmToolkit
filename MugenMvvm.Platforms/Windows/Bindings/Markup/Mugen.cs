using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Collections;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "MugenMvvm.Windows.Bindings.Markup")]

namespace MugenMvvm.Windows.Bindings.Markup
{
    public static class Mugen
    {
        public static readonly DependencyProperty BindProperty = DependencyProperty.RegisterAttached(
            "Bind", typeof(string), typeof(Mugen), new PropertyMetadata(default(string), OnBindChanged));

        private static bool? _canBind;
        private static bool? _isDesignMode;


        public static void SetBind(DependencyObject element, string? value) => element.SetValue(BindProperty, value);

        public static string? GetBind(DependencyObject element) => (string?)element.GetValue(BindProperty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsInDesignMode()
        {
            _isDesignMode ??= (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue;
            return _isDesignMode.Value;
        }

        internal static bool CanBind()
        {
            if (_canBind == null)
            {
                if (IsInDesignMode())
                    _canBind = MugenService.Optional<IBindingManager>() != null;
                else
                    _canBind = true;
            }

            return _canBind.Value;
        }

        internal static void BindDesignMode(ItemOrIReadOnlyList<IBinding> bindings)
        {
            foreach (var binding in bindings)
                BindDesignMode(binding);
        }

        internal static void BindDesignMode(IBinding binding)
        {
            if (binding is InvalidBinding b)
                throw b.Exception;
        }

        private static void OnBindChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var bind = (string?)e.NewValue;
            if (string.IsNullOrEmpty(bind))
                return;

            if (IsInDesignMode())
            {
                if (CanBind())
                    BindDesignMode(target.Bind(bind!));
            }
            else
                target.Bind(bind!, includeResult: false);
        }
    }
}