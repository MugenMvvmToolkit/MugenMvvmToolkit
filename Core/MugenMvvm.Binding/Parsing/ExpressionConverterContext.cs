﻿using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing
{
    public sealed class ExpressionConverterContext<TExpression> : IExpressionConverterContext<TExpression> where TExpression : class
    {
        #region Fields

        private readonly ExpressionDictionary _expressionsDict;

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private IExpressionConverterComponent<TExpression>[] _converters;
        private IMetadataContext? _metadata;

        #endregion

        #region Constructors

        public ExpressionConverterContext(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
            _converters = Default.EmptyArray<IExpressionConverterComponent<TExpression>>();
            _expressionsDict = new ExpressionDictionary();
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata
        {
            get
            {
                if (_metadata == null)
                    _metadataContextProvider.LazyInitialize(ref _metadata, this);
                return _metadata;
            }
        }

        public IExpressionConverterComponent<TExpression>[] Converters
        {
            get => _converters;
            set
            {
                Should.NotBeNull(value, nameof(value));
                _converters = value;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryGetExpression(TExpression expression)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expressionsDict.TryGetValue(expression, out var value);
            return value;
        }

        public void SetExpression(TExpression expression, IExpressionNode value)
        {
            Should.NotBeNull(expression, nameof(expression));
            Should.NotBeNull(value, nameof(value));
            _expressionsDict[expression] = value;
        }

        public void ClearExpression(TExpression expression)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expressionsDict.Remove(expression);
        }

        public IExpressionNode Convert(TExpression expression)
        {
            Should.NotBeNull(expression, nameof(expression));
            var components = _converters;
            IExpressionNode? exp;
            foreach (var component in components)
            {
                exp = component.TryConvert(this, expression);
                if (exp != null)
                    return exp;
            }

            exp = TryGetExpression(expression);
            if (exp != null)
                return exp;

            this.ThrowCannotParse(expression);
            return null;
        }

        #endregion

        #region Methods

        public void Initialize(IReadOnlyMetadataContext? metadata)
        {
            _expressionsDict.Clear();
            _metadata?.Clear();
            if (!metadata.IsNullOrEmpty())
                Metadata.Merge(metadata!);
        }

        #endregion

        #region Nested types

        private sealed class ExpressionDictionary : LightDictionary<TExpression, IExpressionNode?>
        {
            #region Constructors

            public ExpressionDictionary() : base(3)
            {
            }

            #endregion

            #region Methods

            protected override int GetHashCode(TExpression key)
            {
                return key.GetHashCode();
            }

            protected override bool Equals(TExpression x, TExpression y)
            {
                return x.Equals(y);
            }

            #endregion
        }

        #endregion
    }
}