using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public abstract class ExpressionBuilderTestBase<T> : UnitTestBase where T : class, IExpressionBuilderComponent, new()
    {
        protected readonly T Builder;
        protected readonly TestExpressionBuilderContext Context;

        protected ExpressionBuilderTestBase(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Builder = new T();
            Context = new TestExpressionBuilderContext();
        }
    }
}