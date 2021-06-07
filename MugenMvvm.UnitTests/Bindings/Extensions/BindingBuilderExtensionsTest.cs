using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Build;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Converting;
using MugenMvvm.Tests.Bindings.Core;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Extensions
{
    public class BindingBuilderExtensionsTest : UnitTestBase
    {
        private static readonly BindingExpressionRequest ConverterRequest = new("", null, default);
        private static readonly BindingBuilderDelegate<object, object> Delegate = target => ConverterRequest;

        public BindingBuilderExtensionsTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Fact]
        public void BindShouldBuildBinding1()
        {
            var target = this;
            object? source = null;
            var testBuilder = new TestBindingBuilder
            {
                Build = (o, o1, arg3) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(source);
                    arg3.ShouldEqual(DefaultMetadata);
                    return Binding;
                }
            };

            var invokeCount = 0;
            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(Delegate);
                    arg3.ShouldEqual(DefaultMetadata);
                    return testBuilder;
                }
            });

            target.Bind(Delegate, DefaultMetadata, BindingManager).Item.ShouldEqual(Binding);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void BindShouldBuildBinding2()
        {
            var del = new BindingBuilderDelegate<BindingBuilderExtensionsTest, string>(builderTarget => ConverterRequest);
            var target = this;
            var source = "";
            var testBuilder = new TestBindingBuilder
            {
                Build = (o, o1, arg3) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(source);
                    arg3.ShouldEqual(DefaultMetadata);
                    return Binding;
                }
            };

            var invokeCount = 0;
            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(del);
                    arg3.ShouldEqual(DefaultMetadata);
                    return testBuilder;
                }
            });

            target.Bind(source, del, DefaultMetadata, BindingManager).Item.ShouldEqual(Binding);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void BindShouldBuildBinding3()
        {
            var request = "Test";
            var target = this;
            var source = "";
            var testBuilder = new TestBindingBuilder
            {
                Build = (o, o1, arg3) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(source);
                    arg3.ShouldEqual(DefaultMetadata);
                    return Binding;
                }
            };

            var invokeCount = 0;
            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(request);
                    arg3.ShouldEqual(DefaultMetadata);
                    return new[] { testBuilder, testBuilder };
                }
            });

            var readOnlyList = target.Bind(request, source, DefaultMetadata, BindingManager).AsList();
            readOnlyList.Count.ShouldEqual(2);
            readOnlyList[0].ShouldEqual(Binding);
            readOnlyList[1].ShouldEqual(Binding);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void BindWithoutResultShouldBuildBinding()
        {
            var request = "Test";
            var target = this;
            var source = "";
            var buildInvokeCount = 0;
            var testBuilder = new TestBindingBuilder
            {
                Build = (o, o1, arg3) =>
                {
                    ++buildInvokeCount;
                    o.ShouldEqual(target);
                    o1.ShouldEqual(source);
                    arg3.ShouldEqual(DefaultMetadata);
                    return Binding;
                }
            };
            var invokeCount = 0;
            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(request);
                    arg3.ShouldEqual(DefaultMetadata);
                    return new[] { testBuilder, testBuilder };
                }
            });

            target.Bind(request, source, DefaultMetadata, BindingManager, false).IsEmpty.ShouldBeTrue();
            invokeCount.ShouldEqual(1);
            buildInvokeCount.ShouldEqual(2);
        }

        [Fact]
        public void CommandParameterTest()
        {
            var target = "T";
            var source = "S";

            var builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            var request = (BindingExpressionRequest)builder.CommandParameterSource();
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.ShouldEqual(new KeyValuePair<string?, object>(BindingParameterNameConstant.CommandParameter, MemberExpressionNode.Empty));

            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.CommandParameter(this);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.Key.ShouldEqual(BindingParameterNameConstant.CommandParameter);
            ((IExpressionNode)request.Parameters.Item.Value).ShouldEqual(ConstantExpressionNode.Get(this));

            Expression<Func<IBindingBuilderContext<object, string>, object>> expression = context => context.Source;
            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.CommandParameter(expression);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.ShouldEqual(new KeyValuePair<string?, object>(BindingParameterNameConstant.CommandParameter, expression));
        }

        [Fact]
        public void ConverterParameterTest()
        {
            var target = "T";
            var source = "S";

            var builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            var request = (BindingExpressionRequest)builder.ConverterParameterSource();
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.ShouldEqual(new KeyValuePair<string?, object>(BindingParameterNameConstant.ConverterParameter, MemberExpressionNode.Empty));

            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.ConverterParameter(this);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.Key.ShouldEqual(BindingParameterNameConstant.ConverterParameter);
            ((IExpressionNode)request.Parameters.Item.Value).ShouldEqual(ConstantExpressionNode.Get(this));

            Expression<Func<IBindingBuilderContext<object, string>, object>> expression = context => context.Source;
            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.ConverterParameter(expression);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.ShouldEqual(new KeyValuePair<string?, object>(BindingParameterNameConstant.ConverterParameter, expression));
        }

        [Fact]
        public void ConverterTest()
        {
            var target = "T";
            var source = "S";
            var converter = new TestBindingValueConverter();

            var builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            var request = (BindingExpressionRequest)builder.Converter(converter);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.Key.ShouldEqual(BindingParameterNameConstant.Converter);
            ((IExpressionNode)request.Parameters.Item.Value).ShouldEqual(ConstantExpressionNode.Get(converter));

            Expression<Func<IBindingBuilderContext<object, string>, object>> expression = context => context.Source;
            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.Converter(expression);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.ShouldEqual(new KeyValuePair<string?, object>(BindingParameterNameConstant.Converter, expression));
        }

        [Fact]
        public void DelayTest()
        {
            var target = "T";
            var source = "S";

            var builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            var request = (BindingExpressionRequest)builder.Delay(100);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.ShouldEqual(new KeyValuePair<string?, object>(BindingParameterNameConstant.Delay, 100));

            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.TargetDelay(100);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.ShouldEqual(new KeyValuePair<string?, object>(BindingParameterNameConstant.TargetDelay, 100));
        }

        [Fact]
        public void FallbackTest()
        {
            var target = "T";
            var source = "S";
            var value = this;

            var builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            var request = (BindingExpressionRequest)builder.Fallback(value);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.Key.ShouldEqual(BindingParameterNameConstant.Fallback);
            ((IExpressionNode)request.Parameters.Item.Value).ShouldEqual(ConstantExpressionNode.Get(value));

            Expression<Func<IBindingBuilderContext<object, string>, object>> expression = context => context.Source;
            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.Fallback(expression);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.ShouldEqual(new KeyValuePair<string?, object>(BindingParameterNameConstant.Fallback, expression));
        }

        [Fact]
        public void ModeTest()
        {
            var target = "T";
            var source = "S";

            var builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            var request = (BindingExpressionRequest)builder.TwoWay();
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.ShouldEqual(new KeyValuePair<string?, object>(null, MemberExpressionNode.TwoWayMode));

            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.OneWay();
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.ShouldEqual(new KeyValuePair<string?, object>(null, MemberExpressionNode.OneWayMode));

            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.OneWayToSource();
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.ShouldEqual(new KeyValuePair<string?, object>(null, MemberExpressionNode.OneWayToSourceMode));

            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.OneTime();
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.ShouldEqual(new KeyValuePair<string?, object>(null, MemberExpressionNode.OneTimeMode));

            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.NoneMode();
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.ShouldEqual(new KeyValuePair<string?, object>(null, MemberExpressionNode.NoneMode));
        }

        [Fact]
        public void ParseBindingExpressionShouldUseBindingBuilderRequest()
        {
            var testBuilder = new TestBindingBuilder();
            var invokeCount = 0;
            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(Delegate);
                    arg3.ShouldEqual(DefaultMetadata);
                    return testBuilder;
                }
            });

            BindingManager.ParseBindingExpression(Delegate, DefaultMetadata).Item.ShouldEqual(testBuilder);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TargetNullValueTest()
        {
            var target = "T";
            var source = "S";
            var value = this;

            var builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            var request = (BindingExpressionRequest)builder.TargetNullValue(value);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            request.Parameters.Item.Key.ShouldEqual(BindingParameterNameConstant.TargetNullValue);
            ((IExpressionNode)request.Parameters.Item.Value).ShouldEqual(ConstantExpressionNode.Get(value));
        }

        protected override IBindingManager GetBindingManager() => new BindingManager(ComponentCollectionManager);

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BoolParameterTest(bool value)
        {
            var target = "T";
            var source = "S";

            var builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            var request = (BindingExpressionRequest)builder.Observable(value);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            ValidateBoolExpression(request.Parameters.Item, value, MemberExpressionNode.ObservableParameter);

            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.Optional(value);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            ValidateBoolExpression(request.Parameters.Item, value, MemberExpressionNode.OptionalParameter);

            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.HasStablePath(value);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            ValidateBoolExpression(request.Parameters.Item, value, MemberExpressionNode.HasStablePathParameter);

            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.ToggleEnabledState(value);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            ValidateBoolExpression(request.Parameters.Item, value, MemberExpressionNode.ToggleEnabledParameter);

            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.SuppressMethodAccessors(value);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            ValidateBoolExpression(request.Parameters.Item, value, MemberExpressionNode.SuppressMethodAccessorsParameter);

            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.SuppressIndexAccessors(value);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            ValidateBoolExpression(request.Parameters.Item, value, MemberExpressionNode.SuppressIndexAccessorsParameter);

            builder = new BindingBuilderTo<object, string>(new BindingBuilderFrom<object, string>(target), source, default);
            request = builder.ObservableMethods(value);
            request.Target.ShouldEqual(target);
            request.Source.ShouldEqual(source);
            ValidateBoolExpression(request.Parameters.Item, value, MemberExpressionNode.ObservableMethodsParameter);
        }

        private static void ValidateBoolExpression(KeyValuePair<string?, object> parameter, bool value, IExpressionNode expression)
        {
            parameter.Key.ShouldBeNull();
            if (value)
                parameter.Value.ShouldEqual(expression);
            else
                new UnaryExpressionNode(UnaryTokenType.LogicalNegation, expression).ShouldEqual((IExpressionNode)parameter.Value);
        }
    }
}