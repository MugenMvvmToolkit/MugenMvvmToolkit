using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;
using MugenMvvm.Avalonia.Internal;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Collections;

[assembly: XmlnsDefinition("https://github.com/avaloniaui", "MugenMvvm.Avalonia.Bindings.Markup")]

namespace MugenMvvm.Avalonia.Bindings.Markup
{
    public static class Mugen
    {
        private static bool? _canBind;
        public static readonly AttachedProperty<string?> BindProperty = GetBindProperty();


        public static void SetBind(IAvaloniaObject element, string value) => element.SetValue(BindProperty, value);

        public static string? GetBind(IAvaloniaObject element) => element.GetValue(BindProperty);

        internal static bool CanBind()
        {
            if (_canBind == null)
            {
                if (Design.IsDesignMode)
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

        private static void OnBindChanged(AvaloniaObject target, AvaloniaPropertyChangedEventArgs e)
        {
            var bind = (string?) e.NewValue;
            if (string.IsNullOrEmpty(bind))
                return;

            if (Design.IsDesignMode)
            {
                if (CanBind())
                    BindDesignMode(target.Bind(bind!));
            }
            else
                target.Bind(bind!, includeResult: false);
        }

        private static AttachedProperty<string?> GetBindProperty()
        {
            var property = AvaloniaProperty.RegisterAttached<AvaloniaObjectAttachedValueStorageProvider, AvaloniaObject, string?>("Bind");
            property.Changed.AddClassHandler<AvaloniaObject>(OnBindChanged);
            return property;
        }
    }
}