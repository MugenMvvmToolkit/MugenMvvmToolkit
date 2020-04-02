using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingExpressionBuilderComponent : AttachableComponentBase<IBindingManager>, IBindingExpressionBuilderComponent, IHasPriority
    {
        #region Fields

        private readonly StringOrdinalLightDictionary<IBindingComponentBuilder> _componentsDictionary;
        private readonly BindingMemberExpressionCollectorVisitor _expressionCollectorVisitor;
        private readonly IExpressionCompiler? _expressionCompiler;
        private readonly IExpressionParser? _parser;

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
                    bindingExpressions[i] = new BindingExpression(this, result.Target, result.Source, result.Parameters.GetRawValue());
                }

                return bindingExpressions;
            }

            var item = parserResult.Item;
            return new BindingExpression(this, item.Target, item.Source, item.Parameters.GetRawValue());
        }

        #endregion

        #region Nested types

        private sealed class BindingExpression : IHasTargetExpressionBindingExpression
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
                if (_componentBuilders!.Length != 0)
                    binding.Initialize(_componentBuilders.TryGetBindingComponents(binding!, binding, target, source, metadata), metadata);
                if (binding.State == BindingState.Valid)
                    _owner.Owner.OnLifecycleChanged(binding, BindingLifecycleState.Initialized, new BindingTargetState(target), metadata);
                return binding;
            }

            private void Initialize(object target, object? source, IReadOnlyMetadataContext? metadata)
            {
                var parameters = ItemOrList<IExpressionNode, List<IExpressionNode>>.FromRawValue(_parametersRaw);
                _owner.Owner.Components.Get<IBindingExpressionNodeInterceptorComponent>(metadata).Intercept(target, source, ref _targetExpression, ref _sourceExpression, ref parameters, metadata);

                if (!(_targetExpression is IBindingMemberExpressionNode))
                    BindingExceptionManager.ThrowCannotUseExpressionExpected(_targetExpression, typeof(IBindingMemberExpressionNode));

                if (!(_sourceExpression is IBindingMemberExpressionNode))
                {
                    _compiledExpressionSource = _owner._expressionCollectorVisitor.Collect(_sourceExpression, metadata).GetRawValue();
                    _compiledExpression = _owner._expressionCompiler.DefaultIfNull().Compile(_sourceExpression, metadata);
                }

                var dictionary = _owner._componentsDictionary;
                dictionary.Clear();
                _owner.Owner.Components
                    .Get<IBindingComponentProviderComponent>(metadata)
                    .TrySetComponentBuilders(dictionary, _targetExpression, _sourceExpression, parameters.Cast<IReadOnlyList<IExpressionNode>>(), metadata);
                _parametersRaw = null;
                _componentBuilders = dictionary.ValuesToArray();
            }

            #endregion
        }

        #endregion
    }
}