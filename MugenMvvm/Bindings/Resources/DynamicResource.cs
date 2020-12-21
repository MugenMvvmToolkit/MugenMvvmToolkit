using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Bindings.Resources
{
    public sealed class DynamicResource : IValueHolder<IWeakReference>, IValueHolder<IDictionary<string, object?>>, IDynamicResource
    {
        #region Fields

        private object? _value;

        #endregion

        #region Properties

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

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        IDictionary<string, object?>? IValueHolder<IDictionary<string, object?>>.Value { get; set; }

        #endregion

        #region Events

        [Preserve]
        public event EventHandler? ValueChanged;

        #endregion

        #region Methods

        public void RaiseValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);

        #endregion
    }
}