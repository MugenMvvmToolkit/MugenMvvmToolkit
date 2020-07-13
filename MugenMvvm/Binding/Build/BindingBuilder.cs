using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Build;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Convert;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Build
{
    public static class BindingBuilder
    {
        #region Methods

        public static ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> ParseBindingExpression<TTarget, TSource>(this IBindingManager? bindingManager,
            BindingBuilderDelegate<TTarget, TSource> getBuilder, IReadOnlyMetadataContext? metadata)
            where TTarget : class
            where TSource : class
        {
            return bindingManager
                .DefaultIfNull()
                .ParseBindingExpression(BindingBuilderRequest.Get(getBuilder), metadata);
        }

        public static ItemOrList<IBinding, IReadOnlyList<IBinding>> Bind<TTarget>(this TTarget target, BindingBuilderDelegate<TTarget, object> getBuilder, IReadOnlyMetadataContext? metadata = null, IBindingManager? bindingManager = null)
            where TTarget : class
        {
            return bindingManager.BindInternal(BindingBuilderRequest.Get(getBuilder), target, null, metadata);
        }

        public static ItemOrList<IBinding, IReadOnlyList<IBinding>> Bind<TTarget, TSource>(this TTarget target, TSource? source, BindingBuilderDelegate<TTarget, TSource> getBuilder, IReadOnlyMetadataContext? metadata = null, IBindingManager? bindingManager = null)
            where TTarget : class
            where TSource : class
        {
            return bindingManager.BindInternal(BindingBuilderRequest.Get(getBuilder), target, source, metadata);
        }

        public static ItemOrList<IBinding, IReadOnlyList<IBinding>> Bind<TTarget>(this TTarget target, string expression, object? source = null, IReadOnlyMetadataContext? metadata = null, IBindingManager? bindingManager = null)
            where TTarget : class
        {
            return bindingManager.BindInternal(expression, target, source, metadata);
        }

        public static void BindWithoutResult<TTarget>(this TTarget target, string expression, object? source = null, IReadOnlyMetadataContext? metadata = null, IBindingManager? bindingManager = null)
            where TTarget : class
        {
            bindingManager.BindInternalWithoutBindings(expression, target, source, metadata);
        }

        public static BindingBuilderTo<TTarget, TSource> TwoWay<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class
        {
            return builder.BindingParameter(null, MemberExpressionNode.TwoWayMode);
        }

        public static BindingBuilderTo<TTarget, TSource> OneWay<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class
        {
            return builder.BindingParameter(null, MemberExpressionNode.OneWayMode);
        }

        public static BindingBuilderTo<TTarget, TSource> OneWayToSource<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class
        {
            return builder.BindingParameter(null, MemberExpressionNode.OneWayToSourceMode);
        }

        public static BindingBuilderTo<TTarget, TSource> OneTime<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class
        {
            return builder.BindingParameter(null, MemberExpressionNode.OneTimeMode);
        }

        public static BindingBuilderTo<TTarget, TSource> NoneMode<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class
        {
            return builder.BindingParameter(null, MemberExpressionNode.NoneMode);
        }

        public static BindingBuilderTo<TTarget, TSource> Observable<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class
        {
            return builder.BoolParameter(MemberExpressionNode.ObservableParameter, value);
        }

        public static BindingBuilderTo<TTarget, TSource> Optional<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class
        {
            return builder.BoolParameter(MemberExpressionNode.OptionalParameter, value);
        }

        public static BindingBuilderTo<TTarget, TSource> HasStablePath<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class
        {
            return builder.BoolParameter(MemberExpressionNode.HasStablePathParameter, value);
        }

        public static BindingBuilderTo<TTarget, TSource> ToggleEnabledState<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class
        {
            return builder.BoolParameter(MemberExpressionNode.ToggleEnabledParameter, value);
        }

        public static BindingBuilderTo<TTarget, TSource> IgnoreMethodMembers<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class
        {
            return builder.BoolParameter(MemberExpressionNode.IgnoreMethodMembersParameter, value);
        }

        public static BindingBuilderTo<TTarget, TSource> IgnoreIndexMembers<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class
        {
            return builder.BoolParameter(MemberExpressionNode.IgnoreIndexMembersParameter, value);
        }

        public static BindingBuilderTo<TTarget, TSource> ObservableMethods<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, bool value = true)
            where TTarget : class
            where TSource : class
        {
            return builder.BoolParameter(MemberExpressionNode.ObservableMethodsParameter, value);
        }

        public static BindingBuilderTo<TTarget, TSource> Delay<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, int delay)
            where TTarget : class
            where TSource : class
        {
            return builder.BindingParameter(BindingParameterNameConstant.Delay, BoxingExtensions.Box(delay));
        }

        public static BindingBuilderTo<TTarget, TSource> TargetDelay<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, int delay)
            where TTarget : class
            where TSource : class
        {
            return builder.BindingParameter(BindingParameterNameConstant.TargetDelay, BoxingExtensions.Box(delay));
        }

        public static BindingBuilderTo<TTarget, TSource> CommandParameterSource<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class
        {
            return builder.BindingParameter(BindingParameterNameConstant.CommandParameter, MemberExpressionNode.Empty);
        }

        public static BindingBuilderTo<TTarget, TSource> CommandParameter<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, object? value)
            where TTarget : class
            where TSource : class
        {
            return builder.BindingParameter(BindingParameterNameConstant.CommandParameter, ConstantExpressionNode.Get(value));
        }

        public static BindingBuilderTo<TTarget, TSource> CommandParameter<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, Expression<Func<IBindingBuilderContext<TTarget, TSource>, object>> expression)
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

        public static BindingBuilderTo<TTarget, TSource> Converter<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, Expression<Func<IBindingBuilderContext<TTarget, TSource>, object>> expression)
            where TTarget : class
            where TSource : class
        {
            Should.NotBeNull(expression, nameof(expression));
            return builder.BindingParameter(BindingParameterNameConstant.Converter, expression);
        }

        public static BindingBuilderTo<TTarget, TSource> ConverterParameterSource<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder)
            where TTarget : class
            where TSource : class
        {
            return builder.BindingParameter(BindingParameterNameConstant.ConverterParameter, MemberExpressionNode.Empty);
        }

        public static BindingBuilderTo<TTarget, TSource> ConverterParameter<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, object? value)
            where TTarget : class
            where TSource : class
        {
            return builder.BindingParameter(BindingParameterNameConstant.ConverterParameter, ConstantExpressionNode.Get(value));
        }

        public static BindingBuilderTo<TTarget, TSource> ConverterParameter<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, Expression<Func<IBindingBuilderContext<TTarget, TSource>, object>> expression)
            where TTarget : class
            where TSource : class
        {
            Should.NotBeNull(expression, nameof(expression));
            return builder.BindingParameter(BindingParameterNameConstant.ConverterParameter, expression);
        }

        public static BindingBuilderTo<TTarget, TSource> Fallback<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, object? value)
            where TTarget : class
            where TSource : class
        {
            return builder.BindingParameter(BindingParameterNameConstant.Fallback, ConstantExpressionNode.Get(value));
        }

        public static BindingBuilderTo<TTarget, TSource> Fallback<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, Expression<Func<IBindingBuilderContext<TTarget, TSource>, object>> expression)
            where TTarget : class
            where TSource : class
        {
            Should.NotBeNull(expression, nameof(expression));
            return builder.BindingParameter(BindingParameterNameConstant.Fallback, expression);
        }

        public static BindingBuilderTo<TTarget, TSource> TargetNullValue<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, object? value)
            where TTarget : class
            where TSource : class
        {
            return builder.BindingParameter(BindingParameterNameConstant.TargetNullValue, ConstantExpressionNode.Get(value));
        }

        private static void BindInternalWithoutBindings<TRequest>(this IBindingManager? bindingManager, [DisallowNull] in TRequest request, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(target, nameof(target));
            foreach (var bindingBuilder in bindingManager.DefaultIfNull().ParseBindingExpression(request, metadata).Iterator())
                bindingBuilder.Build(target, source, metadata);
        }

        private static ItemOrList<IBinding, IReadOnlyList<IBinding>> BindInternal<TRequest>(this IBindingManager? bindingManager, [DisallowNull] in TRequest request, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(target, nameof(target));
            var expressions = bindingManager
                .DefaultIfNull()
                .ParseBindingExpression(request, metadata);

            if (expressions.Item != null)
                return ItemOrList.FromItem(expressions.Item.Build(target, source, metadata));

            var iterator = expressions.Iterator();
            var result = new IBinding[iterator.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = iterator[i].Build(target, source, metadata);
            return ItemOrList.FromListToReadOnly(result);
        }

        private static BindingBuilderTo<TTarget, TSource> BoolParameter<TTarget, TSource>(this BindingBuilderTo<TTarget, TSource> builder, IExpressionNode parameter, bool value)
            where TTarget : class
            where TSource : class
        {
            if (value)
                return builder.BindingParameter(null, parameter);
            return builder.BindingParameter(null, new UnaryExpressionNode(UnaryTokenType.LogicalNegation, parameter));
        }

        #endregion
    }
}