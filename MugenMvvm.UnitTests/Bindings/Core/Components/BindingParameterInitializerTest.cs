using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Visitors;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingParameterInitializerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void InitializeShouldIgnoreEmptyParameters()
        {
            var initializer = new BindingParameterInitializer();
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);
            initializer.Initialize(null!, context);
            context.BindingComponents.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InitializeShouldAddParameterHandlerComponent(bool ignore)
        {
            var context = new BindingExpressionInitializerContext(this);
            var target = new object();
            var src = new object();
            var converter = new object();
            var converterParameter = new TestMemberPathObserver();
            var fallback = new TestMemberPathObserver();
            var nullValue = new object();

            var parameterVisitCount = 0;
            var parameters = new[]
            {
                new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.Converter), ConstantExpressionNode.Get(converter)),
                new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.ConverterParameter), new TestBindingMemberExpressionNode
                {
                    GetBindingSource = (t, s, m) =>
                    {
                        t.ShouldEqual(target);
                        src.ShouldEqual(s);
                        m.ShouldEqual(context.GetMetadataOrDefault());
                        return converterParameter;
                    },
                    VisitHandler = (visitor, metadataContext) =>
                    {
                        ++parameterVisitCount;
                        metadataContext.ShouldEqual(context.GetMetadataOrDefault());
                        if (visitor is BindingMemberExpressionVisitor expressionVisitor)
                        {
                            expressionVisitor.Flags.ShouldEqual(BindingMemberExpressionFlags.Observable);
                            expressionVisitor.IgnoreIndexMembers.ShouldBeTrue();
                            expressionVisitor.IgnoreMethodMembers.ShouldBeTrue();
                            expressionVisitor.MemberFlags.ShouldEqual(MemberFlags.All & ~MemberFlags.NonPublic);
                        }

                        return null;
                    }
                }),
                new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.Fallback), new UnaryExpressionNode(UnaryTokenType.Minus,
                    new TestBindingMemberExpressionNode
                    {
                        GetBindingSource = (t, s, m) =>
                        {
                            t.ShouldEqual(target);
                            src.ShouldEqual(s);
                            m.ShouldEqual(context.GetMetadataOrDefault());
                            return fallback;
                        }
                    })),
                new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.TargetNullValue), ConstantExpressionNode.Get(nullValue))
            };
            var exp = new TestCompiledExpression();
            var compiler = new ExpressionCompiler();
            compiler.AddComponent(new TestExpressionCompilerComponent
            {
                TryCompile = (node, m) =>
                {
                    node.ShouldEqual(parameters[2].Right);
                    m.ShouldEqual(context.GetMetadataOrDefault());
                    return exp;
                }
            });

            var initializer = new BindingParameterInitializer(compiler);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, parameters, DefaultMetadata);
            if (ignore)
                context.BindingComponents[BindingParameterNameConstant.ParameterHandler] = null;
            initializer.Initialize(null!, context);
            context.BindingComponents.Count.ShouldEqual(1);
            if (ignore)
            {
                context.BindingComponents[BindingParameterNameConstant.ParameterHandler].ShouldBeNull();
                return;
            }

            parameterVisitCount.ShouldEqual(1);
            var bindingComponentProvider = (IBindingComponentProvider) context.BindingComponents[BindingParameterNameConstant.ParameterHandler]!;
            var component = (ParameterHandlerBindingComponent) bindingComponentProvider.TryGetComponent(null!, target, src, DefaultMetadata)!;

            component.Converter.Parameter.ShouldEqual(converter);
            component.Converter.Expression.ShouldBeNull();

            component.ConverterParameter.Parameter.ShouldEqual(converterParameter);
            component.ConverterParameter.Expression.ShouldBeNull();

            component.Fallback.Parameter.ShouldEqual(fallback);
            component.Fallback.Expression.ShouldEqual(exp);

            component.TargetNullValue.Parameter.ShouldEqual(nullValue);
            component.TargetNullValue.Expression.ShouldBeNull();
        }

        #endregion
    }
}