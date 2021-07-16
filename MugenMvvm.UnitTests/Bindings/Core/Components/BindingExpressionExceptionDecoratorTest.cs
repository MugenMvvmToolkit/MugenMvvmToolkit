using System;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Core;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingExpressionExceptionDecoratorTest : UnitTestBase
    {
        public BindingExpressionExceptionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            BindingManager.AddComponent(new BindingExpressionExceptionDecorator());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryParseBindingExpressionShouldWrapBuildExceptionToInvalidBinding(int count)
        {
            var target = new object();
            var source = new object();
            var exception = new Exception();
            var expressions = new IBindingBuilder[count];
            for (var i = 0; i < expressions.Length; i++)
            {
                expressions[i] = new TestBindingBuilder
                {
                    Build = (t, s, arg3) =>
                    {
                        t.ShouldEqual(target);
                        source.ShouldEqual(s);
                        arg3.ShouldEqual(Metadata);
                        throw exception;
                    }
                };
            }

            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, _, _) => expressions
            });

            var result = BindingManager.TryParseBindingExpression("", Metadata);
            result.Count.ShouldEqual(count);

            foreach (var t in result)
            {
                var binding = (InvalidBinding)t.Build(target, source, Metadata);
                binding.Exception.ShouldEqual(exception);
            }
        }

        [Fact]
        public void TryParseBindingExpressionShouldWrapExceptionToInvalidBinding()
        {
            var request = "";
            var exception = new Exception();
            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (m, o, arg3) =>
                {
                    m.ShouldEqual(BindingManager);
                    o.ShouldEqual(request);
                    arg3.ShouldEqual(Metadata);
                    throw exception;
                }
            });

            var expression = BindingManager.TryParseBindingExpression(request, Metadata).Item!;
            expression.ShouldNotBeNull();

            var binding = (InvalidBinding)expression.Build(this, this, Metadata);
            binding.Exception.ShouldEqual(exception);
        }

        protected override IBindingManager GetBindingManager() => new BindingManager(ComponentCollectionManager);
    }
}