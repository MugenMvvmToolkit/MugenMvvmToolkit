using System;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Build;
using MugenMvvm.Bindings.Interfaces.Converting;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Extensions
{
    public static class BindingBuilderExtensions
    {
        public static ItemOrIReadOnlyList<IBindingBuilder> ParseBindingExpression<TTarget, TSource>(this IBindingManager? bindingManager,
            BindingBuilderDelegate<TTarget, TSource> getBuilder, IReadOnlyMetadataContext? metadata)
            where TTarget : class
            where TSource : class =>
            bindingManager
                .DefaultIfNull()
                .ParseBindingExpression(expression: getBuilder, metadata);

        public static ItemOrIReadOnlyList<IBinding> Bind<TTarget>(this TTarget target, BindingBuilderDelegate<TTarget, object> getBuilder,
            IReadOnlyMetadataContext? metadata = null,
            IBindingManager? bindingManager = null)
            where TTarget : class =>
            bindingManager.BindInternal(getBuilder, target, null, metadata);

        public static ItemOrIReadOnlyList<IBinding> Bind<TTarget, TSource>(this TTarget target, TSource? source, BindingBuilderDelegate<TTarget, TSource> getBuilder,
            IReadOnlyMetadataContext? metadata = null, IBindingManager? bindingManager = null)
            where TTarget : class
            where TSource : class =>
            bindingManager.BindInternal(getBuilder, target, source, metadata);

        public static ItemOrIReadOnlyList<IBinding> BindToSelf<TTarget>(this TTarget target, BindingBuilderDelegate<TTarget, TTarget> getBuilder,
            IReadOnlyMetadataContext? metadata = null, IBindingManager? bindingManager = null)
            where TTarget : class =>
            bindingManager.BindInternal(getBuilder, target, target, metadata);

        public static ItemOrIReadOnlyList<IBinding> Bind<TTarget>(this TTarget target, string expression, object? source = null, IReadOnlyMetadataContext? metadata = null,
            IBindingManager? bindingManager = null, bool includeResult = true)
            where TTarget : class
        {
            if (includeResult)
                return bindingManager.BindInternal(expression, target, source, metadata);
            bindingManager.BindInternalWithoutBindings(expression, target, source, metadata);
            return default;
        }

        public static ItemOrIReadOnlyList<IBinding> BindToSelf<TTarget>(this TTarget target, string expression, IReadOnlyMetadataContext? metadata = null,
            IBindingManager? bindingManager = null, bool includeResult = true)
            where TTarget : class =>
            target.Bind(expression, target, metadata, bindingManager, includeResult);

        public static BindingBuilderTo<TTarget, TSource> TwoWay<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class =>
            builder.BindingParameter(null, MemberExpressionNode.TwoWayMode);

        public static BindingBuilderTo<TTarget, TSource> OneWay<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class =>
            builder.BindingParameter(null, MemberExpressionNode.OneWayMode);

        public static BindingBuilderTo<TTarget, TSource> OneWayToSource<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class =>
            builder.BindingParameter(null, MemberExpressionNode.OneWayToSourceMode);

        public static BindingBuilderTo<TTarget, TSource> OneTime<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class =>
            builder.BindingParameter(null, MemberExpressionNode.OneTimeMode);

        public static BindingBuilderTo<TTarget, TSource> NoneMode<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class =>
            builder.BindingParameter(null, MemberExpressionNode.NoneMode);

        public static BindingBuilderTo<TTarget, TSource> Observable<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class =>
            builder.BoolParameter(MemberExpressionNode.ObservableParameter, value);

        public static BindingBuilderTo<TTarget, TSource> Optional<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class =>
            builder.BoolParameter(MemberExpressionNode.OptionalParameter, value);

        public static BindingBuilderTo<TTarget, TSource> HasStablePath<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class =>
            builder.BoolParameter(MemberExpressionNode.HasStablePathParameter, value);

        public static BindingBuilderTo<TTarget, TSource> ToggleEnabledState<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class =>
            builder.BoolParameter(MemberExpressionNode.ToggleEnabledParameter, value);

        public static BindingBuilderTo<TTarget, TSource> SuppressMethodAccessors<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class =>
            builder.BoolParameter(MemberExpressionNode.SuppressMethodAccessorsParameter, value);

        public static BindingBuilderTo<TTarget, TSource> SuppressIndexAccessors<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class =>
            builder.BoolParameter(MemberExpressionNode.SuppressIndexAccessorsParameter, value);

        public static BindingBuilderTo<TTarget, TSource> ObservableMethods<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class =>
            builder.BoolParameter(MemberExpressionNode.ObservableMethodsParameter, value);

        public static BindingBuilderTo<TTarget, TSource> Delay<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, int delay)
            where TTarget : class
            where TSource : class =>
            builder.BindingParameter(BindingParameterNameConstant.Delay, BoxingExtensions.Box(delay));

        public static BindingBuilderTo<TTarget, TSource> TargetDelay<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, int delay)
            where TTarget : class
            where TSource : class =>
            builder.BindingParameter(BindingParameterNameConstant.TargetDelay, BoxingExtensions.Box(delay));

        public static BindingBuilderTo<TTarget, TSource> CommandParameterSource<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class =>
            builder.BindingParameter(BindingParameterNameConstant.CommandParameter, MemberExpressionNode.Empty);

        public static BindingBuilderTo<TTarget, TSource> CommandParameter<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, object? value)
            where TTarget : class
            where TSource : class =>
            builder.BindingParameter(BindingParameterNameConstant.CommandParameter, ConstantExpressionNode.Get(value));

        public static BindingBuilderTo<TTarget, TSource> CommandParameter<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder,
            Expression<Func<IBindingBuilderContext<TTarget, TSource>, object>> expression)
            where TTarget : class
            where TSource : class
        {
            Should.NotBeNull(expression, nameof(expression));
            return builder.BindingParameter(BindingParameterNameConstant.CommandParameter, expression);
        }

        public static BindingBuilderTo<TTarget, TSource> Converter<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, IBindingValueConverter converter)
            where TTarget : class
            where TSource : class
        {
            Should.NotBeNull(converter, nameof(converter));
            return builder.BindingParameter(BindingParameterNameConstant.Converter, ConstantExpressionNode.Get(converter));
        }

        public static BindingBuilderTo<TTarget, TSource> Converter<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder,
            Expression<Func<IBindingBuilderContext<TTarget, TSource>, object>> expression)
            where TTarget : class
            where TSource : class
        {
            Should.NotBeNull(expression, nameof(expression));
            return builder.BindingParameter(BindingParameterNameConstant.Converter, expression);
        }

        public static BindingBuilderTo<TTarget, TSource> ConverterParameterSource<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class =>
            builder.BindingParameter(BindingParameterNameConstant.ConverterParameter, MemberExpressionNode.Empty);

        public static BindingBuilderTo<TTarget, TSource> ConverterParameter<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, object? value)
            where TTarget : class
            where TSource : class =>
            builder.BindingParameter(BindingParameterNameConstant.ConverterParameter, ConstantExpressionNode.Get(value));

        public static BindingBuilderTo<TTarget, TSource> ConverterParameter<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder,
            Expression<Func<IBindingBuilderContext<TTarget, TSource>, object>> expression)
            where TTarget : class
            where TSource : class
        {
            Should.NotBeNull(expression, nameof(expression));
            return builder.BindingParameter(BindingParameterNameConstant.ConverterParameter, expression);
        }

        public static BindingBuilderTo<TTarget, TSource> Fallback<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, object? value)
            where TTarget : class
            where TSource : class =>
            builder.BindingParameter(BindingParameterNameConstant.Fallback, ConstantExpressionNode.Get(value));

        public static BindingBuilderTo<TTarget, TSource> Fallback<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder,
            Expression<Func<IBindingBuilderContext<TTarget, TSource>, object>> expression)
            where TTarget : class
            where TSource : class
        {
            Should.NotBeNull(expression, nameof(expression));
            return builder.BindingParameter(BindingParameterNameConstant.Fallback, expression);
        }

        public static BindingBuilderTo<TTarget, TSource> TargetNullValue<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, object? value)
            where TTarget : class
            where TSource : class =>
            builder.BindingParameter(BindingParameterNameConstant.TargetNullValue, ConstantExpressionNode.Get(value));

        public static BindingBuilderTo<TTarget, TSource> Trace<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, string tag)
            where TTarget : class
            where TSource : class
        {
            Should.NotBeNullOrEmpty(tag, nameof(tag));
            return builder.BindingParameter(BindingParameterNameConstant.Trace, ConstantExpressionNode.Get(tag));
        }

        private static void BindInternalWithoutBindings(this IBindingManager? bindingManager, object request, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(target, nameof(target));
            foreach (var bindingBuilder in bindingManager.DefaultIfNull().ParseBindingExpression(request, metadata))
                bindingBuilder.Build(target, source, metadata);
        }

        private static ItemOrIReadOnlyList<IBinding> BindInternal(this IBindingManager? bindingManager, object request, object target, object? source,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(target, nameof(target));
            var expressions = bindingManager
                              .DefaultIfNull()
                              .ParseBindingExpression(request, metadata);

            if (expressions.Item != null)
                return ItemOrIReadOnlyList.FromItem(expressions.Item.Build(target, source, metadata));

            var result = new IBinding[expressions.Count];
            var index = 0;
            foreach (var builder in expressions)
                result[index++] = builder.Build(target, source, metadata);
            return result;
        }

        private static BindingBuilderTo<TTarget, TSource> BoolParameter<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, IExpressionNode parameter, bool value)
            where TTarget : class
            where TSource : class
        {
            if (value)
                return builder.BindingParameter(null, parameter);
            return builder.BindingParameter(null, new UnaryExpressionNode(UnaryTokenType.LogicalNegation, parameter));
        }
    }
}