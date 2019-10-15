using System.Collections.Generic;
using System.Linq;
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
    public sealed class ParserBindingExpressionBuilderComponent : AttachableComponentBase<IBindingManager>, IBindingExpressionBuilderComponent, IHasPriority,
        IComponentCollectionChangedListener<IComponent<IBindingManager>>
    {
        #region Fields

        private readonly StringOrdinalLightDictionary<IBindingComponentExpression> _componentsDictionary;
        private readonly List<IBindingComponentExpression> _defaultBindingComponents;
        private readonly BindingMemberExpressionCollectorVisitor _expressionCollectorVisitor;

        private readonly IExpressionCompiler? _expressionCompiler;
        private readonly IExpressionParser? _parser;

        private IParameterExpressionInterceptor[] _parameterInterceptors;
        private ISourceExpressionInterceptor[] _sourceInterceptors;
        private ITargetExpressionInterceptor[] _targetInterceptors;

        #endregion

        #region Constructors

        public ParserBindingExpressionBuilderComponent(IExpressionParser? parser = null, IExpressionCompiler? expressionCompiler = null)
        {
            _parser = parser;
            _expressionCompiler = expressionCompiler;
            _targetInterceptors = Default.EmptyArray<ITargetExpressionInterceptor>();
            _sourceInterceptors = Default.EmptyArray<ISourceExpressionInterceptor>();
            _parameterInterceptors = Default.EmptyArray<IParameterExpressionInterceptor>();
            _defaultBindingComponents = new List<IBindingComponentExpression>();
            _componentsDictionary = new StringOrdinalLightDictionary<IBindingComponentExpression>(7);
            _expressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public IList<IBindingComponentExpression> DefaultBindingComponents => _defaultBindingComponents; //todo add values

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression?, IReadOnlyList<IBindingExpression>> TryBuildBindingExpression<TExpression>(in TExpression expression,
            IReadOnlyMetadataContext? metadata)
        {
            var list = _parser.ServiceIfNull().Parse(expression, metadata);
            if (list.Item.IsEmpty)
            {
                var bindingExpressions = new IBindingExpression[list.List.Count];
                for (var i = 0; i < bindingExpressions.Length; i++)
                {
                    var item = list.List[i];
                    MugenExtensions.AddOrdered(bindingExpressions, GetBindingExpression(item.Target, item.Source, item.Parameters, metadata), null);
                }

                return bindingExpressions;
            }

            return new ItemOrList<IBindingExpression?, IReadOnlyList<IBindingExpression>>(GetBindingExpression(list.Item.Target, list.Item.Source, list.Item.Parameters, metadata));
        }

        void IComponentCollectionChangedListener<IComponent<IBindingManager>>.OnAdded(IComponentCollection<IComponent<IBindingManager>> collection,
            IComponent<IBindingManager> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _parameterInterceptors, Owner, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref _sourceInterceptors, Owner, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref _targetInterceptors, Owner, collection, component, metadata);
        }

        void IComponentCollectionChangedListener<IComponent<IBindingManager>>.OnRemoved(IComponentCollection<IComponent<IBindingManager>> collection,
            IComponent<IBindingManager> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _parameterInterceptors, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref _sourceInterceptors, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref _targetInterceptors, collection, component, metadata);
        }

        void IComponentCollectionChangedListener<IComponent<IBindingManager>>.OnCleared(IComponentCollection<IComponent<IBindingManager>> collection,
            ItemOrList<IComponent<IBindingManager>?, IComponent<IBindingManager>[]> oldItems, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnCleared(ref _parameterInterceptors, collection, oldItems, metadata);
            MugenExtensions.ComponentTrackerOnCleared(ref _sourceInterceptors, collection, oldItems, metadata);
            MugenExtensions.ComponentTrackerOnCleared(ref _targetInterceptors, collection, oldItems, metadata);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            _parameterInterceptors = owner.Components.GetItems().OfType<IParameterExpressionInterceptor>().ToArray();
            _sourceInterceptors = owner.Components.GetItems().OfType<ISourceExpressionInterceptor>().ToArray();
            _targetInterceptors = owner.Components.GetItems().OfType<ITargetExpressionInterceptor>().ToArray();
            owner.Components.Components.Add(this);
        }

        protected override void OnDetachedInternal(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.Components.Remove(this);
            _targetInterceptors = Default.EmptyArray<ITargetExpressionInterceptor>();
            _sourceInterceptors = Default.EmptyArray<ISourceExpressionInterceptor>();
            _parameterInterceptors = Default.EmptyArray<IParameterExpressionInterceptor>();
        }

        private IBindingExpression GetBindingExpression(IExpressionNode targetExpression, IExpressionNode sourceExpression,
            ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
        {
            var targetInterceptors = _targetInterceptors;
            for (var i = 0; i < targetInterceptors.Length; i++)
                targetExpression = targetInterceptors[i].InterceptTargetExpression(targetExpression, metadata);

            var sourceInterceptors = _sourceInterceptors;
            for (var i = 0; i < sourceInterceptors.Length; i++)
                sourceExpression = sourceInterceptors[i].InterceptSourceExpression(sourceExpression, metadata);

            _componentsDictionary.Clear();
            if (!parameters.IsNullOrEmpty())
            {
                var interceptors = _parameterInterceptors;
                for (var i = 0; i < interceptors.Length; i++)
                    parameters = interceptors[i].InterceptParameterExpression(parameters, metadata);

                if (!parameters.IsNullOrEmpty())
                {
                    for (var i = 0; i < _defaultBindingComponents.Count; i++)
                        _componentsDictionary[_defaultBindingComponents[i].Name] = _defaultBindingComponents[i];

                    if (parameters.Item != null)
                    {
                        var expression = GetComponentExpression(parameters.Item);
                        _componentsDictionary[expression.Name] = expression;
                    }
                    else
                    {
                        var list = parameters.List;
                        for (var i = 0; i < list.Count; i++)
                        {
                            var expression = GetComponentExpression(list[i]);
                            _componentsDictionary[expression.Name] = expression;
                        }
                    }
                }
            }

            ItemOrList<IBindingComponentExpression?, IBindingComponentExpression[]> componentParameters;
            if (_componentsDictionary == null)
                componentParameters = default;
            else if (_componentsDictionary.Count == 1)
                componentParameters = new ItemOrList<IBindingComponentExpression?, IBindingComponentExpression[]>(_componentsDictionary.First().Value);
            else
            {
                var expressions = new IBindingComponentExpression[_componentsDictionary.Count];
                var index = 0;
                foreach (var keyValuePair in _componentsDictionary)
                    expressions[index++] = keyValuePair.Value;
                componentParameters = expressions;
            }

            if (!(targetExpression is IBindingMemberExpression targetMember))
            {
                BindingExceptionManager.ThrowCannotCompileExpression(targetExpression); //todo change
                return null;
            }

            if (sourceExpression is IBindingMemberExpression memberExpression)
                return new BindingExpression(targetMember, memberExpression, componentParameters, metadata);

            var memberExpressions = _expressionCollectorVisitor.Collect(sourceExpression);
            var compiledExpression = _expressionCompiler.ServiceIfNull().Compile(sourceExpression, metadata);

            return new MultiBindingExpression(targetMember, memberExpressions, compiledExpression, componentParameters, metadata);
        }

        private IBindingComponentExpression GetComponentExpression(IExpressionNode expression)
        {
            if (expression is IBindingComponentExpression bindingComponentExpression)
                return bindingComponentExpression;
            BindingExceptionManager.ThrowCannotCompileExpression(expression); //todo change
            return null!;
        }

        private static ItemOrList<IComponent<IBinding>, IComponent<IBinding>[]> GetComponents(ItemOrList<IBindingComponentExpression, IBindingComponentExpression[]> components,
            IBinding binding, object target, object? source = null, IReadOnlyMetadataContext? metadata = null)
        {
            var list = components.List;
            if (list == null)
                return new ItemOrList<IComponent<IBinding>, IComponent<IBinding>[]>(components.Item.GetComponent(target, source, metadata));

            var result = new IComponent<IBinding>[list.Length];
            for (var i = 0; i < result.Length; i++)
                MugenExtensions.AddOrdered(result, list[i].GetComponent(target, source, metadata), binding);
            return result;
        }

        #endregion

        #region Nested types

        private sealed class MultiBindingExpression : IBindingExpression, IHasPriority
        {
            #region Fields

            private readonly ICompiledExpression _compiledExpression;
            private readonly ItemOrList<IBindingComponentExpression?, IBindingComponentExpression[]> _components;
            private readonly IBindingMemberExpression[] _sourceExpressions;
            private readonly IBindingMemberExpression _targetExpression;

            #endregion

            #region Constructors

            public MultiBindingExpression(IBindingMemberExpression targetExpression, IBindingMemberExpression[] sourceExpressions,
                ICompiledExpression compiledExpression, ItemOrList<IBindingComponentExpression?, IBindingComponentExpression[]> components, IReadOnlyMetadataContext? metadata)
            {
                _targetExpression = targetExpression;
                _compiledExpression = compiledExpression;
                _sourceExpressions = sourceExpressions;
                _components = components;
                Metadata = metadata ?? Default.Metadata;
                Priority = (_targetExpression as IHasPriority)?.Priority ?? 0;
            }

            #endregion

            #region Properties

            public bool HasMetadata => Metadata.Count != 0;

            public IReadOnlyMetadataContext Metadata { get; }

            public int Priority { get; }

            #endregion

            #region Implementation of interfaces

            public IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null)
            {
                var sources = new IMemberPathObserver[_sourceExpressions.Length];
                for (var i = 0; i < sources.Length; i++)
                    sources[i] = _sourceExpressions[i].GetObserver(target, source, metadata);
                var binding = new MultiBinding(_targetExpression.GetObserver(target, source, metadata), sources, _compiledExpression);
                if (!_components.IsNull())
                    binding.SetComponents(GetComponents(_components!, binding, target, source, metadata), metadata);
                return binding;
            }

            #endregion
        }

        private sealed class BindingExpression : IBindingExpression, IHasPriority
        {
            #region Fields

            private readonly ItemOrList<IBindingComponentExpression?, IBindingComponentExpression[]> _components;
            private readonly IBindingMemberExpression _sourceExpression;
            private readonly IBindingMemberExpression _targetExpression;

            #endregion

            #region Constructors

            public BindingExpression(IBindingMemberExpression targetExpression, IBindingMemberExpression sourceExpression,
                ItemOrList<IBindingComponentExpression?, IBindingComponentExpression[]> components, IReadOnlyMetadataContext? metadata)
            {
                _targetExpression = targetExpression;
                _sourceExpression = sourceExpression;
                _components = components;
                Metadata = metadata ?? Default.Metadata;
                Priority = (_targetExpression as IHasPriority)?.Priority ?? 0;
            }

            #endregion

            #region Properties

            public bool HasMetadata => Metadata.Count != 0;

            public IReadOnlyMetadataContext Metadata { get; }

            public int Priority { get; }

            #endregion

            #region Implementation of interfaces

            public IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null)
            {
                var binding = new Binding(_targetExpression.GetObserver(target, source, metadata), _sourceExpression.GetObserver(target, source, metadata));
                if (!_components.IsNull())
                    binding.SetComponents(GetComponents(_components!, binding, target, source, metadata), metadata);
                return binding;
            }

            #endregion
        }

        #endregion
    }
}