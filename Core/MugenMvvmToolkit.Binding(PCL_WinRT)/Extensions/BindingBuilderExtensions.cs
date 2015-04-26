#region Copyright

// ****************************************************************************
// <copyright file="BindingBuilderExtensions.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Interfaces.Syntax;
using MugenMvvmToolkit.Binding.Parse;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.Binding
{
    /// <summary>
    ///     Represents the binding builders extensions.
    /// </summary>
    public static class BindingBuilderExtensions
    {
        #region Fields

        private static readonly DataConstant<object> SyntaxBuilderConstant;

        #endregion

        #region Constructors

        static BindingBuilderExtensions()
        {
            SyntaxBuilderConstant = DataConstant.Create(() => SyntaxBuilderConstant, true);
        }

        #endregion

        #region Methods

        #region Bind

        public static IBindingToSyntax<TTarget, TSource> Bind<TTarget, TSource>([NotNull] this IBindingBuilder builder, [NotNull] TTarget targetGeneric, [NotNull] string targetPath) where TTarget : class
        {
            Should.NotBeNull(builder, "builder");
            Should.NotBeNull(targetGeneric, "targetGeneric");
            Should.NotBeNullOrWhitespace(targetPath, "targetPath");
            builder.Add(BindingBuilderConstants.Target, targetGeneric);
            builder.Add(BindingBuilderConstants.TargetPath, BindingServiceProvider.BindingPathFactory(targetPath));
            return new SyntaxBuilder<TTarget, TSource>(builder);
        }

        public static IBindingToSyntax<TTarget> Bind<TTarget>([NotNull] this IBindingBuilder builder,
            [NotNull] TTarget targetGeneric, [NotNull] string targetPath) where TTarget : class
        {
            return builder.Bind<TTarget, object>(targetGeneric, targetPath);
        }

        public static IBindingToSyntax Bind([NotNull] this IBindingBuilder builder, [NotNull] object target,
            [NotNull] string targetPath)
        {
            return builder.Bind<object, object>(target, targetPath);
        }

        [Obsolete(BindingExceptionManager.ObsoleteExpressionUsage)]
        public static IBindingToSyntax<TTarget, TSource> Bind<TTarget, TSource>([NotNull] this IBindingBuilder builder, [NotNull] TTarget target,
            [NotNull] Expression<Func<TTarget, object>> targetPath) where TTarget : class
        {
            return builder.Bind<TTarget, TSource>(target, BindingExtensions.GetMemberPath(targetPath));
        }

        [Obsolete(BindingExceptionManager.ObsoleteExpressionUsage)]
        public static IBindingToSyntax<TTarget> Bind<TTarget>([NotNull] this IBindingBuilder builder,
            [NotNull] TTarget target, [NotNull] Expression<Func<TTarget, object>> targetPath) where TTarget : class
        {
            return builder.Bind<TTarget, object>(target, targetPath);
        }

        public static IBindingToSyntax<TTarget, TSource> Bind<TTarget, TSource>([NotNull] this IBindingBuilder builder, [NotNull] TTarget target,
            [NotNull] Func<Expression<Func<TTarget, object>>> targetPath) where TTarget : class
        {
            return builder.Bind<TTarget, TSource>(target, BindingExtensions.GetMemberPath(targetPath));
        }

        public static IBindingToSyntax<TTarget> Bind<TTarget>([NotNull] this IBindingBuilder builder,
            [NotNull] TTarget target, [NotNull] Func<Expression<Func<TTarget, object>>> targetPath) where TTarget : class
        {
            return builder.Bind<TTarget, object>(target, targetPath);
        }

        #endregion

        #region To

        public static IBindingModeInfoBehaviorSyntax ToSource([NotNull] this IBindingToSyntax syntax,
            [NotNull] Func<IDataContext, IBindingSource> bindingSourceDelegate)
        {
            Should.NotBeNull(syntax, "syntax");
            Should.NotBeNull(bindingSourceDelegate, "bindingSourceDelegate");
            syntax.Builder.GetOrAddBindingSources().Add(bindingSourceDelegate);
            return syntax.GetOrAddSyntaxBuilder<IBindingModeInfoBehaviorSyntax, object, object>();
        }

        public static IBindingModeInfoBehaviorSyntax To([NotNull] this IBindingToSyntax syntax, [NotNull] object source, [NotNull] string sourcePath)
        {
            Should.NotBeNull(source, "source");
            Should.NotBeNull(sourcePath, "sourcePath");
            syntax.Builder.AddOrUpdate(BindingBuilderConstants.Source, source);
            return syntax.ToSource(context => BindingExtensions.CreateBindingSourceExplicit(context, sourcePath, null));
        }

        public static IBindingModeInfoBehaviorSyntax To([NotNull] this IBindingToSyntax syntax, [NotNull] string sourcePath)
        {
            Should.NotBeNull(sourcePath, "sourcePath");
            return syntax.ToSource(context => BindingExtensions.CreateBindingSource(context, sourcePath));
        }

        public static IBindingModeInfoBehaviorSyntax ToSelf([NotNull] this IBindingToSyntax syntax, [NotNull] string selfPath)
        {
            Should.NotBeNull(selfPath, "selfPath");
            return syntax.ToSource(context => BindingExtensions.CreateBindingSourceSelf(context, selfPath));
        }

        [Obsolete(BindingExceptionManager.ObsoleteExpressionUsage)]
        public static IBindingModeInfoBehaviorSyntax To<TSource>([NotNull] this IBindingToSyntax syntax, TSource source, [NotNull] Expression<Func<TSource, object>> expression)
        {
            Should.NotBeNull(expression, "expression");
            Should.NotBeNull(source, "source");
            syntax.Builder.AddOrUpdate(BindingBuilderConstants.Source, source);
            return LambdaExpressionToBindingExpressionConverter.Convert(expression, syntax, BindingExtensions.CreteBindingSourceFromSourceDel);
        }

        [Obsolete(BindingExceptionManager.ObsoleteExpressionUsage)]
        public static IBindingModeInfoBehaviorSyntax To<TSource>([NotNull] this IBindingToSyntax syntax, [NotNull] Expression<Func<TSource, object>> expression)
        {
            Should.NotBeNull(expression, "expression");
            return LambdaExpressionToBindingExpressionConverter.Convert(expression, syntax, BindingExtensions.CreteBindingSourceFromContextDel);
        }

        [Obsolete(BindingExceptionManager.ObsoleteExpressionUsage)]
        public static IBindingModeInfoBehaviorSyntax To<TTarget, TSource>([NotNull] this IBindingToSyntax<TTarget, TSource> syntax, [NotNull] Expression<Func<TSource, object>> expression)
        {
            Should.NotBeNull(expression, "expression");
            return LambdaExpressionToBindingExpressionConverter.Convert(expression, syntax, BindingExtensions.CreteBindingSourceFromContextDel);
        }

        [Obsolete(BindingExceptionManager.ObsoleteExpressionUsage)]
        public static IBindingModeInfoBehaviorSyntax ToSelf<TTarget>([NotNull] this IBindingToSyntax<TTarget> syntaxGeneric, [NotNull] Expression<Func<TTarget, object>> expression)
        {
            Should.NotBeNull(expression, "expression");
            return LambdaExpressionToBindingExpressionConverter.Convert(expression, syntaxGeneric, BindingExtensions.CreteBindingSourceFromSelfDel);
        }

        public static IBindingModeInfoBehaviorSyntax To<TSource>([NotNull] this IBindingToSyntax syntax, TSource source, [NotNull] Func<Expression<Func<TSource, object>>> expression)
        {
            Should.NotBeNull(expression, "expression");
            Should.NotBeNull(source, "source");
            syntax.Builder.AddOrUpdate(BindingBuilderConstants.Source, source);
            return LambdaExpressionToBindingExpressionConverter.Convert(expression, syntax, BindingExtensions.CreteBindingSourceFromSourceDel);
        }

        public static IBindingModeInfoBehaviorSyntax ToAction<TSource>([NotNull] this IBindingToSyntax syntax, TSource source, [NotNull] Func<Expression<Action<TSource>>> expression)
        {
            Should.NotBeNull(expression, "expression");
            Should.NotBeNull(source, "source");
            syntax.Builder.AddOrUpdate(BindingBuilderConstants.Source, source);
            return LambdaExpressionToBindingExpressionConverter.Convert(expression, syntax, BindingExtensions.CreteBindingSourceFromSourceDel);
        }

        public static IBindingModeInfoBehaviorSyntax To<TSource>([NotNull] this IBindingToSyntax syntax, [NotNull] Func<Expression<Func<TSource, object>>> expression)
        {
            Should.NotBeNull(expression, "expression");
            return LambdaExpressionToBindingExpressionConverter.Convert(expression, syntax, BindingExtensions.CreteBindingSourceFromContextDel);
        }

        public static IBindingModeInfoBehaviorSyntax ToAction<TSource>([NotNull] this IBindingToSyntax syntax, [NotNull] Func<Expression<Action<TSource>>> expression)
        {
            Should.NotBeNull(expression, "expression");
            return LambdaExpressionToBindingExpressionConverter.Convert(expression, syntax, BindingExtensions.CreteBindingSourceFromContextDel);
        }

        public static IBindingModeInfoBehaviorSyntax To<TTarget, TSource>([NotNull] this IBindingToSyntax<TTarget, TSource> syntax, [NotNull] Func<Expression<Func<TSource, object>>> expression)
        {
            Should.NotBeNull(expression, "expression");
            return LambdaExpressionToBindingExpressionConverter.Convert(expression, syntax, BindingExtensions.CreteBindingSourceFromContextDel);
        }

        public static IBindingModeInfoBehaviorSyntax ToAction<TTarget, TSource>([NotNull] this IBindingToSyntax<TTarget, TSource> syntax, [NotNull] Func<Expression<Action<TSource>>> expression)
        {
            Should.NotBeNull(expression, "expression");
            return LambdaExpressionToBindingExpressionConverter.Convert(expression, syntax, BindingExtensions.CreteBindingSourceFromContextDel);
        }

        public static IBindingModeInfoBehaviorSyntax ToSelf<TTarget>([NotNull] this IBindingToSyntax<TTarget> syntaxGeneric, [NotNull] Func<Expression<Func<TTarget, object>>> expression)
        {
            Should.NotBeNull(expression, "expression");
            return LambdaExpressionToBindingExpressionConverter.Convert(expression, syntaxGeneric, BindingExtensions.CreteBindingSourceFromSelfDel);
        }

        #endregion

        #region Mode

        public static IBindingInfoBehaviorSyntax TwoWay([NotNull] this IBindingModeSyntax syntax)
        {
            return syntax.WithBehaviorInternal(new TwoWayBindingMode());
        }

        public static IBindingInfoBehaviorSyntax OneWay([NotNull] this IBindingModeSyntax syntax)
        {
            return syntax.WithBehaviorInternal(new OneWayBindingMode());
        }

        public static IBindingInfoBehaviorSyntax OneWayToSource([NotNull] this IBindingModeSyntax syntax)
        {
            return syntax.WithBehaviorInternal(new OneWayToSourceBindingMode());
        }

        public static IBindingInfoBehaviorSyntax OneTime([NotNull] this IBindingModeSyntax syntax)
        {
            return syntax.WithBehaviorInternal(new OneTimeBindingMode());
        }

        public static IBindingInfoBehaviorSyntax NoneMode([NotNull] this IBindingModeSyntax syntax)
        {
            return syntax.WithBehaviorInternal(NoneBindingMode.Instance);
        }

        #endregion

        #region Parameters

        public static IBindingInfoBehaviorSyntax ToggleEnabledState([NotNull] this IBindingInfoSyntax syntax, bool value)
        {
            Should.NotBeNull(syntax, "syntax");
            syntax.Builder.Add(BindingBuilderConstants.ToggleEnabledState, value);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax, object, object>();
        }

        public static IBindingInfoBehaviorSyntax WithCommandParameter([NotNull] this IBindingInfoSyntax syntax,
            [NotNull] Func<IDataContext, object> getParameter)
        {
            Should.NotBeNull(syntax, "syntax");
            Should.NotBeNull(getParameter, "getParameter");
            syntax.Builder.Add(BindingBuilderConstants.CommandParameter, getParameter);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax, object, object>();
        }

        public static IBindingInfoBehaviorSyntax WithCommandParameter([NotNull] this IBindingInfoSyntax syntax,
            [CanBeNull] object parameter)
        {
            return syntax.WithCommandParameter(d => parameter);
        }

        public static IBindingInfoBehaviorSyntax WithConverter([NotNull] this IBindingInfoSyntax syntax,
            [NotNull] Func<IDataContext, IBindingValueConverter> getConverter)
        {
            Should.NotBeNull(syntax, "syntax");
            Should.NotBeNull(getConverter, "getConverter");
            syntax.Builder.Add(BindingBuilderConstants.Converter, getConverter);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax, object, object>();
        }

        public static IBindingInfoBehaviorSyntax WithConverter([NotNull] this IBindingInfoSyntax syntax,
            [NotNull] IBindingValueConverter converter)
        {
            Should.NotBeNull(converter, "converter");
            return syntax.WithConverter(d => converter);
        }

        public static IBindingInfoBehaviorSyntax WithConverterParameter([NotNull] this IBindingInfoSyntax syntax,
            [NotNull] Func<IDataContext, object> getParameter)
        {
            Should.NotBeNull(syntax, "syntax");
            Should.NotBeNull(getParameter, "getParameter");
            syntax.Builder.Add(BindingBuilderConstants.ConverterParameter, getParameter);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax, object, object>();
        }

        public static IBindingInfoBehaviorSyntax WithConverterParameter([NotNull] this IBindingInfoSyntax syntax,
            [CanBeNull] object parameter)
        {
            return syntax.WithConverterParameter(d => parameter);
        }

        public static IBindingInfoBehaviorSyntax WithConverterCulture([NotNull] this IBindingInfoSyntax syntax,
            [NotNull] Func<IDataContext, CultureInfo> getCulture)
        {
            Should.NotBeNull(syntax, "syntax");
            Should.NotBeNull(getCulture, "getCulture");
            syntax.Builder.Add(BindingBuilderConstants.ConverterCulture, getCulture);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax, object, object>();
        }

        public static IBindingInfoBehaviorSyntax WithConverterCulture([NotNull] this IBindingInfoSyntax syntax,
            [NotNull] CultureInfo culture)
        {
            Should.NotBeNull(culture, "culture");
            return syntax.WithConverterCulture(d => culture);
        }

        public static IBindingInfoBehaviorSyntax WithFallback([NotNull] this IBindingInfoSyntax syntax,
            [NotNull] Func<IDataContext, object> getFallback)
        {
            Should.NotBeNull(syntax, "syntax");
            Should.NotBeNull(getFallback, "getFallback");
            syntax.Builder.Add(BindingBuilderConstants.Fallback, getFallback);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax, object, object>();
        }

        public static IBindingInfoBehaviorSyntax WithFallback([NotNull] this IBindingInfoSyntax syntax,
            [CanBeNull] object fallback)
        {
            return syntax.WithFallback(d => fallback);
        }

        public static IBindingInfoBehaviorSyntax WithTargetNullValue([NotNull] this IBindingInfoSyntax syntax,
            [CanBeNull] object nullValue)
        {
            Should.NotBeNull(syntax, "syntax");
            syntax.Builder.Add(BindingBuilderConstants.TargetNullValue, nullValue);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax, object, object>();
        }

        public static IBindingInfoBehaviorSyntax WithBehavior([NotNull] this IBindingBehaviorSyntax syntax,
            [NotNull] IBindingBehavior behavior)
        {
            return syntax.WithBehaviorInternal(behavior);
        }

        public static IBindingInfoBehaviorSyntax LostFocusUpdateSourceTrigger(
            [NotNull] this IBindingBehaviorSyntax syntax)
        {
            return syntax.WithBehavior(new LostFocusUpdateSourceBehavior());
        }

        public static IBindingInfoBehaviorSyntax ValidatesOnNotifyDataErrors(
            [NotNull] this IBindingBehaviorSyntax syntax)
        {
            return syntax.WithBehavior(new ValidatesOnNotifyDataErrorsBehavior());
        }

        public static IBindingInfoBehaviorSyntax ValidatesOnExceptions([NotNull] this IBindingBehaviorSyntax syntax)
        {
            return syntax.WithBehavior(ValidatesOnExceptionsBehavior.Instance);
        }

        public static IBindingInfoBehaviorSyntax Validate([NotNull] this IBindingBehaviorSyntax syntax)
        {
            syntax.ValidatesOnNotifyDataErrors();
            return syntax.ValidatesOnExceptions();
        }

        public static IBindingBehaviorSyntax WithDelay([NotNull] this IBindingBehaviorSyntax syntax, uint delay, bool isTarget = false)
        {
            return syntax.WithBehavior(new DelayBindingBehavior(delay, isTarget));
        }

        public static IBindingBehaviorSyntax DefaultValueOnException([NotNull] this IBindingBehaviorSyntax syntax, object value = null)
        {
            return syntax.WithBehavior(new DefaultValueOnExceptionBehavior(value));
        }

        #endregion

        #region Internal

        internal static IList<IBindingBehavior> GetOrAddBehaviors(this IDataContext syntax)
        {
            Should.NotBeNull(syntax, "syntax");
            List<IBindingBehavior> data;
            if (!syntax.TryGetData(BindingBuilderConstants.Behaviors, out data))
            {
                data = new List<IBindingBehavior>(2);
                syntax.Add(BindingBuilderConstants.Behaviors, data);
            }
            return data;
        }

        internal static TResult GetOrAddSyntaxBuilder<TResult, T1, T2>(this IBuilderSyntax bindingSyntax) where TResult : IBindingInfoSyntax
        {
            if (bindingSyntax is TResult)
                return (TResult)bindingSyntax;
            var builder = bindingSyntax.Builder;
            object syntaxBuilder;
            if (!builder.TryGetData(SyntaxBuilderConstant, out syntaxBuilder) || !(syntaxBuilder is TResult))
            {
                syntaxBuilder = new SyntaxBuilder<T1, T2>(builder);
                builder.AddOrUpdate(SyntaxBuilderConstant, syntaxBuilder);
            }
            return (TResult)syntaxBuilder;
        }


        private static IBindingInfoBehaviorSyntax WithBehaviorInternal(this IBuilderSyntax syntax,
            IBindingBehavior behavior)
        {
            Should.NotBeNull(syntax, "syntax");
            Should.NotBeNull(behavior, "behavior");
            syntax.Builder.GetOrAddBehaviors().Add(behavior);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax, object, object>();
        }

        private static IList<Func<IDataContext, IBindingSource>> GetOrAddBindingSources([NotNull] this IDataContext syntax)
        {
            Should.NotBeNull(syntax, "syntax");
            IList<Func<IDataContext, IBindingSource>> delegates = syntax.GetData(BindingBuilderConstants.Sources);
            if (delegates == null)
            {
                delegates = new List<Func<IDataContext, IBindingSource>>(1);
                syntax.Add(BindingBuilderConstants.Sources, delegates);
            }
            return delegates;
        }

        #endregion

        #endregion
    }
}