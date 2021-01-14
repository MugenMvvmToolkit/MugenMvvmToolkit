using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Visitors;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class BindingExpressionParser : AttachableComponentBase<IBindingManager>, IBindingExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly BindingExpressionInitializerContext _context;
        private readonly BindingMemberExpressionCollectorVisitor _expressionCollectorVisitor;
        private readonly IExpressionCompiler? _expressionCompiler;
        private readonly IExpressionParser? _parser;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BindingExpressionParser(IExpressionParser? parser = null, IExpressionCompiler? expressionCompiler = null)
        {
            _parser = parser;
            _expressionCompiler = expressionCompiler;
            _expressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();
            _context = new BindingExpressionInitializerContext(this);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.ExpressionParser;

        #endregion

        #region Implementation of interfaces

        public ItemOrIReadOnlyList<IBindingBuilder> TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            var parserResult = _parser.DefaultIfNull().TryParse(expression, metadata);
            var count = parserResult.Count;
            if (count == 0)
                return default;
            if (count == 1)
                return new BindingBuilder(_context, parserResult.Item.Target!, parserResult.Item.Source!, parserResult.Item.Parameters.GetRawValue());

            var bindingExpressions = new IBindingBuilder[count];
            int index = 0;
            foreach (var result in parserResult)
                bindingExpressions[index++] = new BindingBuilder(_context, result.Target!, result.Source!, result.Parameters.GetRawValue());
            return bindingExpressions;
        }

        #endregion

        #region Nested types

        private sealed class BindingBuilder : IHasTargetExpressionBindingBuilder
        {
            #region Fields

            private readonly BindingExpressionInitializerContext _context;
            private object? _compiledExpression;
            private object? _parametersRaw;
            private object _sourceExpression;

            private static readonly object InitializedState = new();

            #endregion

            #region Constructors

            public BindingBuilder(BindingExpressionInitializerContext context, IExpressionNode targetExpression, IExpressionNode sourceExpression, object? parametersRaw)
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

                if (_compiledExpression == InitializedState)
                {
                    return InitializeBinding(new Binding((IMemberPathObserver) ((IBindingMemberExpressionNode) TargetExpression).GetBindingSource(target, source, metadata)!,
                        ((IBindingMemberExpressionNode) _sourceExpression).GetBindingSource(target, source, metadata)), target, source, metadata);
                }

                return InitializeBinding(new MultiBinding((IMemberPathObserver) ((IBindingMemberExpressionNode) TargetExpression).GetBindingSource(target, source, metadata)!,
                    BindingMugenExtensions.ToBindingSource(_sourceExpression, target, source, metadata), (ICompiledExpression) _compiledExpression!), target, source, metadata);
            }

            #endregion

            #region Methods

            private IBinding InitializeBinding(Binding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
            {
                if (_parametersRaw != null)
                {
                    if (_parametersRaw is object[] components)
                        binding.Initialize(BindingComponentExtensions.TryGetBindingComponents(components, binding!, binding, target, source, metadata), metadata);
                    else
                        binding.Initialize(ItemOrArray.FromItem<object?>(BindingComponentExtensions.TryGetBindingComponent(_parametersRaw, binding, target, source, metadata)), metadata);
                }

                if (binding.State == BindingState.Valid)
                    ((BindingExpressionParser) _context.Owner).Owner.OnLifecycleChanged(binding, BindingLifecycleState.Initialized, null, metadata);
                return binding;
            }

            private void Initialize(object target, object? source, IReadOnlyMetadataContext? metadata)
            {
                var component = (BindingExpressionParser) _context.Owner;
                _context.Initialize(target, source, TargetExpression, (IExpressionNode?) _sourceExpression, GetParameters(), metadata);
                component.Owner.Components.Get<IBindingExpressionInitializerComponent>(metadata).Initialize(component.Owner, _context);
                TargetExpression = _context.TargetExpression;
                var sourceExpression = _context.SourceExpression;

                if (!(TargetExpression is IBindingMemberExpressionNode))
                    ExceptionManager.ThrowCannotUseExpressionExpected(TargetExpression, typeof(IBindingMemberExpressionNode));

                if (sourceExpression is IBindingMemberExpressionNode)
                {
                    _sourceExpression = sourceExpression;
                    _compiledExpression = InitializedState;
                }
                else
                {
                    if (sourceExpression == null)
                        ExceptionManager.ThrowExpressionNodeCannotBeNull(typeof(BindingBuilder));
                    _sourceExpression = component._expressionCollectorVisitor.Collect(ref sourceExpression, metadata).GetRawValue()!;
                    _compiledExpression = component._expressionCompiler.DefaultIfNull().Compile(sourceExpression, metadata);
                }

                int size = _context.Components.Count;
                if (size > 1)
                {
                    foreach (var componentPair in _context.Components)
                    {
                        if (componentPair.Value != null)
                            ++size;
                    }
                }

                var components = ItemOrArray.Get<object>(size);
                size = 0;
                foreach (var componentPair in _context.Components)
                {
                    if (componentPair.Value != null)
                        components.SetAt(size++, componentPair.Value);
                }

                _parametersRaw = components.GetRawValue();
                _context.Clear();
            }

            private ItemOrIReadOnlyList<IExpressionNode> GetParameters()
            {
                if (_parametersRaw == null)
                    return default;
                if (_parametersRaw is IReadOnlyList<IExpressionNode> parameters)
                    return new ItemOrIReadOnlyList<IExpressionNode>(parameters.ToList());
                return new ItemOrIReadOnlyList<IExpressionNode>((IExpressionNode) _parametersRaw, true);
            }

            #endregion
        }

        #endregion
    }
}