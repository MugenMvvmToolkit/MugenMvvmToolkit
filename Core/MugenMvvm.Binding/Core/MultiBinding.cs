using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core
{
    public sealed class MultiBinding : Binding, IExpressionValue
    {
        #region Fields

        private ICompiledExpression? _expression;

        #endregion

        #region Constructors

        internal MultiBinding(IMemberPathObserver target, object? source, ICompiledExpression expression)
           : base(target, source)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expression = expression;
        }

        public MultiBinding(IMemberPathObserver target, ItemOrList<object?, object?[]> sources, ICompiledExpression expression)
            : base(target, sources.GetRawValue())
        {
            Should.NotBeNull(expression, nameof(expression));
            _expression = expression;
        }

        #endregion

        #region Properties

        public ICompiledExpression Expression
        {
            get
            {
                if (State == BindingState.Disposed)
                    ExceptionManager.ThrowObjectDisposed(this);
                return _expression!;
            }
        }

        #endregion

        #region Implementation of interfaces

        public object? Invoke(IReadOnlyMetadataContext? metadata = null)
        {
            return _expression.Invoke(SourceRaw, metadata ?? this);
        }

        #endregion

        #region Methods

        protected override void OnDispose()
        {
            _expression = null;
        }

        protected override object? GetSourceValue(MemberPathLastMember targetMember)
        {
            if (MemberType.Event == targetMember.Member.MemberType)
                return this;
            return _expression.Invoke(SourceRaw, this);
        }

        #endregion
    }
}