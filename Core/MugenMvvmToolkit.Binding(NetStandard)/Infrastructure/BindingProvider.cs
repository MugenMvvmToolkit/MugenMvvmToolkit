#region Copyright

// ****************************************************************************
// <copyright file="BindingProvider.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Accessors;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Parse;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class BindingProvider : IBindingProvider
    {
        #region Fields

        private static readonly DataConstant<BindingProvider> ProviderConstant;
        private static readonly DataConstant<Exception> ExceptionConstant;
        private static readonly IComparer<IBindingBehavior> BehaviorComparer;
        private static readonly Func<IDataContext, IDataBinding> CreateInvalidaDataBindingDelegate;
        private static readonly Func<IDataContext, IList<object>, object> FormatMembersExpressionDelegate;

        private readonly OrderedListInternal<IBindingBehavior> _defaultBehaviors;
        private readonly Func<IDataContext, IDataBinding> _buildDelegate;

        private IBindingParser _parser;

        #endregion

        #region Constructors

        static BindingProvider()
        {
            CreateInvalidaDataBindingDelegate = CreateInvalidaDataBinding;
            FormatMembersExpressionDelegate = FormatMembersExpression;
            var type = typeof(BindingProvider);
            ProviderConstant = DataConstant.Create<BindingProvider>(type, nameof(ProviderConstant), true);
            ExceptionConstant = DataConstant.Create<Exception>(type, nameof(ExceptionConstant), true);
            BehaviorComparer = new DelegateComparer<IBindingBehavior>((behavior, bindingBehavior) => bindingBehavior.Priority.CompareTo(behavior.Priority));
        }

        public BindingProvider()
            : this(new BindingParser(), new IBindingBehavior[] { new OneWayBindingMode() })
        {
        }

        public BindingProvider([NotNull] IBindingParser parser, IEnumerable<IBindingBehavior> defaultBehaviors)
        {
            Should.NotBeNull(parser, nameof(parser));
            _parser = parser;
            _defaultBehaviors = new OrderedListInternal<IBindingBehavior>(defaultBehaviors ?? Empty.Array<IBindingBehavior>(), BehaviorComparer);
            _buildDelegate = BuildBinding;
        }

        #endregion

        #region Implementation of IBindingProvider

        public ICollection<IBindingBehavior> DefaultBehaviors => _defaultBehaviors;

        public IBindingParser Parser
        {
            get { return _parser; }
            set
            {
                Should.PropertyNotBeNull(value);
                _parser = value;
            }
        }

        public IBindingBuilder CreateBuilder(IDataContext context = null)
        {
            return CreateBuilderInternal(context.ToNonReadOnly());
        }

        public IDataBinding CreateBinding(IDataContext context)
        {
            try
            {
                return CreateBuilder(context).Build();
            }
            catch (Exception exception)
            {
                return CreateInvalidDataBinding(exception);
            }
        }

        public IList<IBindingBuilder> CreateBuildersFromString(object target, string bindingExpression, IList<object> sources = null, IDataContext context = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(bindingExpression, nameof(bindingExpression));
            try
            {
                var parserResult = Parser.Parse(target, bindingExpression, sources, context);
                var result = new IBindingBuilder[parserResult.Count];
                for (int index = 0; index < parserResult.Count; index++)
                {
                    var builder = new BindingBuilder(parserResult[index]);
                    builder.Add(BindingBuilderConstants.BuildDelegate, _buildDelegate);
                    result[index] = builder;
                }
                return result;
            }
            catch (Exception exception)
            {
                exception = new InvalidOperationException(exception.Message, exception);
                var builder = new BindingBuilder();
                builder.Add(BindingBuilderConstants.Target, target);
                builder.Add(ProviderConstant, this);
                builder.Add(ExceptionConstant, exception);
                builder.Add(BindingBuilderConstants.BuildDelegate, CreateInvalidaDataBindingDelegate);
                return new IBindingBuilder[] { builder };
            }
        }

        public void CreateBindingsFromString(object target, string bindingExpression, IList<object> sources, IDataContext context)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(bindingExpression, nameof(bindingExpression));
            try
            {
                IList<IDataContext> parserResult = Parser.Parse(target, bindingExpression, sources, context);
                for (int index = 0; index < parserResult.Count; index++)
                    BuildBinding(parserResult[index]);
            }
            catch (Exception e)
            {
                CreateInvalidDataBinding(e);
            }
        }

        public IList<IDataBinding> CreateBindingsFromStringWithBindings(object target, string bindingExpression, IList<object> sources = null, IDataContext context = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(bindingExpression, nameof(bindingExpression));
            try
            {
                IList<IDataContext> parserResult = Parser.Parse(target, bindingExpression, sources, context);
                var result = new IDataBinding[parserResult.Count];
                for (int index = 0; index < parserResult.Count; index++)
                    result[index] = BuildBinding(parserResult[index]);
                return result;
            }
            catch (Exception exception)
            {
                return new[]
                {
                    CreateInvalidDataBinding(exception)
                };
            }
        }

        public void BuildFromLambdaExpression(IBindingBuilder builder, Func<LambdaExpression> expression)
        {
            Should.NotBeNull(builder, nameof(builder));
            Should.NotBeNull(expression, nameof(expression));
            BuildFromLambdaExpressionInternal(builder, expression);
        }

        public void BuildParameterFromLambdaExpression<TValue>(IBindingBuilder builder, Func<LambdaExpression> expression,
            DataConstant<Func<IDataContext, TValue>> parameterConstant)
        {
            Should.NotBeNull(builder, nameof(builder));
            Should.NotBeNull(expression, nameof(expression));
            Should.NotBeNull(parameterConstant, nameof(parameterConstant));
            BuildParameterFromLambdaExpressionInternal(builder, expression, parameterConstant);
        }

        public event Action<IBindingProvider, IDataContext> BindingInitializing;

        public event Action<IBindingProvider, IDataBinding> BindingInitialized;

        #endregion

        #region Methods

        protected virtual void BuildFromLambdaExpressionInternal(IBindingBuilder builder, Func<LambdaExpression> expression)
        {
            LambdaExpressionToBindingExpressionConverter.Convert(builder, expression);
        }

        protected virtual void BuildParameterFromLambdaExpressionInternal<TValue>(IBindingBuilder builder, Func<LambdaExpression> expression, DataConstant<Func<IDataContext, TValue>> parameterConstant)
        {
            Func<IDataContext, TValue> value;
            var func = LambdaExpressionToBindingExpressionConverter.ConvertParameter(builder, expression);
            if (typeof(TValue) == typeof(object))
                value = (Func<IDataContext, TValue>)(object)func;
            else
                value = func.CastFunc<TValue>;
            builder.Add(parameterConstant, value);
        }

        [NotNull]
        protected virtual IBindingBuilder CreateBuilderInternal([NotNull] IDataContext context)
        {
            if (!context.Contains(BindingBuilderConstants.BuildDelegate))
                context.Add(BindingBuilderConstants.BuildDelegate, _buildDelegate);
            return new BindingBuilder(context);
        }

        [NotNull]
        protected virtual IDataBinding BuildBinding([NotNull] IDataContext context)
        {
            try
            {
                RaiseInitializing(context);
                object target;
                IBindingPath targetPath;
                IDataBinding binding = CreateBinding(context, out target, out targetPath);
                if (!binding.IsDisposed)
                    BindingServiceProvider.BindingManager.Register(target, targetPath.Path, binding, context);
                RaiseInitialized(binding);
                return binding;
            }
            catch (Exception exception)
            {
                return CreateInvalidDataBinding(new InvalidOperationException(exception.Message, exception));
            }
        }

        [NotNull]
        protected virtual IDataBinding CreateBinding([NotNull] IDataContext context, out object target, out IBindingPath targetPath)
        {
            IBindingSourceAccessor sourceAccessor;
            var formatExpression = context.GetData(BindingBuilderConstants.MultiExpression);
            var sourceDelegates = context.GetData(BindingBuilderConstants.Sources, true);
            if (sourceDelegates.Count > 1 || formatExpression != null)
            {
                formatExpression = formatExpression ?? FormatMembersExpressionDelegate;
                var sources = new IObserver[sourceDelegates.Count];
                for (int index = 0; index < sourceDelegates.Count; index++)
                    sources[index] = sourceDelegates[index].Invoke(context);
                sourceAccessor = new MultiBindingSourceAccessor(sources, formatExpression, context);
            }
            else
                sourceAccessor = new BindingSourceAccessor(sourceDelegates[0].Invoke(context), context, false);
            var binding = new DataBinding(new BindingSourceAccessor(GetBindingTarget(context, out target, out targetPath), context, true), sourceAccessor);
            object source;
            if (context.TryGetData(BindingBuilderConstants.Source, out source))
                binding.Context.AddOrUpdate(BindingConstants.Source, ToolkitExtensions.GetWeakReference(source));
            AddBehaviors(binding, context);
            return binding;
        }

        [NotNull]
        protected virtual IObserver GetBindingTarget([NotNull] IDataContext context, out object target, out IBindingPath targetPath)
        {
            target = context.GetData(BindingBuilderConstants.Target, true);
            targetPath = context.GetData(BindingBuilderConstants.TargetPath, true);
            var src = context.GetData(BindingBuilderConstants.TargetSource);
            return BindingServiceProvider.ObserverProvider.Observe(src ?? target, targetPath, false, context);
        }

        protected virtual void AddBehaviors(IDataBinding binding, IDataContext context)
        {
            List<IBindingBehavior> behaviors = context.GetData(BindingBuilderConstants.Behaviors);
            if (behaviors == null || behaviors.Count == 0)
            {
                for (int index = 0; index < _defaultBehaviors.Count; index++)
                    binding.Behaviors.Add(_defaultBehaviors[index].Clone());
                return;
            }

            int count = behaviors.Count;
            for (int index = 0; index < _defaultBehaviors.Count; index++)
            {
                IBindingBehavior behavior = _defaultBehaviors[index];
                bool hasBehavior = false;
                for (int i = 0; i < count; i++)
                {
                    if (behaviors[i].Id == behavior.Id)
                    {
                        hasBehavior = true;
                        break;
                    }
                }
                if (!hasBehavior)
                    behaviors.Add(behavior.Clone());
            }
            behaviors.Sort(BehaviorComparer);
            for (int index = 0; index < behaviors.Count; index++)
                binding.Behaviors.Add(behaviors[index]);
        }

        protected void RaiseInitializing(IDataContext context)
        {
            BindingInitializing?.Invoke(this, context);
        }

        protected void RaiseInitialized(IDataBinding binding)
        {
            BindingInitialized?.Invoke(this, binding);
        }

        private static IDataBinding CreateInvalidaDataBinding(IDataContext dataContext)
        {
            var exception = dataContext.GetData(ExceptionConstant);
            if (exception == null)
                exception = BindingExceptionManager.InvalidBindingMember(typeof(object), "undefined");
            var b = new InvalidDataBinding(exception);
            dataContext.GetData(ProviderConstant)?.AddBehaviors(b, dataContext);
            return b;
        }

        private IDataBinding CreateInvalidDataBinding(Exception exception)
        {
            var b = new InvalidDataBinding(exception);
            AddBehaviors(b, DataContext.Empty);
            return b;
        }

        private static object FormatMembersExpression(IDataContext context, IList<object> objects)
        {
            var builder = new StringBuilder();
            for (int index = 0; index < objects.Count; index++)
            {
                object o = objects[index];
                if (o != null)
                    builder.Append(o);
            }
            return builder.ToString();
        }

        #endregion
    }
}
