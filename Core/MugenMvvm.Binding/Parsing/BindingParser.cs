using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing
{
    public sealed class BindingParser : ComponentOwnerBase<IBindingParser>, IBindingParser, IComponentOwnerAddedCallback<IComponent<IBindingParser>>,
        IComponentOwnerRemovedCallback<IComponent<IBindingParser>>
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private IBindingParserContextProviderComponent[] _contextProviders;
        private IExpressionParserComponent[] _parsers;

        public static readonly HashSet<char> TargetDelimiters = new HashSet<char> {',', ';', ' '};
        public static readonly HashSet<char> Delimiters = new HashSet<char> {',', ';'};

        #endregion

        #region Constructors

        public BindingParser(IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _metadataContextProvider = metadataContextProvider;
            _parsers = Default.EmptyArray<IExpressionParserComponent>();
            _contextProviders = Default.EmptyArray<IBindingParserContextProviderComponent>();
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<BindingParserResult> Parse(string expression, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(expression, nameof(expression));
            var result = new List<BindingParserResult>();
            var context = GetBindingParserContext(expression, metadata);
            while (!context.IsEof(context.Position))
                result.Add(Parse(context));

            return result;
        }

        void IComponentOwnerAddedCallback<IComponent<IBindingParser>>.OnComponentAdded(IComponentCollection<IComponent<IBindingParser>> collection,
            IComponent<IBindingParser> component, IReadOnlyMetadataContext metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _contextProviders, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref _parsers, this, collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IBindingParser>>.OnComponentRemoved(IComponentCollection<IComponent<IBindingParser>> collection,
            IComponent<IBindingParser> component, IReadOnlyMetadataContext metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _contextProviders, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref _parsers, collection, component, metadata);
        }

        #endregion

        #region Methods

        private static BindingParserResult Parse(IBindingParserContext context)
        {
            context.SkipWhitespacesSetPosition();
            var delimiterPos = context.FindAnyOf(TargetDelimiters);
            var length = context.Length;
            if (delimiterPos > 0)
                context.SetLimit(delimiterPos);

            var target = context.ParseWhileAnyOf(Delimiters);
            context.SetLimit(length);

            IExpressionNode? source = null;
            if (context.IsToken(' ', context.Position))
                source = context.ParseWhileAnyOf(Delimiters);

            List<IExpressionNode>? parameters = null;
            while (context.IsToken(',', context.Position))
            {
                if (parameters == null)
                    parameters = new List<IExpressionNode>();
                context.MoveNext();
                parameters.Add(context.ParseWhileAnyOf(Delimiters));
            }

            if (context.IsEof(context.Position) || context.IsToken(';', context.Position))
            {
                if (context.IsToken(';', context.Position))
                    context.MoveNext();
                return new BindingParserResult(target, source, parameters, context);
            }

            throw new Exception(); //todo add 
        }

        private IBindingParserContext GetBindingParserContext(string expression, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < _contextProviders.Length; i++)
            {
                var context = _contextProviders[i].TryGetBindingParserContext(expression, metadata);
                if (context != null)
                    return context;
            }

            return new BindingParserContext(expression, this, metadata);
        }

        #endregion

        #region Nested types

        private sealed class BindingParserContext : IBindingParserContext
        {
            #region Fields

            private readonly BindingParser _parser;
            private readonly string _source;
            private IReadOnlyMetadataContext? _metadata;

            #endregion

            #region Constructors

            public BindingParserContext(string source, BindingParser parser, IReadOnlyMetadataContext? metadata)
            {
                _source = source;
                _parser = parser;
                _metadata = metadata;
                Length = source.Length;
            }

            #endregion

            #region Properties

            public bool HasMetadata => _metadata != null;

            public IMetadataContext Metadata
            {
                get
                {
                    if (!(_metadata is IMetadataContext ctx))
                    {
                        ctx = _metadata.ToNonReadonly(this, _parser._metadataContextProvider);
                        _metadata = ctx;
                    }

                    return ctx;
                }
            }

            public int Position { get; private set; }

            public int Length { get; private set; }

            #endregion

            #region Implementation of interfaces

            public char TokenAt(int position)
            {
                return _source[position];
            }

            public string GetValue(int start, int end)
            {
                return _source.Substring(start, end - start);
            }

            public void SetPosition(int position)
            {
                Position = position;
            }

            public void SetLimit(int? limit)
            {
                Length = limit.GetValueOrDefault(_source.Length);
            }

            public IExpressionNode? TryParse(IExpressionNode? expression = null, IReadOnlyMetadataContext? metadata = null)
            {
                var components = _parser._parsers;
                for (var i = 0; i < components.Length; i++)
                {
                    var result = components[i].TryParse(this, expression, metadata);
                    if (result != null)
                        return result;
                }

                return null;
            }

            #endregion

            #region Methods

            public override string ToString()
            {
                return $"Position '{Position}' CurrentToken '{GetToken(Position)}' PrevToken '{GetToken(Position - 1)}' NextToken '{GetToken(Position + 1)}'";
            }

            private string GetToken(int position)
            {
                if (this.IsEof(position))
                    return "EOF";
                if (position < 0)
                    return "BOF";
                return _source[position].ToString();
            }

            #endregion
        }

        #endregion
    }
}