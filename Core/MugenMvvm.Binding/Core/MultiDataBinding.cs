using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core
{
    public sealed class MultiDataBinding : DataBindingBase, IDynamicDataBindingValue
    {
        #region Fields

        private Func<object?[]?, IReadOnlyMetadataContext?, object?>? _expression;

        #endregion

        #region Constructors

        public MultiDataBinding(IBindingPathObserver target, IBindingPathObserver[] sources, Func<object?[]?, IReadOnlyMetadataContext?, object?> expression)
            : base(target, sources)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expression = expression;
        }

        #endregion

        #region Implementation of interfaces

        public object? GetValue()
        {
            var sources = (IBindingPathObserver[])SourceRaw;
            var values = new object?[sources.Length];
            for (var i = 0; i < sources.Length; i++)
            {
                var members = sources[i].GetLastMember(Metadata);
                var value = members.GetLastMemberValue(Metadata);
                if (value.IsUnsetValueOrDoNothing())
                    return value;
                values[i] = value;
            }

            return _expression!(values, Metadata);
        }

        #endregion

        #region Methods

        protected override void OnDispose()
        {
            _expression = null;
        }

        protected override object? GetSourceValue(IBindingMemberInfo lastMember)
        {
            if (BindingMemberType.Event.Equals(lastMember.MemberType))
                return this;
            return GetValue();
        }

        #endregion
    }
}