using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingExpressionBuilderComponent : AttachableComponentBase<IBindingManager>, IBindingExpressionBuilderComponent, IHasPriority,
        IComponentCollectionChangedListener, IComparer<IBindingExpression>
    {
        #region Fields

        private readonly StringOrdinalLightDictionary<IBindingComponentBuilder> _componentsDictionary;
        private readonly BindingMemberExpressionCollectorVisitor _expressionCollectorVisitor;
        private readonly IExpressionCompiler? _expressionCompiler;
        private readonly IExpressionParser? _parser;
        private bool _isCachePerTypeRequired;

        #endregion

        #region Constructors

        public BindingExpressionBuilderComponent(IExpressionParser? parser = null, IExpressionCompiler? expressionCompiler = null)
        {
            _parser = parser;
            _expressionCompiler = expressionCompiler;
            _componentsDictionary = new StringOrdinalLightDictionary<IBindingComponentBuilder>(7);
            _expressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.ExpressionBuilder;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> TryBuildBindingExpression<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            var parserResult = _parser.DefaultIfNull().Parse(expression, metadata);
            var list = parserResult.List;
            if (list != null)
            {
                var bindingExpressions = new IBindingExpression[list.Count];
                for (var i = 0; i < bindingExpressions.Length; i++)
                {
                    var result = list[i];
                    MugenExtensions.AddOrdered(bindingExpressions, GetBindingExpression(result.Target, result.Source, result.Parameters), i, this);
                }

                return bindingExpressions;
            }

            var item = parserResult.Item;
            return new ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>>(GetBindingExpression(item.Target, item.Source, item.Parameters));
        }

        int IComparer<IBindingExpression>.Compare(IBindingExpression x, IBindingExpression y)
        {
            return GetPriority(y).CompareTo(GetPriority(x));
        }

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IBindingExpressionInterceptorComponent)
                OnComponentsChanged(metadata);
        }

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IBindingExpressionInterceptorComponent)
                OnComponentsChanged(metadata);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            OnComponentsChanged(metadata);
            owner.Components.AddComponent(this, metadata);
        }

        protected override void OnDetachedInternal(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.RemoveComponent(this, metadata);
            OnComponentsChanged(metadata);
        }

        private IBindingExpression GetBindingExpression(IExpressionNode targetExpression, IExpressionNode sourceExpression,
            ItemOrList<IExpressionNode, IReadOnlyList<IExpressionNode>> parameters)
        {
            if (_isCachePerTypeRequired)
                return new BindingExpressionCache(this, targetExpression, sourceExpression, parameters.GetRawValue());
            return new BindingExpression(this, targetExpression, sourceExpression, parameters.GetRawValue());
        }

        private int GetPriority(IBindingExpression expression)
        {
            if (expression is BindingExpressionCache expressionCache)
                return GetPriority(expressionCache.TargetExpression);
            return GetPriority(((BindingExpression)expression).TargetExpression);
        }

        private int GetPriority(IExpressionNode expression)
        {
            var components = Owner.GetComponents<IBindingExpressionPriorityProviderComponent>(null);
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetPriority(expression, out var priority))
                    return priority;
            }

            return 0;
        }

        private void OnComponentsChanged(IReadOnlyMetadataContext? metadata)
        {
            var components = Owner.GetComponents<IBindingExpressionInterceptorComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].IsCachePerTypeRequired)
                {
                    _isCachePerTypeRequired = true;
                    return;
                }
            }

            _isCachePerTypeRequired = false;
        }

        #endregion

        #region Nested types

        private sealed class BindingExpressionCache : LightDictionary<CacheKey, BindingExpression>, IBindingExpression
        {
            #region Fields

            private readonly BindingExpressionBuilderComponent _owner;
            private readonly object? _parametersRaw;
            private readonly IExpressionNode _sourceExpression;

            #endregion

            #region Constructors

            public BindingExpressionCache(BindingExpressionBuilderComponent owner, IExpressionNode targetExpression, IExpressionNode sourceExpression, object? parametersRaw)
            {
                _owner = owner;
                TargetExpression = targetExpression;
                _sourceExpression = sourceExpression;
                _parametersRaw = parametersRaw;
            }

            #endregion

            #region Properties

            public IExpressionNode TargetExpression { get; }

            #endregion

            #region Implementation of interfaces

            public IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null)
            {
                var cacheKey = new CacheKey(target, source);
                if (!TryGetValue(cacheKey, out var value))
                {
                    value = new BindingExpression(_owner, TargetExpression, _sourceExpression, _parametersRaw);
                    this[cacheKey] = value;
                }

                return value.Build(target, source, metadata);
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                return x.SourceType == y.SourceType && x.TargetType == y.TargetType;
            }

            protected override int GetHashCode(CacheKey key)
            {
                return HashCode.Combine(key.SourceType, key.TargetType);
            }

            #endregion
        }

        private sealed class BindingExpression : IBindingExpression
        {
            #region Fields

            private readonly BindingExpressionBuilderComponent _owner;
            private ICompiledExpression? _compiledExpression;
            private object? _compiledExpressionSource;
            private IBindingComponentBuilder[]? _componentBuilders;
            private object? _parametersRaw;
            private IExpressionNode _sourceExpression;
            private IExpressionNode _targetExpression;

            #endregion

            #region Constructors

            public BindingExpression(BindingExpressionBuilderComponent owner, IExpressionNode targetExpression, IExpressionNode sourceExpression, object? parametersRaw)
            {
                _owner = owner;
                _targetExpression = targetExpression;
                _sourceExpression = sourceExpression;
                _parametersRaw = parametersRaw;
            }

            #endregion

            #region Properties

            public IExpressionNode TargetExpression => _targetExpression;

            #endregion

            #region Implementation of interfaces

            public IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null)
            {
                if (_componentBuilders == null)
                    Initialize(target, source, metadata);

                if (_compiledExpression == null)
                {
                    return InitializeBinding(new Core.Binding(((IBindingMemberExpressionNode)_targetExpression).GetBindingTarget(target, source, metadata),
                        ((IBindingMemberExpressionNode)_sourceExpression).GetBindingSource(target, source, metadata)), target, source, metadata);
                }

                return CreateMultiBinding(target, source, metadata);
            }

            #endregion

            #region Methods

            private IBinding CreateMultiBinding(object target, object? source, IReadOnlyMetadataContext? metadata)
            {
                object? sourceRaw;
                switch (_compiledExpressionSource)
                {
                    case null:
                        sourceRaw = null;
                        break;
                    case IBindingMemberExpressionNode[] expressions:
                        {
                            var array = new object?[expressions.Length];
                            for (var i = 0; i < array.Length; i++)
                                array[i] = expressions[i].GetBindingSource(target, source, metadata);
                            sourceRaw = array;
                            break;
                        }
                    default:
                        sourceRaw = ((IBindingMemberExpressionNode)_compiledExpressionSource).GetBindingSource(target, source, metadata);
                        break;
                }

                return InitializeBinding(new MultiBinding(((IBindingMemberExpressionNode)_targetExpression).GetBindingTarget(target, source, metadata), sourceRaw, _compiledExpression!), target, source, metadata);
            }

            private IBinding InitializeBinding(Core.Binding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
            {
                if (_componentBuilders!.Length == 1)
                    binding.Initialize(new ItemOrList<IComponent<IBinding>?, IComponent<IBinding>?[]>(_componentBuilders[0].GetComponent(binding, target, source, metadata)), metadata);
                else if (_componentBuilders.Length != 0)
                {
                    var components = new IComponent<IBinding>?[_componentBuilders.Length];
                    var size = 0;
                    for (var i = 0; i < components.Length; i++)
                    {
                        var component = _componentBuilders[i].GetComponent(binding, target, source, metadata);
                        if (component != null)
                            MugenExtensions.AddOrdered(components!, component, size++, binding!);
                    }

                    binding.Initialize(components, metadata);
                }

                if (binding.State == BindingState.Valid)
                    _owner.Owner.OnLifecycleChanged(binding, BindingLifecycleState.Initialized, metadata);
                return binding;
            }

            private void Initialize(object target, object? source, IReadOnlyMetadataContext? metadata)
            {
                var parameters = ItemOrList<IExpressionNode, List<IExpressionNode>>.FromRawValue(_parametersRaw);
                var interceptors = _owner.Owner.GetComponents<IBindingExpressionInterceptorComponent>(metadata);
                for (var i = 0; i < interceptors.Length; i++)
                    interceptors[i].Intercept(target, source, ref _targetExpression, ref _sourceExpression, ref parameters, metadata);

                if (!(_targetExpression is IBindingMemberExpressionNode))
                    BindingExceptionManager.ThrowCannotUseExpressionExpected(_targetExpression, typeof(IBindingMemberExpressionNode));

                if (!(_sourceExpression is IBindingMemberExpressionNode))
                {
                    _compiledExpressionSource = _owner._expressionCollectorVisitor.Collect(_sourceExpression, metadata).GetRawValue();
                    _compiledExpression = _owner._expressionCompiler.DefaultIfNull().Compile(_sourceExpression, metadata);
                }

                var dictionary = _owner._componentsDictionary;
                dictionary.Clear();

                var parametersReadonly = parameters.Cast<IReadOnlyList<IExpressionNode>>();
                var providers = _owner.Owner.GetComponents<IBindingComponentProviderComponent>(metadata);
                for (var i = 0; i < providers.Length; i++)
                {
                    var builders = providers[i].TryGetComponentBuilders(_targetExpression, _sourceExpression, parametersReadonly, metadata);
                    for (var j = 0; j < builders.Count(); j++)
                    {
                        var item = builders.Get(j);
                        if (item.IsEmpty)
                            dictionary.Remove(item.Name);
                        else
                            dictionary[item.Name] = item;
                    }
                }

                _parametersRaw = null;
                _componentBuilders = dictionary.ValuesToArray();
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct CacheKey
        {
            #region Fields

            public readonly Type? SourceType;
            public readonly Type TargetType;

            #endregion

            #region Constructors

            public CacheKey(object target, object? source)
            {
                TargetType = target.GetType();
                SourceType = source?.GetType();
            }

            #endregion
        }

        #endregion
    }
}