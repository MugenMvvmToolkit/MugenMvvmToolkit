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
    public sealed class BindingParser : ComponentOwnerBase<IBindingParser>, IBindingParser
    {
        #region Fields

        public static readonly HashSet<char> TargetDelimiters = new HashSet<char> { ',', ';', ' ' };
        public static readonly HashSet<char> Delimiters = new HashSet<char> { ',', ';' };

        #endregion

        #region Constructors

        public BindingParser(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<BindingParserResult> Parse(string expression, IReadOnlyMetadataContext? metadata)
        {
            var result = new List<BindingParserResult>();
            var context = GetBindingParserContext(expression, metadata);
            while (!context.IsEof(context.Position))
                result.Add(Parse(context));

            return result;
        }

        #endregion

        #region Methods

        private static BindingParserResult Parse(IBindingParserContext context)
        {
            context.SetPosition(context.SkipWhitespaces());
            var delimiterPos = context.FindAnyOf(context.Position, TargetDelimiters);
            var length = context.Length;
            if (delimiterPos > 0)
                context.SetLimit(delimiterPos);

            var target = context.ParseWhileAnyOf(Delimiters);
            context.SetLimit(length);

            IExpressionNode? source = null;
            if (context.IsToken(' ', context.Position))
            {
                source = context.ParseWhileAnyOf(Delimiters);
            }

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
            return new BindingParserContext(expression, this);
        }

        #endregion

        #region Nested types

        private sealed class BindingParserContext : IBindingParserContext
        {
            #region Fields

            private readonly string _source;
            private readonly BindingParser _parser;

            #endregion

            #region Constructors

            public BindingParserContext(string source, BindingParser parser)
            {
                _source = source;
                _parser = parser;
                Metadata = MugenExtensions.GetMetadataContext(this);
                Length = source.Length;
            }

            #endregion

            #region Properties

            public bool HasMetadata => true;

            public IMetadataContext Metadata { get; }

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

            public IExpressionNode TryParse(IExpressionNode expression = null, IReadOnlyMetadataContext metadata = null)
            {
                var components = _parser.GetComponents();
                for (int i = 0; i < components.Length; i++)
                {
                    var result = (components[i] as IExpressionParserComponent)?.TryParse(this, expression, metadata);
                    if (result != null)
                        return result;
                }

                return null;
            }

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