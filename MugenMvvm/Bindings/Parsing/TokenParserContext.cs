using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Metadata;

namespace MugenMvvm.Bindings.Parsing
{
    public sealed class TokenParserContext : MetadataOwnerBase, ITokenParserContext, IHasTarget<string>
    {
        private int? _limit;
        private object? _parsers;
        private int _position;

        public TokenParserContext() : base(null)
        {
            Source = string.Empty;
        }

        public string Source { get; private set; }

        public ItemOrArray<ITokenParserComponent> Parsers
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ItemOrArray.FromRawValue<ITokenParserComponent>(_parsers);
            set => _parsers = value.GetRawValue();
        }

        public int Position
        {
            get => _position;
            set
            {
                Should.BeValid(value <= Length && value >= 0, nameof(value));
                _position = value;
            }
        }

        public int? Limit
        {
            get => _limit;
            set
            {
                Should.BeValid(value == null || value.Value <= Source.Length && value >= 0, nameof(value));
                _limit = value;
            }
        }

        public int Length
        {
            get
            {
                if (_limit == null)
                    return Source.Length;
                return _limit.Value;
            }
        }

        string IHasTarget<string>.Target => Source;

        public override string ToString() =>
            $"Position '{Position.ToString()}' CurrentToken '{GetToken(Position)}' PrevToken '{GetToken(Position - 1)}' NextToken '{GetToken(Position + 1)}' Source '{Source}'";

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

        public char TokenAt(int position) => Source[position];

        public string GetValue(int start, int end) => Source.Substring(start, end - start);

#if SPAN_API
        public ReadOnlySpan<char> GetValueSpan(int start, int end) => Source.AsSpan(start, end - start);
#endif

        public IExpressionNode? TryParse(IExpressionNode? expression = null, Func<ITokenParserContext, ITokenParserComponent, bool>? condition = null) =>
            Parsers.TryParse(this, expression, condition);

        private string GetToken(int position)
        {
            if (this.IsEof(position))
                return "EOF";
            if (position < 0)
                return "BOF";
            return Source[position].ToString();
        }
    }
}