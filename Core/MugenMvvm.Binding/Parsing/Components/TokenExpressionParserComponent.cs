using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing.Components
{
    //todo use span/memory?
    public class TokenExpressionParserComponent : AttachableComponentBase<IExpressionParser>, IExpressionParserComponent, IExpressionParserComponentInternal<string>, IHasPriority,
        IComponentCollectionChangedListener<IComponent<IExpressionParser>>
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly TokenParserContext _parserContext;

        protected IParser[] Parsers;

        private static readonly MemberExpressionNode EmptyMember = new MemberExpressionNode(null, string.Empty);

        public static readonly HashSet<char> TargetDelimiters = new HashSet<char> {',', ';', ' '};
        public static readonly HashSet<char> Delimiters = new HashSet<char> {',', ';'};

        #endregion

        #region Constructors

        public TokenExpressionParserComponent(IMetadataContextProvider? metadataContextProvider = null) //todo review input parameter usage
        {
            _metadataContextProvider = metadataContextProvider;
            _parserContext = new TokenParserContext(this);
            Parsers = Default.EmptyArray<IParser>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener<IComponent<IExpressionParser>>.OnAdded(IComponentCollection<IComponent<IExpressionParser>> collection,
            IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentAdded(collection, component, metadata);
        }

        void IComponentCollectionChangedListener<IComponent<IExpressionParser>>.OnRemoved(IComponentCollection<IComponent<IExpressionParser>> collection,
            IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentRemoved(collection, component, metadata);
        }

        void IComponentCollectionChangedListener<IComponent<IExpressionParser>>.OnCleared(IComponentCollection<IComponent<IExpressionParser>> collection,
            ItemOrList<IComponent<IExpressionParser>?, IComponent<IExpressionParser>[]> oldItems, IReadOnlyMetadataContext? metadata)
        {
            OnComponentCleared(collection, oldItems, metadata);
        }

        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> IExpressionParserComponent.TryParse<TExpression>(in TExpression expression,
            IReadOnlyMetadataContext? metadata)
        {
            return TryParseInternal(expression, metadata);
        }

        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> IExpressionParserComponentInternal<string>.TryParse(in string expression,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(expression, nameof(expression));
            return ParseInternal(expression, metadata);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            Parsers = owner.Components.GetItems().OfType<IParser>().ToArray();
            owner.Components.Components.Add(this);
        }

        protected override void OnDetachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.Components.Remove(this);
            Parsers = Default.EmptyArray<IParser>();
        }

        protected virtual ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> TryParseInternal<TExpression>(in TExpression expression,
            IReadOnlyMetadataContext? metadata)
        {
            if (this is IExpressionParserComponentInternal<TExpression> parser)
                return parser.TryParse(expression, metadata);
            return default;
        }

        protected virtual ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> ParseInternal(in string expression, IReadOnlyMetadataContext? metadata)
        {
            _parserContext.Initialize(expression, metadata);
            return ParseInternal(_parserContext);
        }

        protected virtual void OnComponentAdded(IComponentCollection<IComponent<IExpressionParser>> collection,
            IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref Parsers, Owner, collection, component, metadata);
        }

        protected virtual void OnComponentRemoved(IComponentCollection<IComponent<IExpressionParser>> collection,
            IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref Parsers, collection, component, metadata);
        }

        protected virtual void OnComponentCleared(IComponentCollection<IComponent<IExpressionParser>> collection,
            ItemOrList<IComponent<IExpressionParser>?, IComponent<IExpressionParser>[]> oldItems, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnCleared(ref Parsers, collection, oldItems, metadata);
        }

        protected ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> ParseInternal(IContext context)
        {
            ExpressionParserResult itemResult = default;
            List<ExpressionParserResult>? result = null;
            while (!context.IsEof())
            {
                var r = TryParseNext(context);
                if (r.IsEmpty)
                    break;
                if (itemResult.IsEmpty)
                    itemResult = r;
                else
                {
                    if (result == null)
                        result = new List<ExpressionParserResult> {itemResult};
                    result.Add(r);
                }
            }

            if (result == null)
                return itemResult;
            return result;
        }

        private ExpressionParserResult TryParseNext(IContext context)
        {
            var delimiterPos = context.SkipWhitespaces().FindAnyOf(TargetDelimiters);
            var length = context.Length;
            if (delimiterPos > 0)
                context.SetLimit(delimiterPos);

            var target = context.ParseWhileAnyOf(Delimiters);
            context.SetLimit(length);

            IExpressionNode? source = null;
            if (context.IsToken(' '))
                source = context.ParseWhileAnyOf(Delimiters);

            List<IExpressionNode>? parameters = null;
            IExpressionNode? parameter = null;
            while (context.IsToken(','))
            {
                var param = context.MoveNext().ParseWhileAnyOf(Delimiters);
                if (parameter == null)
                    parameter = param;
                else
                {
                    if (parameters == null)
                        parameters = new List<IExpressionNode> {parameter};
                    parameters.Add(param);
                }
            }

            if (context.IsEof() || context.IsToken(';'))
            {
                if (context.IsToken(';'))
                    context.MoveNext();
                return new ExpressionParserResult(target, source ?? EmptyMember, parameters ?? new ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>>(parameter),
                    context);
            }

            BindingExceptionManager.ThrowCannotParseExpression(this);
            return default;
        }

        #endregion

        #region Nested types

        public interface IContext : IMetadataOwner<IMetadataContext>
        {
            int Position { get; }

            int Length { get; }

            char TokenAt(int position);

            string GetValue(int start, int end);

            void SetPosition(int position);

            void SetLimit(int? limit);

            IExpressionNode? TryParse(IExpressionNode? expression = null);
        }

        public interface IParser : IComponent<IExpressionParser>
        {
            IExpressionNode? TryParse(IContext context, IExpressionNode? expression);
        }

        private sealed class TokenParserContext : IContext
        {
            #region Fields

            private readonly TokenExpressionParserComponent _parser;
            private IReadOnlyMetadataContext? _metadata;
            private string _source;

            #endregion

            #region Constructors

            public TokenParserContext(TokenExpressionParserComponent parser)
            {
                _source = string.Empty;
                _parser = parser;
            }

            #endregion

            #region Properties

            public bool HasMetadata => _metadata != null && _metadata.Count != 0;

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata is IMetadataContext ctx)
                        return ctx;

                    Interlocked.CompareExchange(ref _metadata, _metadata.ToNonReadonly(this, _parser._metadataContextProvider), null);
                    return (IMetadataContext) _metadata!;
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

            public IExpressionNode? TryParse(IExpressionNode? expression = null)
            {
                var components = _parser.Parsers;
                for (var i = 0; i < components.Length; i++)
                {
                    var result = components[i].TryParse(this, expression);
                    if (result != null)
                        return result;
                }

                return null;
            }

            public void SetLimit(int? limit)
            {
                Length = limit.GetValueOrDefault(_source.Length);
            }

            #endregion

            #region Methods

            public void Initialize(string source, IReadOnlyMetadataContext? metadata)
            {
                _source = source;
                _metadata = metadata;
                Length = source.Length;
            }

            public override string ToString()
            {
                return
                    $"Position '{Position.ToString()}' CurrentToken '{GetToken(Position)}' PrevToken '{GetToken(Position - 1)}' NextToken '{GetToken(Position + 1)}' Source '{_source}'";
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