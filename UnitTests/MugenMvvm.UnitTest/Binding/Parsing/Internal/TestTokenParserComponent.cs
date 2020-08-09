using System;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Parsing.Internal
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