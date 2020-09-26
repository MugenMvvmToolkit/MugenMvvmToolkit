using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingHolder : IBindingHolderComponent, IHasPriority
    {
        #region Fields

        private readonly IAttachedValueManager? _attachedValueManager;
        private static readonly Func<object, string, IBinding?, IBinding, IBinding> UpdateBindingDelegate = UpdateBinding;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BindingHolder(IAttachedValueManager? attachedValueManager = null)
        {
            _attachedValueManager = attachedValueManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.BindingHolder;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBinding, IReadOnlyList<IBinding>> TryGetBindings(IBindingManager bindingManager, object target, string? path, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(target, nameof(target));
            var values = path == null
                ? target.AttachedValues(metadata, _attachedValueManager).GetValues<object?>(null, (_, key, __, ___) => key.StartsWith(BindingInternalConstant.BindPrefix, StringComparison.Ordinal))
                : target.AttachedValues(metadata, _attachedValueManager).GetValues(path,
                    (_, key, __, state) => key.StartsWith(BindingInternalConstant.BindPrefix, StringComparison.Ordinal) && key.EndsWith(state, StringComparison.Ordinal));

            var iterator = values.Iterator(pair => pair.Key == null);
            if (iterator.Count == 0)
                return default;
            if (iterator.Count == 1)
                return ItemOrList.FromItem((IBinding) values.Item.Value!);

            var bindings = new IBinding[iterator.Count];
            for (var i = 0; i < bindings.Length; i++)
                bindings[i] = (IBinding) iterator[i].Value!;
            return ItemOrList.FromListToReadOnly(bindings);
        }

        public bool TryRegister(IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            if (target == null)
                return false;

            target.AttachedValues(metadata, _attachedValueManager).AddOrUpdate(GetPath(binding.Target.Path), binding, binding, UpdateBindingDelegate);
            return true;
        }

        public bool TryUnregister(IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            if (target == null)
                return false;
            return target.AttachedValues(metadata, _attachedValueManager).Remove(GetPath(binding.Target.Path), out _);
        }

        #endregion

        #region Methods

        private static string GetPath(IMemberPath memberPath)
        {
            if (memberPath is IValueHolder<string> valueHolder)
            {
                if (valueHolder.Value == null)
                    valueHolder.Value = BindingInternalConstant.BindPrefix + memberPath.Path;
                return valueHolder.Value;
            }

            return BindingInternalConstant.BindPrefix + memberPath.Path;
        }

        private static IBinding UpdateBinding(object item, string path, IBinding? currentValue, IBinding addValue)
        {
            currentValue?.Dispose();
            return addValue;
        }

        #endregion
    }
}