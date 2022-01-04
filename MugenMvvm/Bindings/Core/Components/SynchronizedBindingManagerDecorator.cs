using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;

namespace MugenMvvm.Bindings.Core.Components
{
    public class SynchronizedBindingManagerDecorator : ComponentDecoratorBase<IBindingManager, IBindingExpressionParserComponent>, IBindingExpressionParserComponent,
        IBindingExpressionInitializerComponent, IHasCacheComponent<IBindingManager>, IComponentCollectionDecorator<IHasCacheComponent<IBindingManager>>,
        IComponentCollectionDecorator<IBindingExpressionInitializerComponent>, ISynchronizedComponent<IBindingManager>
    {
        private ItemOrArray<IHasCacheComponent<IBindingManager>> _cacheComponents;
        private ItemOrArray<IBindingExpressionInitializerComponent> _initializerComponents;
        private readonly object _syncRoot;

        public SynchronizedBindingManagerDecorator(object? syncRoot = null, int priority = ComponentPriority.Synchronizer) : base(priority)
        {
            _syncRoot = syncRoot ?? this;
        }

        public object SyncRoot => _syncRoot;

        void IBindingExpressionInitializerComponent.Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            lock (_syncRoot)
            {
                _initializerComponents.Initialize(bindingManager, context);
            }
        }

        ItemOrIReadOnlyList<IBindingBuilder> IBindingExpressionParserComponent.TryParseBindingExpression(IBindingManager bindingManager, object expression,
            IReadOnlyMetadataContext? metadata)
        {
            lock (_syncRoot)
            {
                return Components.TryParseBindingExpression(bindingManager, expression, metadata);
            }
        }

        void IComponentCollectionDecorator<IBindingExpressionInitializerComponent>.Decorate(IComponentCollection collection,
            ref ItemOrListEditor<IBindingExpressionInitializerComponent> components,
            IReadOnlyMetadataContext? metadata) =>
            _initializerComponents = this.Decorate(ref components);

        void IComponentCollectionDecorator<IHasCacheComponent<IBindingManager>>.Decorate(IComponentCollection collection,
            ref ItemOrListEditor<IHasCacheComponent<IBindingManager>> components, IReadOnlyMetadataContext? metadata) =>
            _cacheComponents = this.Decorate(ref components);

        void IHasCacheComponent<IBindingManager>.Invalidate(IBindingManager owner, object? state, IReadOnlyMetadataContext? metadata)
        {
            lock (_syncRoot)
            {
                _cacheComponents.Invalidate(owner, state, metadata);
            }
        }
    }
}