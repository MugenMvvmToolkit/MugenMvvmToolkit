using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Bindings.Resources
{
    public sealed class DynamicResource : IValueHolder<IWeakReference>, IValueHolder<IDictionary<string, object?>>, IDynamicResource
    {
        private object? _value;

        [Preserve] public event EventHandler? ValueChanged;

        [Preserve]
        public object? Value
        {
            get => _value;
            set
            {
                if (_value == value)
                    return;
                _value = value;
                RaiseValueChanged();
            }
        }

        IDictionary<string, object?>? IValueHolder<IDictionary<string, object?>>.Value { get; set; }

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        public void RaiseValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}