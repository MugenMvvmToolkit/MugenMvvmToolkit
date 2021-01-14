using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Components.Internal;
using MugenMvvm.UnitTests.Threading.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Components
{
    public class ComponentDecoratorTest : UnitTestBase
    {
        [Fact]
        public void AttachDetachShouldAddRemoveDecorator()
        {
            var threadDispatcher = new ThreadDispatcher();
            var decorator = new TestThreadDispatcherDecorator();
            threadDispatcher.AddComponent(decorator);

            threadDispatcher.Components.Get<object>().Single().ShouldEqual(decorator);
            threadDispatcher.Components.Components.Get<object>().Single().ShouldEqual(decorator);

            threadDispatcher.RemoveComponent(decorator);
            threadDispatcher.Components.Get<object>().ShouldBeEmpty();
            threadDispatcher.Components.Components.Get<object>().ShouldBeEmpty();
        }

        [Fact]
        public void ShouldDecorateItems()
        {
            var collection = new ComponentCollection(this);
            var decorator1 = new TestThreadDispatcherDecorator();
            var decorator2 = new TestThreadDispatcherDecorator();
            var component1 = new TestThreadDispatcherComponent();
            var component2 = new TestThreadDispatcherComponent();
            var components = new ItemOrListEditor<IThreadDispatcherComponent>(new List<IThreadDispatcherComponent> {decorator1, decorator2, component1, component2});

            ((IComponentCollectionDecorator<IThreadDispatcherComponent>) decorator2).Decorate(collection, ref components, DefaultMetadata);
            ((IComponentCollectionDecorator<IThreadDispatcherComponent>) decorator1).Decorate(collection, ref components, DefaultMetadata);

            components.AsList().Single().ShouldEqual(decorator1);
            decorator1.Components.Single().ShouldEqual(decorator2);
            decorator2.Components.ShouldEqual(new[] {component1, component2});
        }

        private sealed class TestThreadDispatcherDecorator : TestComponentDecorator<IThreadDispatcher, IThreadDispatcherComponent>, IThreadDispatcherComponent
        {
            public bool CanExecuteInline(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata) =>
                throw new NotSupportedException();

            public bool TryExecute(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, object handler, object? state, IReadOnlyMetadataContext? metadata) =>
                throw new NotSupportedException();
        }
    }
}