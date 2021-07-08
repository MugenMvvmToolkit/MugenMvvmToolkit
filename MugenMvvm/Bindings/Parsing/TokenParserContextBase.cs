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
    public abstract class TokenParserContextBase<T> : MetadataOwnerBase, ITokenParserContext, IHasTarget<T> where T : class
    {
        private int? _limit;
        private object? _parsers;
        private int _position;
        private int _length;

        protected TokenParserContextBase(IReadOnlyMetadataContext? metadata) : base(metadata)
        {
            Source = null!;
        }

        public T Source { get; private set; }

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
                Should.BeValid(value == null || value.Value <= _length && value >= 0, nameof(value));
                _limit = value;
            }
        }

        public int Length => _limit ?? _length;

        T IHasTarget<T>.Target => Source;

        public abstract char TokenAt(int position);

        public abstract string GetValue(int start, int end);

#if SPAN_API
        public abstract ReadOnlySpan<char> GetValueSpan(int start, int end);
#endif

        public override string ToString() =>
            $"Position '{Position}' CurrentToken '{GetToken(Position)}' PrevToken '{GetToken(Position - 1)}' NextToken '{GetToken(Position + 1)}' Source '{Source}'";

        public void Initialize(T source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(source, nameof(source));
            Source = source;
            Position = 0;
            Limit = null;
            _length = GetLength();
            MetadataRaw?.Clear();
            if (!metadata.IsNullOrEmpty())
                Metadata.Merge(metadata!);
        }

        public IExpressionNode? TryParse(IExpressionNode? expression = null, Func<ITokenParserContext, ITokenParserComponent, bool>? condition = null) =>
            Parsers.TryParse(this, expression, condition);

        protected abstract int GetLength();

        private string GetToken(int position)
        {
            if (this.IsEof(position))
                return "EOF";
            if (position < 0)
                return "BOF";
            return TokenAt(position).ToString();
        }
    }
}