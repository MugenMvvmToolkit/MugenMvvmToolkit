using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Internal
{
    public class TestExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly IExpressionParser? _parser;

        #endregion

        #region Constructors

        public TestExpressionParserComponent(IExpressionParser? parser = null)
        {
            _parser = parser;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<object, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<ExpressionParserResult>>? TryParse { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrIReadOnlyList<ExpressionParserResult> IExpressionParserComponent.TryParse(IExpressionParser parser, object expression, IReadOnlyMetadataContext? metadata)
        {
            _parser?.ShouldEqual(parser);
            return TryParse?.Invoke(expression, metadata) ?? default;
        }

        #endregion
    }
}