using System;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing
{
    public sealed class TokenParserContext : ITokenParserContext
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private int? _limit;

        private IMetadataContext? _metadata;
        private int _position;

        #endregion

        #region Constructors

        public TokenParserContext(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
            Source = string.Empty;
        }

        #endregion

        #region Properties

        public string Source { get; private set; }

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

        public int Position
        {
            get => _position;
            set
            {
                Should.BeValid(nameof(value), value <= Length);
                _position = value;
            }
        }

        public int? Limit
        {
            get => _limit;
            set
            {
                Should.BeValid(nameof(value), value == null || value.Value <= Source.Length);
                _limit = value;
            }
        }

        public int Length => _limit.GetValueOrDefault(Source.Length);

        public IComponentOwner? Owner { get; set; }

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

        public IExpressionNode? TryParse(IExpressionNode? expression = null, Func<ITokenParserContext, ITokenParserComponent, bool>? condition = null)
        {
            return Owner?.Components.Get<ITokenParserComponent>(_metadata).TryParse(this, expression, condition);
        }

        #endregion

        #region Methods

        public void Initialize(string source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNullOrEmpty(source, nameof(source));
            Source = source;
            Position = 0;
            Limit = null;
            _metadata?.Clear();
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