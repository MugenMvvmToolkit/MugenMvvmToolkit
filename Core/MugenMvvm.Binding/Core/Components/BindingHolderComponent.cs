using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingHolderComponent : IBindingHolderComponent
    {
        #region Fields

        private readonly IAttachedValueManager? _attachedValueManager;

        private const string BindPrefix = "@#b";

        private static readonly UpdateValueDelegate<object, IBinding, IBinding, IBinding, IBinding> UpdateBindingDelegate = UpdateBinding;

        #endregion

        #region Constructors

        public BindingHolderComponent(IAttachedValueManager? attachedValueManager = null)
        {
            _attachedValueManager = attachedValueManager;
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBinding?, IReadOnlyList<IBinding>> TryGetBindings(object target, string? path, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(target, nameof(target));
            var attachedValueProvider = _attachedValueManager.ServiceIfNull().GetAttachedValueProvider(target, metadata);
            if (attachedValueProvider == null)
                return default;

            var values = path == null
                ? attachedValueProvider.GetValues(target, target, (o, s, arg3) => s.StartsWith(BindPrefix, StringComparison.Ordinal))
                : attachedValueProvider.GetValues(target, path, (o, s, arg3) => s.StartsWith(BindPrefix, StringComparison.Ordinal) && s.EndsWith(s, StringComparison.Ordinal));

            if (values.Count == 0)
                return default;
            if (values.Count == 1)
                return new ItemOrList<IBinding?, IReadOnlyList<IBinding>>((IBinding)values[0].Value);

            var bindings = new IBinding[values.Count];
            for (var i = 0; i < bindings.Length; i++)
                bindings[i] = (IBinding)values[i].Value;
            return bindings;
        }

        public bool TryRegister(IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            var source = binding.Target.Source;
            if (source == null)
                return false;

            var attachedValueProvider = _attachedValueManager.ServiceIfNull().GetOrAddAttachedValueProvider(source, metadata);
            attachedValueProvider.AddOrUpdate(source, BindPrefix + binding.Target.Path.Path, binding, binding, binding, UpdateBindingDelegate);
            return true;
        }

        public bool TryUnregister(IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            var source = binding.Target.Source;
            if (source == null)
                return false;

            var attachedValueProvider = _attachedValueManager.ServiceIfNull().GetAttachedValueProvider(source, metadata);
            if (attachedValueProvider == null)
                return false;

            return attachedValueProvider.Clear(source, BindPrefix + binding.Target.Path.Path);
        }

        #endregion

        #region Methods

        private static IBinding UpdateBinding(object item, IBinding addValue, IBinding currentValue, IBinding state1, IBinding state2)
        {
            currentValue?.Dispose();
            return addValue;
        }

        #endregion
    }
}