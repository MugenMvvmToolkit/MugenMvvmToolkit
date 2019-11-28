using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingHolderComponent : IBindingHolderComponent, IHasPriority
    {
        #region Fields

        private readonly IAttachedValueManager? _attachedValueManager;

        private const string BindPrefix = "@#b";

        private static readonly UpdateValueDelegate<object, IBinding, IBinding, object?> UpdateBindingDelegate = UpdateBinding;

        #endregion

        #region Constructors

        public BindingHolderComponent(IAttachedValueManager? attachedValueManager = null)
        {
            _attachedValueManager = attachedValueManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.BindingHolder;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBinding, IReadOnlyList<IBinding>> TryGetBindings(object target, string? path, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(target, nameof(target));
            var attachedValueProvider = _attachedValueManager.DefaultIfNull().GetAttachedValueProvider(target, metadata);
            if (attachedValueProvider == null)
                return default;

            var values = path == null
                ? attachedValueProvider.GetValues(target, target, (o, pair, arg3) => pair.Key.StartsWith(BindPrefix, StringComparison.Ordinal))
                : attachedValueProvider.GetValues(target, path, (o, pair, arg3) => pair.Key.StartsWith(BindPrefix, StringComparison.Ordinal) && pair.Key.EndsWith(pair.Key, StringComparison.Ordinal));

            if (values.Count == 0)
                return default;
            if (values.Count == 1)
                return new ItemOrList<IBinding, IReadOnlyList<IBinding>>((IBinding)values[0].Value!);

            var bindings = new IBinding[values.Count];
            for (var i = 0; i < bindings.Length; i++)
                bindings[i] = (IBinding)values[i].Value!;
            return bindings;
        }

        public bool TryRegister(IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            var target = binding.Target.Target;
            if (target == null)
                return false;

            var attachedValueProvider = _attachedValueManager.DefaultIfNull().GetOrAddAttachedValueProvider(target, metadata);
            attachedValueProvider.AddOrUpdate(target, GetPath(binding.Target.Path), binding, null, UpdateBindingDelegate);
            return true;
        }

        public bool TryUnregister(IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            var target = binding.Target.Target;
            if (target == null)
                return false;

            var attachedValueProvider = _attachedValueManager.DefaultIfNull().GetAttachedValueProvider(target, metadata);
            return attachedValueProvider != null && attachedValueProvider.Clear(target, GetPath(binding.Target.Path));
        }

        #endregion

        #region Methods

        private static string GetPath(IMemberPath memberPath)
        {
            if (memberPath is IValueHolder<string> valueHolder)
            {
                if (valueHolder.Value == null)
                    valueHolder.Value = BindPrefix + memberPath.Path;
                return valueHolder.Value;
            }

            return BindPrefix + memberPath.Path;
        }

        private static IBinding UpdateBinding(object item, IBinding addValue, IBinding currentValue, object? _)
        {
            currentValue?.Dispose();
            return addValue;
        }

        #endregion
    }
}