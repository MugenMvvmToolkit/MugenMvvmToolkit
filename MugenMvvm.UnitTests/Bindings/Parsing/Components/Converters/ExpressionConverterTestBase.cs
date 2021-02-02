using System.Linq.Expressions;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public abstract class ExpressionConverterTestBase<T> : UnitTestBase where T : class, IExpressionConverterComponent<Expression>, new()
    {
        protected readonly T Converter;
        protected readonly ExpressionConverterContext<Expression> Context;

        protected ExpressionConverterTestBase(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Converter = new T();
            Context = new ExpressionConverterContext<Expression>();
        }
    }
}