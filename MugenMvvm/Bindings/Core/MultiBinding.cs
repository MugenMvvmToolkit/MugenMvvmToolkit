using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Core
{
    public sealed class MultiBinding : Binding, IValueExpression
    {
        private ICompiledExpression? _expression;

        public MultiBinding(IMemberPathObserver target, ItemOrArray<object?> sources, ICompiledExpression expression)
            : base(target, sources.GetRawValue())
        {
            Should.NotBeNull(expression, nameof(expression));
            _expression = expression;
            if (!sources.HasItem)
                ClearFlag(HasItem);
        }

        internal MultiBinding(IMemberPathObserver target, object? source, ICompiledExpression expression)
            : base(target, source)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expression = expression;
        }

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

        public object? Invoke(IReadOnlyMetadataContext? metadata = null) => _expression.Invoke(SourceRaw, metadata ?? this);

        protected override int GetMetadataCount() => 2;

        protected override ItemOrIEnumerable<KeyValuePair<IMetadataContextKey, object?>> GetMetadataValues() =>
            new[] {BindingMetadata.Binding.ToValue(this), BindingMetadata.IsMultiBinding.ToValue(true)};

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
    }
}