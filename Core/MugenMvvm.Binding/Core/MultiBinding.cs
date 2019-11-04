using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core
{
    public sealed class MultiBinding : Binding, IDynamicBindingValue
    {
        #region Fields

        private ICompiledExpression? _expression;

        #endregion

        #region Constructors

        public MultiBinding(IMemberPathObserver target, ItemOrList<IMemberPathObserver?, IMemberPathObserver[]> sources, ICompiledExpression expression)
            : base(target, sources.Item ?? (object?)sources.List)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expression = expression;
        }

        #endregion

        #region Implementation of interfaces

        public object? GetValue()
        {
            ItemOrList<ExpressionValue, ExpressionValue[]> values;
            if (SourceRaw is IMemberPathObserver[] sources)
            {
                var expressionValues = new ExpressionValue[sources.Length];
                for (var i = 0; i < sources.Length; i++)
                {
                    var members = sources[i].GetLastMember(this);
                    var value = members.GetLastMemberValue(this);
                    if (value.IsUnsetValueOrDoNothing())
                        return value;
                    expressionValues[i] = new ExpressionValue(value?.GetType() ?? members.LastMember.Type, null);
                }

                values = expressionValues;
            }
            else
            {
                var members = ((IMemberPathObserver)SourceRaw).GetLastMember(this);
                var value = members.GetLastMemberValue(this);
                if (value.IsUnsetValueOrDoNothing())
                    return value;

                values = new ExpressionValue(value?.GetType() ?? members.LastMember.Type, value);
            }

            return _expression!.Invoke(values, this);
        }

        #endregion

        #region Methods

        protected override void OnDispose()
        {
            _expression = null;
        }

        protected override object? GetSourceValue(MemberPathLastMember targetMember)
        {
            if (BindingMemberType.Event == targetMember.LastMember.MemberType)
                return this;
            return GetValue();
        }

        #endregion
    }
}