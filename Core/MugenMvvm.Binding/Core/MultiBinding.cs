using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;

namespace MugenMvvm.Binding.Core
{
    public sealed class MultiBinding : Binding, IDynamicBindingValue
    {
        #region Fields

        private ICompiledExpression? _expression;

        #endregion

        #region Constructors

        public MultiBinding(IMemberPathObserver target, IMemberPathObserver[] sources, ICompiledExpression expression)
            : base(target, sources)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expression = expression;
        }

        #endregion

        #region Implementation of interfaces

        public object? GetValue()
        {
            var sources = (IMemberPathObserver[]) SourceRaw;
            var values = new object?[sources.Length];
            for (var i = 0; i < sources.Length; i++)
            {
                var members = sources[i].GetLastMember(Metadata);
                var value = members.GetLastMemberValue(Metadata);
                if (value.IsUnsetValueOrDoNothing())
                    return value;
                values[i] = value;
            }

            return _expression!.Invoke(values, Metadata);
        }

        #endregion

        #region Methods

        protected override void OnDispose()
        {
            _expression = null;
        }

        protected override object? GetSourceValue(in MemberPathLastMember targetMember)
        {
            if (BindingMemberType.Event.Equals(targetMember.LastMember.MemberType))
                return this;
            return GetValue();
        }

        #endregion
    }
}