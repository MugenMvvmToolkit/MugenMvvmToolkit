using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Components
{
    public static class GlobalBindingValueConverter
    {
        #region Fields

        private static readonly ComponentTracker<IBindingValueConverterComponent, IBindingManager> Tracker =
            new ComponentTracker<IBindingValueConverterComponent, IBindingManager>();

        #endregion

        #region Methods

        public static void Initialize(IBindingManager bindingManager)
        {
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Tracker.Attach(bindingManager);
        }

        public static object? Convert(object? value, Type targetType, IBindingMemberInfo? member = null, IReadOnlyMetadataContext? metadata = null)
        {
            var components = Tracker.GetComponents();
            if (components.Length == 0)
            {
                if (targetType.IsInstanceOfTypeUnified(value))
                    return value;
                return System.Convert.ChangeType(value, targetType);
            }

            for (var i = 0; i < components.Length; i++)
                value = components[i].Convert(value, targetType, member, metadata);
            return value;
        }

        #endregion
    }
}