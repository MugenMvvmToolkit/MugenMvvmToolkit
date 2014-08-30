#region Copyright
// ****************************************************************************
// <copyright file="BindingBuilderExtensions.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Interfaces.Syntax;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Sources;
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

        private static readonly DataConstant<SyntaxBuilder<object, object>> SyntaxBuilderConstant;

        #endregion

        #region Constructors

        static BindingBuilderExtensions()
        {
            SyntaxBuilderConstant = DataConstant.Create(() => SyntaxBuilderConstant, true);
        }

        #endregion

        #region Methods

        #region Bind

        public static IBindingToSyntax<TTarget> Bind<TTarget>([NotNull] this IBindingBuilder builder,
            [NotNull] TTarget targetGeneric, [NotNull] string targetPath) where TTarget : class
        {
            Bind(builder, target: targetGeneric, targetPath: targetPath);
            return new SyntaxBuilder<TTarget, object>(builder);
        }

        public static IBindingToSyntax<TTarget, TSource> Bind<TTarget, TSource>([NotNull] this IBindingBuilder builder,
            [NotNull] TTarget target, [NotNull] Expression<Func<TTarget, object>> targetPath) where TTarget : class
        {
            Bind(builder, target, targetPath);
            return new SyntaxBuilder<TTarget, TSource>(builder);
        }

        public static IBindingToSyntax<TTarget, TSource> Bind<TTarget, TSource>([NotNull] this IBindingBuilder builder,
            [NotNull] TTarget targetGeneric, [NotNull] string targetPath) where TTarget : class
        {
            Bind(builder, targetGeneric, targetPath);
            return new SyntaxBuilder<TTarget, TSource>(builder);
        }

        public static IBindingToSyntax<TTarget> Bind<TTarget>([NotNull] this IBindingBuilder builder,
            [NotNull] TTarget target, [NotNull] Expression<Func<TTarget, object>> targetPath) where TTarget : class
        {
            Should.NotBeNull(targetPath, "targetPath");
            Bind(builder, target, BindingExtensions.GetMemberPath(targetPath));
            return new SyntaxBuilder<TTarget, object>(builder);
        }

        public static IBindingToSyntax Bind([NotNull] this IBindingBuilder builder, [NotNull] object target,
            [NotNull] string targetPath)
        {
            Should.NotBeNull(builder, "builder");
            Should.NotBeNull(target, "target");
            Should.NotBeNullOrWhitespace(targetPath, "targetPath");
            builder.Add(BindingBuilderConstants.Target, target);
            builder.Add(BindingBuilderConstants.TargetPath, BindingPath.Create(targetPath));
            return builder.GetOrAddSyntaxBuilder();
        }

        #endregion

        #region To

        public static IBindingModeInfoBehaviorSyntax ToSource([NotNull] this IBindingToSyntax syntax,
            [NotNull] Func<IDataContext, IBindingSource> bindingSourceDelegate)
        {
            Should.NotBeNull(syntax, "syntax");
            Should.NotBeNull(bindingSourceDelegate, "bindingSourceDelegate");
            syntax.Builder.GetOrAddBindingSources().Add(bindingSourceDelegate);
            return syntax.Builder.GetOrAddSyntaxBuilder();
        }

        public static IBindingModeInfoBehaviorSyntax To([NotNull] this IBindingToSyntax syntax, [NotNull] object source,
            [NotNull] string sourcePath)
        {
            Should.NotBeNull(source, "source");
            Should.NotBeNull(sourcePath, "sourcePath");
            return syntax.ToSource(context =>
            {
                IObserver observer = BindingServiceProvider.ObserverProvider.Observe(source, BindingPath.Create(sourcePath), false);
                return new BindingSource(observer);
            });
        }

        public static IBindingModeInfoBehaviorSyntax To<TSource>([NotNull] this IBindingToSyntax syntax, TSource source,
            [NotNull] Expression<Func<TSource, object>> sourcePath)
        {
            Should.NotBeNull(sourcePath, "sourcePath");
            return syntax.To(source, BindingExtensions.GetMemberPath(sourcePath));
        }

        public static IBindingModeInfoBehaviorSyntax To([NotNull] this IBindingToSyntax syntax,
            [NotNull] string sourcePath)
        {
            Should.NotBeNull(sourcePath, "sourcePath");
            return syntax.ToSource(context =>
            {
                IBindingContext bindingContext = BindingServiceProvider
                    .ContextManager
                    .GetBindingContext(context.GetData(BindingBuilderConstants.Target, true),
                        context.GetData(BindingBuilderConstants.TargetPath, true).Path);
                IObserver observer = BindingServiceProvider.ObserverProvider.Observe(bindingContext, BindingPath.Create(sourcePath),
                    false);
                return new BindingSource(observer);
            });
        }

        public static IBindingModeInfoBehaviorSyntax To<TSource>([NotNull] this IBindingToSyntax syntax,
            [NotNull] Expression<Func<TSource, object>> sourcePath)
        {
            Should.NotBeNull(sourcePath, "sourcePath");
            return syntax.To(BindingExtensions.GetMemberPath(sourcePath));
        }

        public static IBindingModeInfoBehaviorSyntax To<TTarget, TSource>(
            [NotNull] this IBindingToSyntax<TTarget, TSource> syntax,
            [NotNull] Expression<Func<TSource, object>> sourcePath)
        {
            Should.NotBeNull(sourcePath, "sourcePath");
            return syntax.To(BindingExtensions.GetMemberPath(sourcePath));
        }

        public static IBindingModeInfoBehaviorSyntax ToSelf([NotNull] this IBindingToSyntax syntax,
            [NotNull] string selfPath)
        {
            Should.NotBeNull(selfPath, "selfPath");
            return syntax.ToSource(context =>
            {
                object target = context.GetData(BindingBuilderConstants.Target, true);
                return new BindingSource(BindingServiceProvider.ObserverProvider.Observe(target, BindingPath.Create(selfPath), false));
            });
        }

        public static IBindingModeInfoBehaviorSyntax ToSelf<TTarget>(
            [NotNull] this IBindingToSyntax<TTarget> syntaxGeneric,
            [NotNull] Expression<Func<TTarget, object>> selfPath)
        {
            Should.NotBeNull(selfPath, "selfPath");
            return ToSelf(syntaxGeneric, BindingExtensions.GetMemberPath(selfPath));
        }

        public static IBindingModeInfoBehaviorSyntax ToExpression<T>(this T syntax,
            [NotNull] Func<IDataContext, IList<object>, object> multiExpression,
            [NotNull] params Func<T, IBindingModeInfoBehaviorSyntax>[] sources)
            where T : IBindingToSyntax
        {
            Should.NotBeNull(syntax, "syntax");
            Should.NotBeNull(multiExpression, "multiExpression");
            Should.NotBeNullOrEmpty(sources, "sources");
            for (int index = 0; index < sources.Length; index++)
                sources[index].Invoke(syntax);
            syntax.Builder.Add(BindingBuilderConstants.MultiExpression, multiExpression);
            return syntax.Builder.GetOrAddSyntaxBuilder();
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
            return syntax.Builder.GetOrAddSyntaxBuilder();
        }

        public static IBindingInfoBehaviorSyntax WithCommandParameter([NotNull] this IBindingInfoSyntax syntax,
            [NotNull] Func<IDataContext, object> getParameter)
        {
            Should.NotBeNull(syntax, "syntax");
            Should.NotBeNull(getParameter, "getParameter");
            syntax.Builder.Add(BindingBuilderConstants.CommandParameter, getParameter);
            return syntax.Builder.GetOrAddSyntaxBuilder();
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
            return syntax.Builder.GetOrAddSyntaxBuilder();
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
            return syntax.Builder.GetOrAddSyntaxBuilder();
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
            return syntax.Builder.GetOrAddSyntaxBuilder();
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
            return syntax.Builder.GetOrAddSyntaxBuilder();
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
            return syntax.Builder.GetOrAddSyntaxBuilder();
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

        public static IBindingBehaviorSyntax WithDelay([NotNull] this IBindingBehaviorSyntax syntax, uint delay)
        {
            return syntax.WithBehavior(new DelayBindingBehavior(delay));
        }

        public static IBindingBehaviorSyntax DefaultValueOnException([NotNull] this IBindingBehaviorSyntax syntax)
        {
            return syntax.WithBehavior(DefaultValueOnExceptionBehavior.Instance);
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

        private static IBindingInfoBehaviorSyntax WithBehaviorInternal(this IBuilderSyntax syntax,
            IBindingBehavior behavior)
        {
            Should.NotBeNull(syntax, "syntax");
            Should.NotBeNull(behavior, "behavior");
            IList<IBindingBehavior> behaviors = syntax.Builder.GetOrAddBehaviors();
            behaviors.Add(behavior);
            return syntax.Builder.GetOrAddSyntaxBuilder();
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

        private static SyntaxBuilder<object, object> GetOrAddSyntaxBuilder(this IBindingBuilder builder)
        {
            SyntaxBuilder<object, object> syntaxBuilder;
            if (!builder.TryGetData(SyntaxBuilderConstant, out syntaxBuilder))
            {
                syntaxBuilder = new SyntaxBuilder<object, object>(builder);
                builder.Add(SyntaxBuilderConstant, syntaxBuilder);
            }
            return syntaxBuilder;
        }

        #endregion

        #endregion
    }
}