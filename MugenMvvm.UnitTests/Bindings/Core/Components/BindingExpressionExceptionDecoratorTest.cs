using System;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingExpressionExceptionDecoratorTest : UnitTestBase
    {
        private readonly BindingManager _bindingManager;

        public BindingExpressionExceptionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _bindingManager = new BindingManager(ComponentCollectionManager);
            _bindingManager.AddComponent(new BindingExpressionExceptionDecorator());
        }

        [Fact]
        public void TryParseBindingExpressionShouldWrapExceptionToInvalidBinding()
        {
            var request = "";
            var exception = new Exception();
            _bindingManager.AddComponent(new TestBindingExpressionParserComponent(_bindingManager)
            {
                TryParseBindingExpression = (o, arg3) =>
                {
                    o.ShouldEqual(request);
                    arg3.ShouldEqual(DefaultMetadata);
                    throw exception;
                }
            });

            var expression = _bindingManager.TryParseBindingExpression(request, DefaultMetadata).Item!;
            expression.ShouldNotBeNull();

            var binding = (InvalidBinding) expression.Build(this, this, DefaultMetadata);
            binding.Exception.ShouldEqual(exception);
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
                        arg3.ShouldEqual(DefaultMetadata);
                        throw exception;
                    }
                };
            }

            _bindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (o, arg3) => expressions
            });

            var result = _bindingManager.TryParseBindingExpression("", DefaultMetadata).AsList();
            result.Count.ShouldEqual(count);

            for (var i = 0; i < result.Count; i++)
            {
                var binding = (InvalidBinding) result[i].Build(target, source, DefaultMetadata);
                binding.Exception.ShouldEqual(exception);
            }
        }
    }
}