using System;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Internal
{
    public class TestTokenParserComponent : ITokenParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<ITokenParserContext, IExpressionNode?, IExpressionNode?>? TryParse { get; set; }

        #endregion

        #region Implementation of interfaces

        IExpressionNode? ITokenParserComponent.TryParse(ITokenParserContext context, IExpressionNode? expression) => TryParse?.Invoke(context, expression);

        #endregion
    }
}