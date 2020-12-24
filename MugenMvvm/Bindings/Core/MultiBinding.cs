using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Core
{
    public sealed class MultiBinding : Binding, IValueExpression
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
            if (!sources.HasItem)
                ClearFlag(HasItem);
        }

        #endregion

        #region Properties

        public ICompiledExpression Expression
        {
            get
            {
                var expression = _expression;
                if (expression == null)
                    ExceptionManager.ThrowObjectDisposed(this);
                return expression;
            }
        }

        #endregion

        #region Implementation of interfaces

        public object? Invoke(IReadOnlyMetadataContext? metadata = null) => _expression.Invoke(SourceRaw, metadata ?? this);

        #endregion

        #region Methods

        protected override int GetMetadataCount() => 2;

        protected override ItemOrList<KeyValuePair<IMetadataContextKey, object?>, IEnumerable<KeyValuePair<IMetadataContextKey, object?>>> GetMetadataValues()
            => new[] {BindingMetadata.Binding.ToValue(this), BindingMetadata.IsMultiBinding.ToValue(true)};

        protected override bool ContainsMetadata(IMetadataContextKey contextKey) => base.ContainsMetadata(contextKey) || BindingMetadata.IsMultiBinding.Equals(contextKey);

        protected override bool TryGetMetadata(IMetadataContextKey contextKey, out object? value)
        {
            if (BindingMetadata.IsMultiBinding.Equals(contextKey))
            {
                value = BoxingExtensions.TrueObject;
                return true;
            }

            return base.TryGetMetadata(contextKey, out value);
        }

        protected override void OnDispose() => _expression = null;

        protected override object? GetSourceValue(MemberPathLastMember targetMember)
        {
            if (MemberType.Event == targetMember.Member.MemberType)
                return this;
            return _expression.Invoke(SourceRaw, this);
        }

        #endregion
    }
}