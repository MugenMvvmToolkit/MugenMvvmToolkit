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

        private readonly StringOrdinalLightDictionary<IBindingComponentBuilder> _componentsDictionary;
        private readonly BindingMemberExpressionCollectorVisitor _expressionCollectorVisitor;
        private readonly IExpressionCompiler? _expressionCompiler;
        private readonly IExpressionParser? _parser;

        private IBindingComponentProviderComponent[] _componentProviders;
        private IBindingExpressionInterceptor[] _expressionInterceptors;

        #endregion

        #region Constructors

        public BindingExpressionBuilderComponent(IExpressionParser? parser = null, IExpressionCompiler? expressionCompiler = null)
        {
            _parser = parser;
            _expressionCompiler = expressionCompiler;
            _expressionInterceptors = Default.EmptyArray<IBindingExpressionInterceptor>();
            _componentProviders = Default.EmptyArray<IBindingComponentProviderComponent>();
            DefaultBindingComponents = new List<IBindingComponentBuilder>();
            _componentsDictionary = new StringOrdinalLightDictionary<IBindingComponentBuilder>(7);
            _expressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();
            BindingMemberPriorities = new Dictionary<string, int>
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

        public Dictionary<string, int> BindingMemberPriorities { get; }

        public List<IBindingComponentBuilder> DefaultBindingComponents { get; }//todo add values

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression?, IReadOnlyList<IBindingExpression>> TryBuildBindingExpression<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata)
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
            MugenExtensions.ComponentTrackerOnAdded(ref _expressionInterceptors, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _componentProviders, collection, component);
        }

        void IComponentCollectionChangedListener<IComponent<IBindingManager>>.OnRemoved(IComponentCollection<IComponent<IBindingManager>> collection,
            IComponent<IBindingManager> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _expressionInterceptors, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _componentProviders, component);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            _expressionInterceptors = owner.Components.GetComponents().OfType<IBindingExpressionInterceptor>().ToArray();
            _componentProviders = owner.Components.GetComponents().OfType<IBindingComponentProviderComponent>().ToArray();
            owner.Components.Components.Add(this);
        }

        protected override void OnDetachedInternal(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.Components.Remove(this);
            _expressionInterceptors = Default.EmptyArray<IBindingExpressionInterceptor>();
            _componentProviders = Default.EmptyArray<IBindingComponentProviderComponent>();
        }

        private BindingExpressionBase GetBindingExpression(IExpressionNode targetExpression, IExpressionNode sourceExpression,
            ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
        {
            var list = parameters.List;
            var parametersList = list == null
                ? new ItemOrList<IExpressionNode?, List<IExpressionNode>>(parameters.Item)
                : new ItemOrList<IExpressionNode?, List<IExpressionNode>>(new List<IExpressionNode>(list));

            for (int i = 0; i < _expressionInterceptors.Length; i++)
                _expressionInterceptors[i].Intercept(ref targetExpression, ref sourceExpression, ref parametersList, metadata);
            parameters = parametersList.Cast<IReadOnlyList<IExpressionNode>>();

            if (!(targetExpression is IBindingMemberExpressionNode targetMember))
            {
                BindingExceptionManager.ThrowCannotUseExpressionExpected(targetExpression, typeof(IBindingMemberExpressionNode));
                return null;
            }

            if (sourceExpression is IBindingMemberExpressionNode memberExpression)
                return new BindingExpression(this, targetMember, memberExpression, parameters, metadata);

            var memberExpressions = _expressionCollectorVisitor.Collect(sourceExpression);
            var compiledExpression = _expressionCompiler.ServiceIfNull().Compile(sourceExpression, metadata);

            return new MultiBindingExpression(this, targetMember, memberExpressions.GetRawValue(), compiledExpression, parameters, metadata);
        }

        #endregion

        #region Nested types

        private abstract class BindingExpressionBase : IBindingExpression
        {
            #region Fields

            protected readonly IReadOnlyMetadataContext? MetadataRaw;
            protected readonly IBindingMemberExpressionNode TargetExpression;

            private readonly BindingExpressionBuilderComponent _builder;
            private readonly object? _parametersRaw;

            private IBindingComponentBuilder[]? _componentBuilders;

            #endregion

            #region Constructors

            protected BindingExpressionBase(BindingExpressionBuilderComponent builder, IBindingMemberExpressionNode targetExpression,
                ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
            {
                _builder = builder;
                TargetExpression = targetExpression;
                MetadataRaw = metadata;
                _parametersRaw = parameters.GetRawValue();
                if (TargetExpression is IHasPriority hasPriority)
                    Priority = hasPriority.Priority;
                else if (builder.BindingMemberPriorities.TryGetValue(targetExpression.Name, out var p))
                    Priority = p;
            }

            #endregion

            #region Properties

            public bool HasMetadata => !MetadataRaw.IsNullOrEmpty();

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
                    _componentBuilders = BuildComponents(binding, target, source, metadata);

                if (_componentBuilders.Length == 1)
                    binding.AddOrderedComponents(new ItemOrList<IComponent<IBinding>?, IComponent<IBinding>[]>(_componentBuilders[0].GetComponent(binding, target, source, metadata)), metadata);
                else if (_componentBuilders.Length != 0)
                {
                    var components = new IComponent<IBinding>[_componentBuilders.Length];
                    for (var i = 0; i < components.Length; i++)
                        MugenExtensions.AddOrdered(components, _componentBuilders[i].GetComponent(binding, target, source, metadata), i, binding);
                    binding.AddOrderedComponents(components, metadata);
                }

                if (binding.State != BindingState.Disposed)
                    _builder.Owner.OnLifecycleChanged(binding, BindingLifecycleState.Initialized, metadata);
            }

            private IBindingComponentBuilder[] BuildComponents(Binding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
            {
                var dictionary = _builder._componentsDictionary;
                dictionary.Clear();
                foreach (var builder in _builder.DefaultBindingComponents)
                    dictionary[builder.Name] = builder;

                var parameters = ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>>.FromRawValue(_parametersRaw);
                var providers = _builder._componentProviders;
                for (var i = 0; i < providers.Length; i++)
                {
                    var builders = providers[i].TryGetComponentBuilders(binding, target, source, parameters, metadata);
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

                return dictionary.ValuesToArray();
            }

            #endregion
        }

        private sealed class BindingExpression : BindingExpressionBase
        {
            #region Fields

            private readonly IBindingMemberExpressionNode _sourceExpression;

            #endregion

            #region Constructors

            public BindingExpression(BindingExpressionBuilderComponent builder, IBindingMemberExpressionNode targetExpression, IBindingMemberExpressionNode sourceExpression,
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
                var binding = new Binding(TargetExpression.GetTargetObserver(target, source, metadata), _sourceExpression.GetSourceObserver(target, source, metadata));
                InitializeBinding(binding, target, source, metadata);
                return binding;
            }

            #endregion
        }

        private sealed class MultiBindingExpression : BindingExpressionBase
        {
            #region Fields

            private readonly ICompiledExpression _compiledExpression;
            private readonly object? _sourceRaw;

            #endregion

            #region Constructors

            public MultiBindingExpression(BindingExpressionBuilderComponent builder, IBindingMemberExpressionNode targetExpression, object? sourceRaw,
                ICompiledExpression compiledExpression, ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
                : base(builder, targetExpression, parameters, metadata)
            {
                _compiledExpression = compiledExpression;
                _sourceRaw = sourceRaw;
            }

            #endregion

            #region Methods

            public override IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null)
            {
                if (metadata == null)
                    metadata = MetadataRaw;
                ItemOrList<IMemberPathObserver?, IMemberPathObserver[]> sources;
                if (_sourceRaw == null)
                    sources = default;
                else if (_sourceRaw is IBindingMemberExpressionNode[] expressions)
                {
                    var array = new IMemberPathObserver[expressions.Length];
                    for (var i = 0; i < array.Length; i++)
                        array[i] = expressions[i].GetSourceObserver(target, source, metadata);
                    sources = array;
                }
                else
                {
                    var observer = ((IBindingMemberExpressionNode)_sourceRaw).GetSourceObserver(target, source, metadata);
                    sources = new ItemOrList<IMemberPathObserver?, IMemberPathObserver[]>(observer);
                }

                var binding = new MultiBinding(TargetExpression.GetTargetObserver(target, source, metadata), sources, _compiledExpression);
                InitializeBinding(binding, target, source, metadata);
                return binding;
            }

            #endregion
        }

        #endregion
    }
}