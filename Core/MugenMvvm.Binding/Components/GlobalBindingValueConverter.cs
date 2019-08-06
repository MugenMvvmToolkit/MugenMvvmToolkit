using System;
using System.Linq;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Components
{
    public static class GlobalBindingValueConverter
    {
        #region Fields

        private static readonly ComponentTracker<IBindingValueConverterComponent, IComponent<IBindingManager>> Tracker =
            new ComponentTracker<IBindingValueConverterComponent, IComponent<IBindingManager>>();

        #endregion

        #region Methods

        public static void Initialize(IBindingManager bindingManager)
        {
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Tracker.Clear();
            Tracker.AddRange(bindingManager.Components.GetItems().OfType<IBindingValueConverterComponent>());
            bindingManager.Components.Components.Add(Tracker);
        }

        public static object? Convert(object? value, Type targetType, IBindingMemberInfo? member = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (Tracker.Count == 0)
            {
                if (targetType.IsInstanceOfTypeUnified(value))
                    return value;
                return System.Convert.ChangeType(value, targetType);
            }

            for (var i = 0; i < Tracker.Count; i++)
                value = Tracker[i].Convert(value, targetType, member, metadata);
            return value;
        }

        #endregion
    }
}