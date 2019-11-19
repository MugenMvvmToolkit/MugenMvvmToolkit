using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Binding.Core
{
    public sealed class MultiBinding : Binding, IExpressionValue
    {
        #region Fields

        private ICompiledExpression? _expression;

        #endregion

        #region Constructors

        public MultiBinding(IMemberPathObserver target, ItemOrList<IMemberPathObserver?, IMemberPathObserver[]> sources, ICompiledExpression expression)
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

        protected override IEnumerator<MetadataContextValue> GetMetadataEnumerator()
        {
            IEnumerable<MetadataContextValue> v = new[]
            {
                MetadataContextValue.Create(BindingMetadata.Binding, this),
                MetadataContextValue.Create(BindingMetadata.IsMultiBinding, true)
            };
            return v.GetEnumerator();
        }

        protected override int GetMetadataCount()
        {
            return base.GetMetadataCount() + 1;
        }

        protected override bool ContainsMetadata(IMetadataContextKey contextKey)
        {
            return BindingMetadata.IsMultiBinding.Equals(contextKey) || base.ContainsMetadata(contextKey);
        }

        protected override bool TryGetMetadata<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue)
        {
            if (BindingMetadata.IsMultiBinding.Equals(contextKey))
            {
                value = MugenExtensions.ConvertGenericValue<bool, T>(true);
                return true;
            }

            return base.TryGetMetadata(contextKey, out value, defaultValue);
        }

        #endregion
    }
}