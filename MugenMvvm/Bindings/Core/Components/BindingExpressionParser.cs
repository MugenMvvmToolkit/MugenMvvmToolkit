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

        public ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            var parserResult = _parser.DefaultIfNull().TryParse(expression, metadata);
            var list = parserResult.List;
            if (list != null)
            {
                var bindingExpressions = new IBindingBuilder[list.Count];
                for (var i = 0; i < bindingExpressions.Length; i++)
                {
                    var result = list[i];
                    bindingExpressions[i] = new BindingBuilder(_context, result.Target, result.Source, result.Parameters.GetRawValue());
                }

                return ItemOrList.FromListToReadOnly(bindingExpressions);
            }

            var item = parserResult.Item;
            if (item.IsEmpty)
                return default;
            return ItemOrList.FromItem<IBindingBuilder>(new BindingBuilder(_context, item.Target, item.Source, item.Parameters.GetRawValue()));
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

            private static readonly object InitializedState = new object();

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
                    MugenBindingExtensions.ToBindingSource(_sourceExpression, target, source, metadata), (ICompiledExpression) _compiledExpression!), target, source, metadata);
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
                        binding.Initialize(ItemOrList.FromItem<IComponent<IBinding>?, IComponent<IBinding>?[]>(BindingComponentExtensions.TryGetBindingComponent(_parametersRaw, binding, target, source, metadata)),
                            metadata);
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
                    BindingExceptionManager.ThrowCannotUseExpressionExpected(TargetExpression, typeof(IBindingMemberExpressionNode));

                if (sourceExpression is IBindingMemberExpressionNode)
                {
                    _sourceExpression = sourceExpression;
                    _compiledExpression = InitializedState;
                }
                else
                {
                    if (sourceExpression == null)
                        BindingExceptionManager.ThrowExpressionNodeCannotBeNull(typeof(BindingBuilder));
                    _sourceExpression = component._expressionCollectorVisitor.Collect(sourceExpression, metadata).GetRawValue()!;
                    _compiledExpression = component._expressionCompiler.DefaultIfNull().Compile(sourceExpression, metadata);
                }

                var components = ItemOrListEditor.Get<object>();
                foreach (var componentPair in _context.BindingComponents)
                    components.AddIfNotNull(componentPair.Value!);

                var itemOrList = components.ToItemOrList();
                _parametersRaw = itemOrList.Item ?? itemOrList.List?.ToArray();
                _context.Clear();
            }

            private ItemOrList<IExpressionNode, IList<IExpressionNode>> GetParameters()
            {
                if (_parametersRaw == null)
                    return default;
                if (_parametersRaw is IReadOnlyList<IExpressionNode> parameters)
                    return ItemOrList.FromList(iList: parameters.ToList());
                return ItemOrList.FromRawValue<IExpressionNode, IList<IExpressionNode>>(_parametersRaw);
            }

            #endregion
        }

        #endregion
    }
}