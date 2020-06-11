using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components
{
    public class ExceptionBindingExpressionBuilderDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryBuildBindingExpressionShouldWrapExceptionToInvalidBinding()
        {
            var request = "";
            var exception = new Exception();
            var decorator = new ExceptionBindingExpressionBuilderDecorator();
            var component = new TestBindingExpressionBuilderComponent
            {
                TryBuildBindingExpression = (o, type, arg3) =>
                {
                    o.ShouldEqual(request);
                    type.ShouldEqual(request.GetType());
                    arg3.ShouldEqual(DefaultMetadata);
                    throw exception;
                }
            };
            ((IComponentCollectionDecorator<IBindingExpressionBuilderComponent>) decorator).Decorate(new List<IBindingExpressionBuilderComponent> {decorator, component}, DefaultMetadata);

            var expression = decorator.TryBuildBindingExpression(request, DefaultMetadata).Item!;
            expression.ShouldNotBeNull();

            var binding = (InvalidBinding) expression.Build(this, this, DefaultMetadata);
            binding.Exception.ShouldEqual(exception);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryBuildBindingExpressionShouldWrapBuildExceptionToInvalidBinding(int count)
        {
            var target = new object();
            var source = new object();
            var exception = new Exception();
            var expressions = new IBindingExpression[count];
            for (var i = 0; i < expressions.Length; i++)
            {
                expressions[i] = new TestBindingExpression
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

            var decorator = new ExceptionBindingExpressionBuilderDecorator();
            var component = new TestBindingExpressionBuilderComponent
            {
                TryBuildBindingExpression = (o, type, arg3) => expressions
            };
            ((IComponentCollectionDecorator<IBindingExpressionBuilderComponent>) decorator).Decorate(new List<IBindingExpressionBuilderComponent> {decorator, component}, DefaultMetadata);

            var result = decorator.TryBuildBindingExpression("", DefaultMetadata).AsList();
            result.Count.ShouldEqual(count);

            for (var i = 0; i < result.Count; i++)
            {
                var binding = (InvalidBinding) result[i].Build(target, source, DefaultMetadata);
                binding.Exception.ShouldEqual(exception);
            }
        }

        #endregion
    }
}