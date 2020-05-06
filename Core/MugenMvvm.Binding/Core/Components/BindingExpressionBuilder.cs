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
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingExpressionBuilder : AttachableComponentBase<IBindingManager>, IBindingExpressionBuilderComponent, IHasPriority
    {
        #region Fields

        private readonly BindingExpressionInitializerContext _context;

        private readonly BindingMemberExpressionCollectorVisitor _expressionCollectorVisitor;
        private readonly IExpressionCompiler? _expressionCompiler;
        private readonly IExpressionParser? _parser;

        #endregion

        #region Constructors

        public BindingExpressionBuilder(IExpressionParser? parser = null, IExpressionCompiler? expressionCompiler = null, IMetadataContextProvider? metadataContextProvider = null)
        {
            _parser = parser;
            _expressionCompiler = expressionCompiler;
            _expressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();
            _context = new BindingExpressionInitializerContext(this, metadataContextProvider);
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
                    bindingExpressions[i] = new BindingExpression(_context, result.Target, result.Source, result.Parameters.GetRawValue());
                }

                return bindingExpressions;
            }

            var item = parserResult.Item;
            return new BindingExpression(_context, item.Target, item.Source, item.Parameters.GetRawValue());
        }

        #endregion

        #region Nested types

        private sealed class BindingExpression : IHasTargetExpressionBindingExpression
        {
            #region Fields

            private readonly BindingExpressionInitializerContext _context;
            private object? _compiledExpression;
            private object? _parametersRaw;
            private object _sourceExpression;

            private static readonly object InitializedState = new object();

            #endregion

            #region Constructors

            public BindingExpression(BindingExpressionInitializerContext context, IExpressionNode targetExpression, IExpressionNode sourceExpression, object? parametersRaw)
            {
                Should.NotBeNull(context, nameof(context));
                Should.NotBeNull(targetExpression, nameof(targetExpression));
                Should.NotBeNull(sourceExpression, nameof(sourceExpression));
                _context = context;
                TargetExpression = targetExpression;
                _sourceExpression = sourceExpression;
                _parametersRaw = parametersRaw;
            }

            #endregion

            #region Properties

            public IExpressionNode TargetExpression { get; private set; }

            #endregion

            #region Implementation of interfaces

            public IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null)
            {
                if (_compiledExpression == null)
                    Initialize(target, source, metadata);

                if (ReferenceEquals(_compiledExpression, InitializedState))
                {
                    return InitializeBinding(new Core.Binding(((IBindingMemberExpressionNode)TargetExpression).GetBindingTarget(target, source, metadata),
                        ((IBindingMemberExpressionNode)_sourceExpression).GetBindingSource(target, source, metadata)), target, source, metadata);
                }

                return CreateMultiBinding(target, source, metadata);
            }

            #endregion

            #region Methods

            private IBinding CreateMultiBinding(object target, object? source, IReadOnlyMetadataContext? metadata)
            {
                object? sourceRaw;
                switch (_sourceExpression)
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
                        sourceRaw = ((IBindingMemberExpressionNode)_sourceExpression).GetBindingSource(target, source, metadata);
                        break;
                }

                return InitializeBinding(new MultiBinding(((IBindingMemberExpressionNode)TargetExpression).GetBindingTarget(target, source, metadata), sourceRaw,
                    (ICompiledExpression)_compiledExpression!), target, source, metadata);
            }

            private IBinding InitializeBinding(Core.Binding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
            {
                if (_parametersRaw != null)
                {
                    if (_parametersRaw is object[] components)
                        binding.Initialize(BindingComponentExtensions.TryGetBindingComponents(components, binding!, binding, target, source, metadata), metadata);
                    else
                        binding.Initialize(new ItemOrList<IComponent<IBinding>?, IComponent<IBinding>?[]>(BindingComponentExtensions.TryGetBindingComponent(_parametersRaw, binding, target, source, metadata)), metadata);
                }

                if (binding.State == BindingState.Valid)
                    ((BindingExpressionBuilder)_context.Owner).Owner.OnLifecycleChanged(binding, BindingLifecycleState.Initialized, new BindingTargetSourceState(target, source), metadata);
                return binding;
            }

            private void Initialize(object target, object? source, IReadOnlyMetadataContext? metadata)
            {
                var component = (BindingExpressionBuilder)_context.Owner;
                _context.Initialize(target, source, TargetExpression, (IExpressionNode?)_sourceExpression, ItemOrList<IExpressionNode, IList<IExpressionNode>>.FromRawValue(_parametersRaw), metadata);
                component.Owner.Components.Get<IBindingExpressionInitializerComponent>(metadata).Initialize(_context);
                TargetExpression = _context.TargetExpression;
                var sourceExpression = _context.SourceExpression;

                if (!(TargetExpression is IBindingMemberExpressionNode))
                    BindingExceptionManager.ThrowCannotUseExpressionExpected(TargetExpression, typeof(IBindingMemberExpressionNode));

                if (sourceExpression is IBindingMemberExpressionNode)
                {
                    _sourceExpression = sourceExpression;
                    _compiledExpression = InitializedState;
                }
                else
                {
                    if (sourceExpression == null)
                        BindingExceptionManager.ThrowExpressionNodeCannotBeNull(typeof(BindingExpression));
                    _sourceExpression = component._expressionCollectorVisitor.Collect(sourceExpression, metadata).GetRawValue()!;
                    _compiledExpression = component._expressionCompiler.DefaultIfNull().Compile(sourceExpression, metadata);
                }

                ItemOrList<object, List<object>> components = default;
                foreach (var componentPair in _context.BindingComponents)
                {
                    if (componentPair.Value != null)
                        components.Add(componentPair.Value);
                }

                _parametersRaw = components.Item ?? components.List?.ToArray();
                _context.Clear();
            }

            #endregion
        }

        #endregion
    }
}