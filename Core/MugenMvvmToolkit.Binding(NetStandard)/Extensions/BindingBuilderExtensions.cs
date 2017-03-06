#region Copyright

// ****************************************************************************
// <copyright file="BindingBuilderExtensions.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Interfaces.Syntax;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.Binding
{
    public static class BindingBuilderExtensions
    {
        #region Fields

        private static readonly DataConstant<object> SyntaxBuilderConstant;
        private static readonly BindingMemberDescriptor<object, string> DefautBindingMemberDescriptor;

        #endregion

        #region Constructors

        static BindingBuilderExtensions()
        {
            SyntaxBuilderConstant = DataConstant.Create<object>(typeof(BindingBuilderExtensions), nameof(SyntaxBuilderConstant), true);
            DefautBindingMemberDescriptor = new BindingMemberDescriptor<object, string>("DefautBindingMember");
        }

        #endregion

        #region Methods

        #region Clear

        public static void ClearBindings<T>([CanBeNull] this T item, bool clearDataContext, bool clearAttachedValues)
            where T : class
        {
            if (item == null)
                return;
            try
            {
                BindingServiceProvider.BindingManager.ClearBindings(item);
                if (clearDataContext && BindingServiceProvider.ContextManager.HasBindingContext(item))
                    BindingServiceProvider.ContextManager.GetBindingContext(item).Value = null;
                if (clearAttachedValues)
                    ServiceProvider.AttachedValueProvider.Clear(item);
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
            }
        }

        #endregion

        #region Bind

        public static void BindFromExpression<TTarget>([NotNull]this TTarget target, string expression, IList<object> sources = null, IDataContext context = null) where TTarget : class
        {
            BindingServiceProvider.BindingProvider.CreateBindingsFromString(target, expression, sources, context);
        }

        public static void BindFromExpression<TTarget>([NotNull]this TTarget target, string targetPath, string expression, IDataContext context = null) where TTarget : class
        {
            BindingServiceProvider.BindingProvider.CreateBindingsFromString(target, targetPath + " " + expression, context: context);
        }

        public static void BindFromExpression<TTarget>([NotNull]this TTarget target, string targetPath, object source, string expression, IDataContext context = null) where TTarget : class
        {
            BindingServiceProvider.BindingProvider.CreateBindingsFromString(target, targetPath + " " + expression, new[] { source }, context);
        }

        public static IBindingToSyntax<TTarget> Bind<TTarget>([NotNull]this TTarget targetGeneric) where TTarget : class
        {
            return targetGeneric.Bind(targetGeneric.GetBindingMemberValue(DefautBindingMemberDescriptor));
        }

        public static IBindingToSyntax<TTarget> Bind<TTarget>([NotNull]this TTarget targetGeneric, [NotNull] string targetPath) where TTarget : class
        {
            return BindingServiceProvider.BindingProvider.CreateBuilder().Bind(targetGeneric, targetPath);
        }

        public static IBindingToSyntax<TTarget> Bind<TTarget>([NotNull]this TTarget targetGeneric, [NotNull] Func<Expression<Func<TTarget, object>>> targetPath) where TTarget : class
        {
            return BindingServiceProvider.BindingProvider.CreateBuilder().Bind(targetGeneric, targetPath);
        }

        public static IBindingToSyntax<TTarget, TSource> Bind<TTarget, TSource>([NotNull] this IBindingBuilder builder, [NotNull] TTarget targetGeneric) where TTarget : class
        {
            return builder.Bind<TTarget, TSource>(targetGeneric, targetGeneric.GetBindingMemberValue(DefautBindingMemberDescriptor));
        }

        public static IBindingToSyntax<TTarget, TSource> Bind<TTarget, TSource>([NotNull] this IBindingBuilder builder,
            [NotNull] TTarget targetGeneric, [NotNull] string targetPath) where TTarget : class
        {
            Should.NotBeNull(builder, nameof(builder));
            Should.NotBeNull(targetGeneric, nameof(targetGeneric));
            Should.NotBeNullOrWhitespace(targetPath, nameof(targetPath));
            builder.Add(BindingBuilderConstants.Target, targetGeneric);
            builder.Add(BindingBuilderConstants.TargetPath, BindingServiceProvider.BindingPathFactory(targetPath));
            return new SyntaxBuilder<TTarget, TSource>(builder);
        }

        public static IBindingToSyntax<TTarget> Bind<TTarget>([NotNull] this IBindingBuilder builder, [NotNull] TTarget targetGeneric) where TTarget : class
        {
            return builder.Bind<TTarget, object>(targetGeneric);
        }

        public static IBindingToSyntax<TTarget> Bind<TTarget>([NotNull] this IBindingBuilder builder,
            [NotNull] TTarget targetGeneric, [NotNull] string targetPath) where TTarget : class
        {
            return builder.Bind<TTarget, object>(targetGeneric, targetPath);
        }

        public static IBindingToSyntax<TTarget, TSource> Bind<TTarget, TSource>([NotNull] this IBindingBuilder builder,
            [NotNull] TTarget target,
            [NotNull] Func<Expression<Func<TTarget, object>>> targetPath) where TTarget : class
        {
            return builder.Bind<TTarget, TSource>(target, BindingExtensions.GetMemberPath(targetPath));
        }

        public static IBindingToSyntax<TTarget> Bind<TTarget>([NotNull] this IBindingBuilder builder,
            [NotNull] TTarget target, [NotNull] Func<Expression<Func<TTarget, object>>> targetPath)
            where TTarget : class
        {
            return builder.Bind<TTarget, object>(target, targetPath);
        }

        #endregion

        #region To

        public static IBindingModeInfoBehaviorSyntax<object> ToSource([NotNull] this IBindingToSyntax syntax,
            [NotNull] Func<IDataContext, IObserver> bindingSourceDelegate)
        {
            Should.NotBeNull(syntax, nameof(syntax));
            Should.NotBeNull(bindingSourceDelegate, nameof(bindingSourceDelegate));
            syntax.Builder.GetOrAddBindingSources().Add(bindingSourceDelegate);
            return syntax.GetOrAddSyntaxBuilder<IBindingModeInfoBehaviorSyntax<object>, object, object>();
        }

        public static IBindingModeInfoBehaviorSyntax<TSource> To<TSource>([NotNull] this IBindingToSyntax syntax,
            [NotNull] TSource source, [NotNull] string sourcePath)
        {
            Should.NotBeNull(source, nameof(source));
            Should.NotBeNull(sourcePath, nameof(sourcePath));
            syntax.Builder.AddOrUpdate(BindingBuilderConstants.Source, source);
            syntax.Builder.GetOrAddBindingSources().Add(context => BindingExtensions.CreateBindingSource(context, sourcePath, source));
            return syntax.GetOrAddSyntaxBuilder<IBindingModeInfoBehaviorSyntax<TSource>, object, TSource>();
        }

        public static IBindingModeInfoBehaviorSyntax<object> To([NotNull] this IBindingToSyntax syntax,
            [NotNull] string sourcePath)
        {
            Should.NotBeNull(sourcePath, nameof(sourcePath));
            return syntax.ToSource(context => BindingExtensions.CreateBindingSource(context, sourcePath, null));
        }

        public static IBindingModeInfoBehaviorSyntax<TSource> To<TSource>([NotNull] this IBindingToSyntax syntax,
            TSource source, [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<object, TSource>, object>>> expression)
        {
            return syntax.ToInternal(expression, source);
        }

        public static IBindingModeInfoBehaviorSyntax<TSource> To<TSource>([NotNull] this IBindingToSyntax syntax,
            [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<object, TSource>, object>>> expression)
        {
            return syntax.ToInternal<TSource>(expression);
        }

        public static IBindingModeInfoBehaviorSyntax<TSource> To<TTarget, TSource>(
            [NotNull] this IBindingToSyntax<TTarget, TSource> syntax,
            [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<TTarget, TSource>, object>>> expression)
            where TTarget : class
        {
            return syntax.ToInternal<TSource>(expression);
        }

        public static IBindingModeInfoBehaviorSyntax<TSource> To<TTarget, TSource>(
            [NotNull] this IBindingToSyntax<TTarget> syntax, TSource source,
            [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<TTarget, TSource>, object>>> expression)
            where TTarget : class
        {
            return syntax.ToInternal(expression, source);
        }

        public static IBindingModeInfoBehaviorSyntax<TSource> ToAction<TSource>([NotNull] this IBindingToSyntax syntax,
            TSource source, [NotNull] Func<Expression<Action<TSource, IBindingSyntaxContext<object, TSource>>>> expression)
        {
            return syntax.ToInternal(expression, source);
        }

        public static IBindingModeInfoBehaviorSyntax<TSource> ToAction<TSource>([NotNull] this IBindingToSyntax syntax,
            [NotNull] Func<Expression<Action<TSource, IBindingSyntaxContext<object, TSource>>>> expression)
        {
            return syntax.ToInternal<TSource>(expression);
        }

        public static IBindingModeInfoBehaviorSyntax<TSource> ToAction<TTarget, TSource>(
            [NotNull] this IBindingToSyntax<TTarget, TSource> syntax,
            [NotNull] Func<Expression<Action<TSource, IBindingSyntaxContext<TTarget, TSource>>>> expression)
            where TTarget : class
        {
            return syntax.ToInternal<TSource>(expression);
        }

        public static IBindingModeInfoBehaviorSyntax<TSource> ToAction<TTarget, TSource>(
            [NotNull] this IBindingToSyntax<TTarget> syntax, TSource source,
            [NotNull] Func<Expression<Action<TSource, IBindingSyntaxContext<TTarget, TSource>>>> expression)
            where TTarget : class
        {
            return syntax.ToInternal(expression, source);
        }

        public static IBindingModeInfoBehaviorSyntax<TTarget> ToSelf<TTarget>([NotNull] this IBindingToSyntax<TTarget> syntax,
            [NotNull] string selfPath)
            where TTarget : class
        {
            Should.NotBeNull(syntax, nameof(syntax));
            return syntax.To((TTarget)syntax.Builder.GetData(BindingBuilderConstants.Target, true), selfPath);
        }

        public static IBindingModeInfoBehaviorSyntax<TTarget> ToSelf<TTarget>(
            [NotNull] this IBindingToSyntax<TTarget> syntax,
            [NotNull] Func<Expression<Func<TTarget, IBindingSyntaxContext<TTarget, TTarget>, object>>> expression)
            where TTarget : class
        {
            Should.NotBeNull(syntax, nameof(syntax));
            return syntax.ToInternal(expression, (TTarget)syntax.Builder.GetData(BindingBuilderConstants.Target, true));
        }

        #endregion

        #region Mode

        public static IBindingInfoBehaviorSyntax<TSource> TwoWay<TSource>(
            [NotNull] this IBindingModeSyntax<TSource> syntax)
        {
            return syntax.WithBehaviorInternal<TSource>(BindingServiceProvider.BindingModeToBehavior["TwoWay"].Clone());
        }

        public static IBindingInfoBehaviorSyntax<TSource> OneWay<TSource>([NotNull] this IBindingModeSyntax<TSource> syntax)
        {
            return syntax.WithBehaviorInternal<TSource>(BindingServiceProvider.BindingModeToBehavior["OneWay"].Clone());
        }

        public static IBindingInfoBehaviorSyntax<TSource> OneWayToSource<TSource>(
            [NotNull] this IBindingModeSyntax<TSource> syntax)
        {
            return syntax.WithBehaviorInternal<TSource>(BindingServiceProvider.BindingModeToBehavior["OneWayToSource"].Clone());
        }

        public static IBindingInfoBehaviorSyntax<TSource> OneTime<TSource>(
            [NotNull] this IBindingModeSyntax<TSource> syntax)
        {
            return syntax.WithBehaviorInternal<TSource>(BindingServiceProvider.BindingModeToBehavior["OneTime"].Clone());
        }

        public static IBindingInfoBehaviorSyntax<TSource> NoneMode<TSource>(
            [NotNull] this IBindingModeSyntax<TSource> syntax)
        {
            return syntax.WithBehaviorInternal<TSource>(BindingServiceProvider.BindingModeToBehavior["None"].Clone());
        }

        #endregion

        #region Parameters

        public static IBindingInfoBehaviorSyntax<TSource> Observable<TSource>([NotNull] this IBindingInfoSyntax<TSource> syntax, bool value = true)
        {
            Should.NotBeNull(syntax, nameof(syntax));
            syntax.Builder.AddOrUpdate(BindingBuilderConstants.Observable, value);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax<TSource>, object, TSource>();
        }

        public static IBindingInfoBehaviorSyntax<TSource> Optional<TSource>([NotNull] this IBindingInfoSyntax<TSource> syntax, bool value = true)
        {
            Should.NotBeNull(syntax, nameof(syntax));
            syntax.Builder.AddOrUpdate(BindingBuilderConstants.Optional, value);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax<TSource>, object, TSource>();
        }

        public static IBindingInfoBehaviorSyntax<TSource> HasStablePath<TSource>([NotNull] this IBindingInfoSyntax<TSource> syntax, bool value = true)
        {
            Should.NotBeNull(syntax, nameof(syntax));
            syntax.Builder.AddOrUpdate(BindingBuilderConstants.HasStablePath, value);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax<TSource>, object, TSource>();
        }

        public static IBindingInfoBehaviorSyntax<TSource> DisableEqualityChecking<TSource>([NotNull] this IBindingInfoSyntax<TSource> syntax, bool targetValue = true, bool sourceValue = true)
        {
            Should.NotBeNull(syntax, nameof(syntax));
            var behaviors = syntax.Builder.GetOrAddBehaviors();
            behaviors.Add(DisableEqualityCheckingBehavior.GetTargetBehavior(targetValue));
            behaviors.Add(DisableEqualityCheckingBehavior.GetSourceBehavior(sourceValue));
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax<TSource>, object, TSource>();
        }

        public static IBindingInfoBehaviorSyntax<TSource> ToggleEnabledState<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax, bool value)
        {
            return syntax.WithParameter(BindingBuilderConstants.ToggleEnabledState, value);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithCommandParameter<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [NotNull] Func<IDataContext, object> getParameter)
        {
            return syntax.WithParameter(BindingBuilderConstants.CommandParameter, getParameter);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithCommandParameter<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [CanBeNull] object parameter)
        {
            return syntax.WithCommandParameter(d => parameter);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithCommandParameter<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<object, TSource>, object>>> expression)
        {
            return syntax.WithParameter<TSource, object>(BindingBuilderConstants.CommandParameter, expression);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithCommandParameter<TSource>(
            [NotNull] this IBindingInfoSyntax syntax,
            [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<object, TSource>, object>>> expression)
        {
            return syntax.WithParameter<TSource, object>(BindingBuilderConstants.CommandParameter, expression);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithConverter<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [NotNull] Func<IDataContext, IBindingValueConverter> getConverter)
        {
            return syntax.WithParameter(BindingBuilderConstants.Converter, getConverter);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithConverter<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [NotNull] IBindingValueConverter converter)
        {
            Should.NotBeNull(converter, nameof(converter));
            return syntax.WithConverter(d => converter);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithConverter<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<object, TSource>, object>>> expression)
        {
            return syntax.WithParameter<TSource, IBindingValueConverter>(BindingBuilderConstants.Converter, expression);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithConverter<TSource>(
            [NotNull] this IBindingInfoSyntax syntax,
            [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<object, TSource>, object>>> expression)
        {
            return syntax.WithParameter<TSource, IBindingValueConverter>(BindingBuilderConstants.Converter, expression);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithConverterParameter<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [NotNull] Func<IDataContext, object> getParameter)
        {
            return syntax.WithParameter(BindingBuilderConstants.ConverterParameter, getParameter);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithConverterParameter<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [CanBeNull] object parameter)
        {
            return syntax.WithConverterParameter(d => parameter);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithConverterParameter<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<object, TSource>, object>>> expression)
        {
            return syntax.WithParameter<TSource, object>(BindingBuilderConstants.ConverterParameter, expression);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithConverterParameter<TSource>(
            [NotNull] this IBindingInfoSyntax syntax, [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<object, TSource>, object>>> expression)
        {
            return syntax.WithParameter<TSource, object>(BindingBuilderConstants.ConverterParameter, expression);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithConverterCulture<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [NotNull] Func<IDataContext, CultureInfo> getCulture)
        {
            return syntax.WithParameter(BindingBuilderConstants.ConverterCulture, getCulture);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithConverterCulture<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [NotNull] CultureInfo culture)
        {
            Should.NotBeNull(culture, nameof(culture));
            return syntax.WithConverterCulture(d => culture);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithConverterCulture<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<object, TSource>, object>>> expression)
        {
            return syntax.WithParameter<TSource, CultureInfo>(BindingBuilderConstants.ConverterCulture, expression);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithConverterCulture<TSource>(
            [NotNull] this IBindingInfoSyntax syntax,
            [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<object, TSource>, object>>> expression)
        {
            return syntax.WithParameter<TSource, CultureInfo>(BindingBuilderConstants.ConverterCulture, expression);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithFallback<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [NotNull] Func<IDataContext, object> getFallback)
        {
            return syntax.WithParameter(BindingBuilderConstants.Fallback, getFallback);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithFallback<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [CanBeNull] object fallback)
        {
            return syntax.WithFallback(d => fallback);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithFallback<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<object, TSource>, object>>> expression)
        {
            return syntax.WithParameter<TSource, object>(BindingBuilderConstants.Fallback, expression);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithFallback<TSource>(
            [NotNull] this IBindingInfoSyntax syntax,
            [NotNull] Func<Expression<Func<TSource, IBindingSyntaxContext<object, TSource>, object>>> expression)
        {
            return syntax.WithParameter<TSource, object>(BindingBuilderConstants.Fallback, expression);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithTargetNullValue<TSource>(
            [NotNull] this IBindingInfoSyntax<TSource> syntax,
            [CanBeNull] object nullValue)
        {
            return syntax.WithParameter(BindingBuilderConstants.TargetNullValue, nullValue);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithBehavior<TSource>(
            [NotNull] this IBindingBehaviorSyntax<TSource> syntax,
            [NotNull] IBindingBehavior behavior)
        {
            return syntax.WithBehaviorInternal<TSource>(behavior);
        }

        public static IBindingInfoBehaviorSyntax<TSource> LostFocusUpdateSourceTrigger<TSource>(
            [NotNull] this IBindingBehaviorSyntax<TSource> syntax)
        {
            return syntax.WithBehavior(new LostFocusUpdateSourceBehavior());
        }

        public static IBindingInfoBehaviorSyntax<TSource> ValidatesOnNotifyDataErrors<TSource>(
            [NotNull] this IBindingBehaviorSyntax<TSource> syntax)
        {
            return syntax.WithBehavior(new ValidatesOnNotifyDataErrorsBehavior());
        }

        public static IBindingInfoBehaviorSyntax<TSource> ValidatesOnExceptions<TSource>(
            [NotNull] this IBindingBehaviorSyntax<TSource> syntax)
        {
            return syntax.WithBehavior(ValidatesOnExceptionsBehavior.Instance);
        }

        public static IBindingInfoBehaviorSyntax<TSource> Validate<TSource>(
            [NotNull] this IBindingBehaviorSyntax<TSource> syntax)
        {
            IList<IBindingBehavior> behaviors = syntax.Builder.GetOrAddBehaviors();
            behaviors.Add(new ValidatesOnNotifyDataErrorsBehavior());
            behaviors.Add(ValidatesOnExceptionsBehavior.Instance);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax<TSource>, object, TSource>();
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithDelay<TSource>(
            [NotNull] this IBindingBehaviorSyntax<TSource> syntax, uint delay, bool isTarget = false)
        {
            return syntax.WithBehaviorInternal<TSource>(new DelayBindingBehavior(delay, isTarget));
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithSourceDelay<TSource>([NotNull] this IBindingBehaviorSyntax<TSource> syntax, uint delay)
        {
            return syntax.WithDelay(delay, false);
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithTargetDelay<TSource>([NotNull] this IBindingBehaviorSyntax<TSource> syntax, uint delay)
        {
            return syntax.WithDelay(delay, true);
        }

        public static IBindingInfoBehaviorSyntax<TSource> DefaultValueOnException<TSource>([NotNull] this IBindingBehaviorSyntax<TSource> syntax, object value = null)
        {
            return syntax.WithBehaviorInternal<TSource>(new DefaultValueOnExceptionBehavior(value));
        }

        public static IBindingInfoBehaviorSyntax<TSource> WithDebugTag<TSource>([NotNull] this IBindingInfoSyntax<TSource> syntax, string tag)
        {
            return syntax.WithParameter(BindingBuilderConstants.DebugTag, tag);
        }

        public static IDataBinding Build(this IBindingInfoSyntax syntax)
        {
            Should.NotBeNull(syntax, nameof(syntax));
            return syntax.Builder.Build();
        }

        [CanBeNull]
        public static string TryGetDefaultBindingMember([NotNull] object instance)
        {
            Should.NotBeNull(instance, nameof(instance));
            string value;
            instance.TryGetBindingMemberValue(DefautBindingMemberDescriptor, out value);
            return value;
        }

        public static void RegisterDefaultBindingMember<TType>([NotNull] string member)
            where TType : class
        {
            Should.NotBeNull(member, nameof(member));
            BindingServiceProvider.MemberProvider.Register(AttachedBindingMember.CreateMember(DefautBindingMemberDescriptor.Override<TType>(), (info, type) => member, null));
        }

        public static void RegisterDefaultBindingMember<TType>([NotNull] Func<Expression<Func<TType, object>>> getMember)
            where TType : class
        {
            RegisterDefaultBindingMember<TType>(getMember.GetMemberName());
        }

        public static void RegisterDefaultBindingMember<TType, TValue>(BindingMemberDescriptor<TType, TValue> member)
            where TType : class
        {
            RegisterDefaultBindingMember<TType>(member.Path);
        }

        #endregion

        #region Internal

        internal static IList<IBindingBehavior> GetOrAddBehaviors(this IDataContext syntax)
        {
            Should.NotBeNull(syntax, nameof(syntax));
            List<IBindingBehavior> data;
            if (!syntax.TryGetData(BindingBuilderConstants.Behaviors, out data))
            {
                data = new List<IBindingBehavior>(2);
                syntax.Add(BindingBuilderConstants.Behaviors, data);
            }
            return data;
        }

        internal static IList<Func<IDataContext, IObserver>> GetOrAddBindingSources([NotNull] this IDataContext syntax)
        {
            Should.NotBeNull(syntax, nameof(syntax));
            IList<Func<IDataContext, IObserver>> delegates = syntax.GetData(BindingBuilderConstants.Sources);
            if (delegates == null)
            {
                delegates = new List<Func<IDataContext, IObserver>>(1);
                syntax.Add(BindingBuilderConstants.Sources, delegates);
            }
            return delegates;
        }

        private static TResult GetOrAddSyntaxBuilder<TResult, T1, T2>(this IBuilderSyntax bindingSyntax)
            where T1 : class
            where TResult : IBindingInfoSyntax<T2>
        {
            if (bindingSyntax is TResult)
                return (TResult)bindingSyntax;
            IBindingBuilder builder = bindingSyntax.Builder;
            object syntaxBuilder;
            if (!builder.TryGetData(SyntaxBuilderConstant, out syntaxBuilder) || !(syntaxBuilder is TResult))
            {
                syntaxBuilder = new SyntaxBuilder<T1, T2>(builder);
                builder.AddOrUpdate(SyntaxBuilderConstant, syntaxBuilder);
            }
            return (TResult)syntaxBuilder;
        }

        private static IBindingModeInfoBehaviorSyntax<TSource> ToInternal<TSource>(this IBindingToSyntax syntax,
            Func<LambdaExpression> expression)
        {
            BindingServiceProvider.BindingProvider.BuildFromLambdaExpression(syntax.Builder, expression);
            return syntax.GetOrAddSyntaxBuilder<IBindingModeInfoBehaviorSyntax<TSource>, object, TSource>();
        }

        private static IBindingModeInfoBehaviorSyntax<TSource> ToInternal<TSource>(this IBindingToSyntax syntax,
            Func<LambdaExpression> expression, TSource source)
        {
            Should.NotBeNull(syntax, nameof(syntax));
            syntax.Builder.AddOrUpdate(BindingBuilderConstants.Source, source);
            BindingServiceProvider.BindingProvider.BuildFromLambdaExpression(syntax.Builder, expression);
            return syntax.GetOrAddSyntaxBuilder<IBindingModeInfoBehaviorSyntax<TSource>, object, TSource>();
        }

        private static IBindingInfoBehaviorSyntax<TSource> WithBehaviorInternal<TSource>(this IBuilderSyntax syntax,
            IBindingBehavior behavior)
        {
            Should.NotBeNull(syntax, nameof(syntax));
            Should.NotBeNull(behavior, nameof(behavior));
            syntax.Builder.GetOrAddBehaviors().Add(behavior);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax<TSource>, object, TSource>();
        }

        private static IBindingInfoBehaviorSyntax<TSource> WithParameter<TSource, TValue>(this IBuilderSyntax syntax,
            DataConstant<Func<IDataContext, TValue>> constant, Func<LambdaExpression> expression)
        {
            Should.NotBeNull(syntax, nameof(syntax));
            BindingServiceProvider.BindingProvider.BuildParameterFromLambdaExpression(syntax.Builder, expression, constant);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax<TSource>, object, TSource>();
        }

        private static IBindingInfoBehaviorSyntax<TSource> WithParameter<TSource, TValue>(this IBindingInfoSyntax<TSource> syntax, DataConstant<TValue> constant, TValue value)
        {
            Should.NotBeNull(syntax, nameof(syntax));
            syntax.Builder.Add(constant, value);
            return syntax.GetOrAddSyntaxBuilder<IBindingInfoBehaviorSyntax<TSource>, object, TSource>();
        }

        internal static object AsBindingExpressionWithContext<TType>(this Func<object[], object> func, IDataContext context, TType list) where TType : IList<object>
        {
            return func.Invoke((object[])(object)list);
        }

        internal static object AsBindingExpression<TType>(this Func<object[], object> func, IDataContext context,
            TType list) where TType : IList<object>
        {
            var args = new object[list.Count + 1];
            args[0] = context;
            for (int i = 0; i < list.Count; i++)
                args[i + 1] = list[i];
            return func.Invoke(args);
        }

        #endregion

        #endregion
    }
}
