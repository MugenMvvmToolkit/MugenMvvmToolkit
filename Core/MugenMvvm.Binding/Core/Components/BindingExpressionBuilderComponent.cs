using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingExpressionBuilderComponent : AttachableComponentBase<IBindingManager>, IBindingExpressionBuilderComponent, IHasPriority,
        IComponentCollectionChangedListener<IComponent<IBindingManager>>, IComparer<IBindingExpression>
    {
        #region Fields

        private readonly Dictionary<string, int> _bindingMemberPriorities;

        private readonly StringOrdinalLightDictionary<IBindingComponentBuilder> _componentsDictionary;
        private readonly List<IBindingComponentBuilder> _defaultBindingComponents;
        private readonly BindingMemberExpressionCollectorVisitor _expressionCollectorVisitor;
        private readonly IExpressionCompiler? _expressionCompiler;
        private readonly IExpressionParser? _parser;

        private IBindingComponentProviderComponent[] _componentProviders;
        private IParameterExpressionInterceptor[] _parameterInterceptors;
        private ISourceExpressionInterceptor[] _sourceInterceptors;
        private ITargetExpressionInterceptor[] _targetInterceptors;

        #endregion

        #region Constructors

        public BindingExpressionBuilderComponent(IExpressionParser? parser = null, IExpressionCompiler? expressionCompiler = null)
        {
            _parser = parser;
            _expressionCompiler = expressionCompiler;
            _targetInterceptors = Default.EmptyArray<ITargetExpressionInterceptor>();
            _sourceInterceptors = Default.EmptyArray<ISourceExpressionInterceptor>();
            _parameterInterceptors = Default.EmptyArray<IParameterExpressionInterceptor>();
            _componentProviders = Default.EmptyArray<IBindingComponentProviderComponent>();
            _defaultBindingComponents = new List<IBindingComponentBuilder>();
            _componentsDictionary = new StringOrdinalLightDictionary<IBindingComponentBuilder>(7);
            _expressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();
            _bindingMemberPriorities = new Dictionary<string, int>
            {
                {BindableMembers.Object.DataContext, int.MaxValue - 1000},
                {"BindingContext", int.MaxValue - 1000},
                {"ItemTemplate", 10},
                {"ItemTemplateSelector", 10},
                {"ContentTemplate", 10},
                {"ContentTemplateSelector", 10},
                {"CommandParameter", 10}
            };
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public IDictionary<string, int> BindingMemberPriorities => _bindingMemberPriorities;

        public IList<IBindingComponentBuilder> DefaultBindingComponents => _defaultBindingComponents; //todo add values

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression?, IReadOnlyList<IBindingExpression>> TryBuildBindingExpression<TExpression>(in TExpression expression,
            IReadOnlyMetadataContext? metadata)
        {
            var parserResult = _parser.ServiceIfNull().Parse(expression, metadata);
            var list = parserResult.List;
            if (list != null)
            {
                var bindingExpressions = new IBindingExpression[list.Count];
                for (var i = 0; i < bindingExpressions.Length; i++)
                {
                    var result = list[i];
                    MugenExtensions.AddOrdered(bindingExpressions, GetBindingExpression(result.Target, result.Source, result.Parameters, metadata), i, this);
                }

                return bindingExpressions;
            }

            var item = parserResult.Item;
            return GetBindingExpression(item.Target, item.Source, item.Parameters, metadata);
        }

        int IComparer<IBindingExpression>.Compare(IBindingExpression x, IBindingExpression y)
        {
            return ((BindingExpressionBase)y).Priority.CompareTo(((BindingExpressionBase)x).Priority);
        }

        void IComponentCollectionChangedListener<IComponent<IBindingManager>>.OnAdded(IComponentCollection<IComponent<IBindingManager>> collection,
            IComponent<IBindingManager> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _parameterInterceptors, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _sourceInterceptors, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _targetInterceptors, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _componentProviders, collection, component);
        }

        void IComponentCollectionChangedListener<IComponent<IBindingManager>>.OnRemoved(IComponentCollection<IComponent<IBindingManager>> collection,
            IComponent<IBindingManager> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _parameterInterceptors, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _sourceInterceptors, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _targetInterceptors, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _componentProviders, component);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            _parameterInterceptors = owner.Components.GetItems().OfType<IParameterExpressionInterceptor>().ToArray();
            _sourceInterceptors = owner.Components.GetItems().OfType<ISourceExpressionInterceptor>().ToArray();
            _targetInterceptors = owner.Components.GetItems().OfType<ITargetExpressionInterceptor>().ToArray();
            _componentProviders = owner.Components.GetItems().OfType<IBindingComponentProviderComponent>().ToArray();
            owner.Components.Components.Add(this);
        }

        protected override void OnDetachedInternal(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.Components.Remove(this);
            _targetInterceptors = Default.EmptyArray<ITargetExpressionInterceptor>();
            _sourceInterceptors = Default.EmptyArray<ISourceExpressionInterceptor>();
            _parameterInterceptors = Default.EmptyArray<IParameterExpressionInterceptor>();
            _componentProviders = Default.EmptyArray<IBindingComponentProviderComponent>();
        }

        private BindingExpressionBase GetBindingExpression(IExpressionNode targetExpression, IExpressionNode sourceExpression,
            ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
        {
            if (!parameters.IsNullOrEmpty())
            {
                var interceptors = _parameterInterceptors;
                IExpressionNode? expression = null;
                List<IExpressionNode>? nodes = null;
                for (var i = 0; i < interceptors.Length; i++)
                    interceptors[i].InterceptParameterExpression(parameters, metadata).Merge(ref expression, ref nodes);
                parameters = nodes ?? new ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>>(expression);
            }

            var targetInterceptors = _targetInterceptors;
            for (var i = 0; i < targetInterceptors.Length; i++)
                targetExpression = targetInterceptors[i].InterceptTargetExpression(targetExpression, parameters, metadata);

            var sourceInterceptors = _sourceInterceptors;
            for (var i = 0; i < sourceInterceptors.Length; i++)
                sourceExpression = sourceInterceptors[i].InterceptSourceExpression(sourceExpression, parameters, metadata);

            if (!(targetExpression is IBindingMemberExpression targetMember))
            {
                BindingExceptionManager.ThrowCannotUseExpressionExpected(targetExpression, typeof(IBindingMemberExpression));
                return null;
            }

            if (sourceExpression is IBindingMemberExpression memberExpression)
                return new BindingExpression(this, targetMember, memberExpression, parameters, metadata);

            var memberExpressions = _expressionCollectorVisitor.Collect(sourceExpression);
            var compiledExpression = _expressionCompiler.ServiceIfNull().Compile(sourceExpression, metadata);

            return new MultiBindingExpression(this, targetMember, memberExpressions, compiledExpression, parameters, metadata);
        }

        #endregion

        #region Nested types

        private abstract class BindingExpressionBase : IBindingExpression
        {
            #region Fields

            private readonly BindingExpressionBuilderComponent _builder;
            private readonly ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> _parameters;
            protected readonly IBindingMemberExpression TargetExpression;
            protected readonly IReadOnlyMetadataContext? MetadataRaw;

            private IBindingComponentBuilder[]? _componentBuilders;

            #endregion

            #region Constructors

            protected BindingExpressionBase(BindingExpressionBuilderComponent builder, IBindingMemberExpression targetExpression,
                ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
            {
                _builder = builder;
                TargetExpression = targetExpression;
                _parameters = parameters;
                MetadataRaw = metadata;
                if (TargetExpression is IHasPriority hasPriority)
                    Priority = hasPriority.Priority;
                else if (builder._bindingMemberPriorities.TryGetValue(targetExpression.Name, out var p))
                    Priority = p;
            }

            #endregion

            #region Properties

            public bool HasMetadata => MetadataRaw != null && MetadataRaw.Count != 0;

            public IReadOnlyMetadataContext Metadata => MetadataRaw ?? Default.Metadata;

            public int Priority { get; }

            #endregion

            #region Implementation of interfaces

            public abstract IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null);

            #endregion

            #region Methods

            protected void InitializeBinding(Binding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
            {
                _builder.Owner.OnLifecycleChanged(binding, BindingLifecycleState.Created, metadata);
                if (_componentBuilders == null)
                {
                    var dictionary = _builder._componentsDictionary;
                    dictionary.Clear();
                    foreach (var builder in _builder._defaultBindingComponents)
                        dictionary[builder.Name] = builder;

                    var providers = _builder._componentProviders;
                    for (var i = 0; i < providers.Length; i++)
                    {
                        var builders = providers[i].TryGetComponentBuilders(binding, target, source, _parameters, metadata);
                        var list = builders.List;
                        var item = builders.Item;
                        if (item == null && list == null)
                            continue;

                        if (list == null)
                            dictionary[item!.Name] = item;
                        else
                        {
                            for (var j = 0; j < list.Count; j++)
                            {
                                item = list[j];
                                dictionary[item.Name] = item;
                            }
                        }
                    }

                    _componentBuilders = dictionary.ValuesToArray();
                }

                if (_componentBuilders.Length == 1)
                    binding.SetComponents(new ItemOrList<IComponent<IBinding>, IComponent<IBinding>[]>(_componentBuilders[0].GetComponent(target, source, metadata)), metadata);
                else if (_componentBuilders.Length != 0)
                {
                    var components = new IComponent<IBinding>[_componentBuilders.Length];
                    for (var i = 0; i < components.Length; i++)
                        MugenExtensions.AddOrdered(components, _componentBuilders[i].GetComponent(target, source, metadata), i, binding);
                    binding.SetComponents(components, metadata);
                }

                if (binding.State != BindingState.Disposed)
                    _builder.Owner.OnLifecycleChanged(binding, BindingLifecycleState.Initialized, metadata);
            }

            #endregion
        }

        private sealed class BindingExpression : BindingExpressionBase
        {
            #region Fields

            private readonly IBindingMemberExpression _sourceExpression;

            #endregion

            #region Constructors

            public BindingExpression(BindingExpressionBuilderComponent builder, IBindingMemberExpression targetExpression, IBindingMemberExpression sourceExpression,
                ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
                : base(builder, targetExpression, parameters, metadata)
            {
                _sourceExpression = sourceExpression;
            }

            #endregion

            #region Methods

            public override IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null)
            {
                if (metadata == null)
                    metadata = MetadataRaw;
                var binding = new Binding(TargetExpression.GetObserver(target, target, metadata), _sourceExpression.GetObserver(target, source, metadata));
                InitializeBinding(binding, target, source, metadata);
                return binding;
            }

            #endregion
        }

        private sealed class MultiBindingExpression : BindingExpressionBase
        {
            #region Fields

            private readonly ICompiledExpression _compiledExpression;
            private readonly IBindingMemberExpression[] _sourceExpressions;

            #endregion

            #region Constructors

            public MultiBindingExpression(BindingExpressionBuilderComponent builder, IBindingMemberExpression targetExpression, IBindingMemberExpression[] sourceExpressions,
                ICompiledExpression compiledExpression, ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
                : base(builder, targetExpression, parameters, metadata)
            {
                _compiledExpression = compiledExpression;
                _sourceExpressions = sourceExpressions;
            }

            #endregion

            #region Methods

            public override IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null)
            {
                if (metadata == null)
                    metadata = MetadataRaw;
                var sources = new IMemberPathObserver[_sourceExpressions.Length];
                for (var i = 0; i < sources.Length; i++)
                    sources[i] = _sourceExpressions[i].GetObserver(target, source, metadata);
                var binding = new MultiBinding(TargetExpression.GetObserver(target, target, metadata), sources, _compiledExpression);
                InitializeBinding(binding, target, source, metadata);
                return binding;
            }

            #endregion
        }

        #endregion
    }
}