#region Copyright

// ****************************************************************************
// <copyright file="BindingProvider.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Parse;
using MugenMvvmToolkit.Binding.Sources;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the binding provider that allows to create the data bindings.
    /// </summary>
    public class BindingProvider : IBindingProvider
    {
        #region Fields

        private static readonly DataConstant<BindingProvider> ProviderConstant;
        private static readonly DataConstant<Exception> ExceptionConstant;
        private static readonly IComparer<IBindingBehavior> BehaviorComparer;
        private static readonly Func<IDataContext, IDataBinding> CreateInvalidaDataBindingDelegate;
        private static readonly Func<IDataContext, IList<object>, object> FormatMembersExpressionDelegate;

        private readonly OrderedListInternal<IBindingBehavior> _defaultBehaviors;
        private readonly IList<IBindingSourceDecorator> _decorators;
        private readonly Func<IDataContext, IDataBinding> _buildDelegate;

        private IBindingParser _parser;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingProvider" /> class.
        /// </summary>
        static BindingProvider()
        {
            CreateInvalidaDataBindingDelegate = CreateInvalidaDataBinding;
            FormatMembersExpressionDelegate = FormatMembersExpression;
            ProviderConstant = DataConstant.Create(() => ProviderConstant, true);
            ExceptionConstant = DataConstant.Create(() => ExceptionConstant, true);
            BehaviorComparer = new DelegateComparer<IBindingBehavior>((behavior, bindingBehavior) => bindingBehavior.Priority.CompareTo(behavior.Priority));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingProvider" /> class.
        /// </summary>
        public BindingProvider(IBindingParser parser = null)
        {
            _parser = parser ?? new BindingParser();
            var comparer = new DelegateComparer<IBindingSourceDecorator>((manager, targetManager) => targetManager.Priority.CompareTo(manager.Priority));
            _decorators = new OrderedListInternal<IBindingSourceDecorator>(comparer);
            _defaultBehaviors = new OrderedListInternal<IBindingBehavior>(BehaviorComparer)
            {
                new OneWayBindingMode()
            };
            _buildDelegate = BuildBinding;
        }

        #endregion

        #region Implementation of IBindingProvider

        /// <summary>
        ///     Gets the default behaviors.
        /// </summary>
        public ICollection<IBindingBehavior> DefaultBehaviors
        {
            get { return _defaultBehaviors; }
        }

        /// <summary>
        ///     Gets the collection of <see cref="IBindingSourceDecorator" />.
        /// </summary>
        public ICollection<IBindingSourceDecorator> SourceDecorators
        {
            get { return _decorators; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="IBindingParser" />.
        /// </summary>
        public IBindingParser Parser
        {
            get { return _parser; }
            set
            {
                Should.PropertyNotBeNull(value);
                _parser = value;
            }
        }

        /// <summary>
        ///     Occurs when binding starts initialization.
        /// </summary>
        public event Action<IBindingProvider, IDataContext> BindingInitializing;

        /// <summary>
        ///     Occurs when binding ends initialization.
        /// </summary>
        public event Action<IBindingProvider, IDataBinding> BindingInitialized;

        /// <summary>
        ///     Creates an instance of <see cref="IBindingBuilder" />.
        /// </summary>
        /// <param name="context">The specified context.</param>
        /// <returns>An instance of <see cref="IBindingBuilder" />.</returns>
        public IBindingBuilder CreateBuilder(IDataContext context = null)
        {
            context = context.ToNonReadOnly();
            if (!context.Contains(BindingBuilderConstants.BuildDelegate))
                context.Add(BindingBuilderConstants.BuildDelegate, _buildDelegate);
            return new BindingBuilder(context);
        }

        /// <summary>
        ///     Creates an instance of <see cref="IDataBinding" />.
        /// </summary>
        /// <param name="context">The specified context.</param>
        /// <returns>An instance of <see cref="IDataBinding" />.</returns>
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

        /// <summary>
        ///     Creates a series of instances of <see cref="IBindingBuilder" />.
        /// </summary>
        /// <param name="target">The specified binding target.</param>
        /// <param name="bindingExpression">The specified binding expression.</param>
        /// <param name="sources">The specified sources, if any.</param>
        /// <returns>An instance of <see cref="IBindingBuilder" />.</returns>
        public IList<IBindingBuilder> CreateBuildersFromString(object target, string bindingExpression, IList<object> sources = null)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNullOrWhitespace(bindingExpression, "bindingExpression");
            try
            {
                var parserResult = Parser.Parse(bindingExpression, DataContext.Empty, target, sources);
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

        /// <summary>
        ///     Creates a series of instances of <see cref="IDataBinding" />.
        /// </summary>
        /// <param name="target">The specified binding target.</param>
        /// <param name="bindingExpression">The specified binding expression.</param>
        /// <param name="sources">The specified sources, if any.</param>
        /// <returns>An instance of <see cref="IDataBinding" />.</returns>
        public IList<IDataBinding> CreateBindingsFromString(object target, string bindingExpression, IList<object> sources = null)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNullOrWhitespace(bindingExpression, "bindingExpression");
            try
            {
                IList<IDataContext> parserResult = Parser.Parse(bindingExpression, DataContext.Empty, target, sources);
                var result = new IDataBinding[parserResult.Count];
                for (int index = 0; index < parserResult.Count; index++)
                    result[index] = BuildBinding(parserResult[index]);
                return result;
            }
            catch (Exception exception)
            {
                return new[]
                {
                    CreateInvalidDataBinding(new InvalidOperationException(exception.Message, exception))
                };
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Builds an instance of <see cref="IDataBinding" />.
        /// </summary>
        /// <returns>
        ///     The builded <see cref="IDataBinding" />.
        /// </returns>
        protected IDataBinding BuildBinding([NotNull] IDataContext context)
        {
            try
            {
                var handler = BindingInitializing;
                if (handler != null)
                    handler(this, context);

                object target;
                IBindingPath targetPath;
                IDataBinding binding = CreateBinding(context, out target, out targetPath);
                if (!binding.IsDisposed)
                    BindingServiceProvider.BindingManager.Register(target, targetPath.Path, binding, context);

                var initialized = BindingInitialized;
                if (initialized != null)
                    initialized(this, binding);
                return binding;
            }
            catch (Exception exception)
            {
                return CreateInvalidDataBinding(new InvalidOperationException(exception.Message, exception));
            }
        }

        /// <summary>
        ///     Creates the data binding.
        /// </summary>
        /// <returns>An instance of <see cref="IDataBinding" /></returns>
        [NotNull]
        protected virtual IDataBinding CreateBinding([NotNull] IDataContext context, out object target, out IBindingPath targetPath)
        {
            IBindingSourceAccessor sourceAccessor;
            var formatExpression = context.GetData(BindingBuilderConstants.MultiExpression);
            var sourceDelegates = context.GetData(BindingBuilderConstants.Sources, true);
            if (sourceDelegates.Count > 1 || formatExpression != null)
            {
                formatExpression = formatExpression ?? FormatMembersExpressionDelegate;
                var sources = new IBindingSource[sourceDelegates.Count];
                for (int index = 0; index < sourceDelegates.Count; index++)
                    sources[index] = Decorate(sourceDelegates[index].Invoke(context), false, context);
                sourceAccessor = new MultiBindingSourceAccessor(sources, formatExpression, context);
            }
            else
                sourceAccessor = new BindingSourceAccessor(Decorate(sourceDelegates[0].Invoke(context), false, context), context, false);
            var binding = new DataBinding(new BindingSourceAccessor(GetBindingTarget(context, out target, out targetPath), context, true), sourceAccessor);
            object source;
            if (context.TryGetData(BindingBuilderConstants.Source, out source))
                binding.Context.AddOrUpdate(BindingConstants.Source, ToolkitExtensions.GetWeakReference(source));
            AddBehaviors(binding, context);
            return binding;
        }

        /// <summary>
        ///     Creates the binding target.
        /// </summary>
        [NotNull]
        protected virtual IBindingSource GetBindingTarget([NotNull] IDataContext context, out object target, out IBindingPath targetPath)
        {
            target = context.GetData(BindingBuilderConstants.Target, true);
            targetPath = context.GetData(BindingBuilderConstants.TargetPath, true);
            var src = context.GetData(BindingBuilderConstants.TargetSource);
            IBindingSource bindingSource = new BindingTarget(BindingServiceProvider.ObserverProvider.Observe(src ?? target, targetPath, false))
            {
                CommandParameterDelegate = context.GetData(BindingBuilderConstants.CommandParameter)
            };
            if (_decorators.Count != 0)
                return Decorate(bindingSource, true, context);
            return bindingSource;
        }

        /// <summary>
        /// Adds the behaviors to the specified binding.
        /// </summary>
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

        /// <summary>
        ///     Decorates the source using <see cref="SourceDecorators" /> property.
        /// </summary>
        protected IBindingSource Decorate(IBindingSource source, bool isTarget, IDataContext context)
        {
            for (int index = 0; index < _decorators.Count; index++)
                _decorators[index].Decorate(ref source, isTarget, context);
            return source;
        }

        private static IDataBinding CreateInvalidaDataBinding(IDataContext dataContext)
        {
            var exception = dataContext.GetData(ExceptionConstant);
            var bindingProvider = dataContext.GetData(ProviderConstant);
            if (exception == null)
                exception = BindingExceptionManager.InvalidBindingMember(typeof(object), "undefined");
            var b = new InvalidDataBinding(exception);
            if (bindingProvider != null)
                bindingProvider.AddBehaviors(b, dataContext);
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