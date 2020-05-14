using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Binding.Core
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

        protected override int GetMetadataCount()
        {
            return 2;
        }

        protected override IEnumerator<MetadataContextValue> GetMetadataEnumerator()
        {
            return ((IEnumerable<MetadataContextValue>) new[] {MetadataContextValue.Create(BindingMetadata.Binding, this), MetadataContextValue.Create(BindingMetadata.IsMultiBinding, true)}).GetEnumerator();
        }

        protected override bool ContainsMetadata(IMetadataContextKey contextKey)
        {
            return base.ContainsMetadata(contextKey) || BindingMetadata.IsMultiBinding.Equals(contextKey);
        }

        protected override bool TryGetMetadata<T>(IReadOnlyMetadataContextKey<T> contextKey, out T value, T defaultValue)
        {
            if (BindingMetadata.IsMultiBinding.Equals(contextKey))
            {
                value = MugenExtensions.CastGeneric<bool, T>(true);
                return true;
            }

            return base.TryGetMetadata(contextKey, out value, defaultValue);
        }

        protected override void OnDispose()
        {
            _expression?.Dispose();
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