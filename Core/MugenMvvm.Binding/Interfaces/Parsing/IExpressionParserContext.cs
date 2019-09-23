﻿using MugenMvvm.Binding.Parsing;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IExpressionParserContext : IMetadataOwner<IMetadataContext>
    {
        ExpressionParserResult TryParseNext(IReadOnlyMetadataContext? metadata);
    }
}