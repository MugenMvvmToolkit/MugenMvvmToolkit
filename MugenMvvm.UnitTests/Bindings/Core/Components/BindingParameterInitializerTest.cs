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
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingParameterInitializerTest : UnitTestBase
    {
        private readonly ExpressionCompiler _compiler;
        private readonly BindingParameterInitializer _initializer;
        private readonly BindingExpressionInitializerContext _context;

        public BindingParameterInitializerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _compiler = new ExpressionCompiler(ComponentCollectionManager);
            _initializer = new BindingParameterInitializer(_compiler);
            _context = new BindingExpressionInitializerContext(this);
        }

        [Fact]
        public void InitializeShouldIgnoreEmptyParameters()
        {
            _context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);
            _initializer.Initialize(null!, _context);
            _context.Components.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InitializeShouldAddParameterHandlerComponent(bool ignore)
        {
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
                new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.ConverterParameter),
                    new TestBindingMemberExpressionNode
                    {
                        GetBindingSource = (t, s, m) =>
                        {
                            t.ShouldEqual(target);
                            src.ShouldEqual(s);
                            m.ShouldEqual(_context.GetMetadataOrDefault());
                            return converterParameter;
                        },
                        VisitHandler = (visitor, metadataContext) =>
                        {
                            ++parameterVisitCount;
                            metadataContext.ShouldEqual(_context.GetMetadataOrDefault());
                            if (visitor is BindingMemberExpressionVisitor expressionVisitor)
                            {
                                expressionVisitor.Flags.ShouldEqual(BindingMemberExpressionFlags.Observable);
                                expressionVisitor.SuppressIndexAccessors.ShouldBeTrue();
                                expressionVisitor.SuppressMethodAccessors.ShouldBeTrue();
                                expressionVisitor.MemberFlags.ShouldEqual(MemberFlags.All & ~MemberFlags.NonPublic);
                            }

                            return null;
                        }
                    }),
                new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.Fallback), new UnaryExpressionNode(
                    UnaryTokenType.Minus,
                    new TestBindingMemberExpressionNode
                    {
                        GetBindingSource = (t, s, m) =>
                        {
                            t.ShouldEqual(target);
                            src.ShouldEqual(s);
                            m.ShouldEqual(_context.GetMetadataOrDefault());
                            return fallback;
                        }
                    })),
                new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.TargetNullValue),
                    ConstantExpressionNode.Get(nullValue))
            };
            var exp = new TestCompiledExpression();
            _compiler.AddComponent(new TestExpressionCompilerComponent
            {
                TryCompile = (node, m) =>
                {
                    node.ShouldEqual(parameters[2].Right);
                    m.ShouldEqual(_context.GetMetadataOrDefault());
                    return exp;
                }
            });

            _context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, parameters, DefaultMetadata);
            if (ignore)
                _context.Components[BindingParameterNameConstant.ParameterHandler] = null;
            _initializer.Initialize(null!, _context);
            _context.Components.Count.ShouldEqual(1);
            if (ignore)
            {
                _context.Components[BindingParameterNameConstant.ParameterHandler].ShouldBeNull();
                return;
            }

            parameterVisitCount.ShouldEqual(1);
            var bindingComponentProvider = (IBindingComponentProvider) _context.Components[BindingParameterNameConstant.ParameterHandler]!;
            var component = (BindingParameterHandler) bindingComponentProvider.TryGetComponent(null!, target, src, DefaultMetadata)!;

            component.Converter.Parameter.ShouldEqual(converter);
            component.Converter.Expression.ShouldBeNull();

            component.ConverterParameter.Parameter.ShouldEqual(converterParameter);
            component.ConverterParameter.Expression.ShouldBeNull();

            component.Fallback.Parameter.ShouldEqual(fallback);
            component.Fallback.Expression.ShouldEqual(exp);

            component.TargetNullValue.Parameter.ShouldEqual(nullValue);
            component.TargetNullValue.Expression.ShouldBeNull();
        }
    }
}