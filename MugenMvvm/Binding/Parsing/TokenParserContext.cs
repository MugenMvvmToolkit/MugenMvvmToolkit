using System;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Binding.Parsing
{
    public sealed class TokenParserContext : MetadataOwnerBase, ITokenParserContext, IHasTarget<string>
    {
        #region Fields

        private int? _limit;
        private ITokenParserComponent[] _parsers;
        private int _position;

        #endregion

        #region Constructors

        public TokenParserContext(IMetadataContextManager? metadataContextManager = null) : base(null, metadataContextManager)
        {
            Source = string.Empty;
            _parsers = Default.Array<ITokenParserComponent>();
        }

        #endregion

        #region Properties

        public string Source { get; private set; }

        public int Position
        {
            get => _position;
            set
            {
                Should.BeValid(nameof(value), value <= Length && value >= 0);
                _position = value;
            }
        }

        public int? Limit
        {
            get => _limit;
            set
            {
                Should.BeValid(nameof(value), value == null || value.Value <= Source.Length && value >= 0);
                _limit = value;
            }
        }

        public int Length => _limit.GetValueOrDefault(Source.Length);

        public ITokenParserComponent[] Parsers
        {
            get => _parsers;
            set
            {
                Should.NotBeNull(value, nameof(value));
                _parsers = value;
            }
        }

        string IHasTarget<string>.Target => Source;

        #endregion

        #region Implementation of interfaces

        public char TokenAt(int position)
        {
            return Source[position];
        }

        public string GetValue(int start, int end)
        {
            return Source.Substring(start, end - start);
        }

#if SPAN_API
        public ReadOnlySpan<char> GetValueSpan(int start, int end)
        {
            return Source.AsSpan(start, end - start);
        }
#endif

        public IExpressionNode? TryParse(IExpressionNode? expression = null, Func<ITokenParserContext, ITokenParserComponent, bool>? condition = null)
        {
            return Parsers.TryParse(this, expression, condition);
        }

        #endregion

        #region Methods

        public void Initialize(string source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNullOrEmpty(source, nameof(source));
            Source = source;
            Position = 0;
            Limit = null;
            MetadataRaw?.Clear();
            if (!metadata.IsNullOrEmpty())
                Metadata.Merge(metadata!);
        }

        public override string ToString()
        {
            return $"Position '{Position.ToString()}' CurrentToken '{GetToken(Position)}' PrevToken '{GetToken(Position - 1)}' NextToken '{GetToken(Position + 1)}' Source '{Source}'";
        }

        private string GetToken(int position)
        {
            if (this.IsEof(position))
                return "EOF";
            if (position < 0)
                return "BOF";
            return Source[position].ToString();
        }

        #endregion
    }
}