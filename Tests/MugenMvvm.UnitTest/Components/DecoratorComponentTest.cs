using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;
using MugenMvvm.Threading;
using MugenMvvm.UnitTest.Threading;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Components
{
    public class DecoratorComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void AttachDetachShouldAddRemoveDecorator()
        {
            var threadDispatcher = new ThreadDispatcher();
            var decorator = new TestDecoratorThreadDispatcherComponent();
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
            var decorator1 = new TestDecoratorThreadDispatcherComponent();
            var decorator2 = new TestDecoratorThreadDispatcherComponent();
            var component1 = new TestThreadDispatcherComponent();
            var component2 = new TestThreadDispatcherComponent();
            var components = new List<IThreadDispatcherComponent> {decorator1, decorator2, component1, component2};

            ((IDecoratorComponentCollectionComponent<IThreadDispatcherComponent>) decorator2).Decorate(components, DefaultMetadata);
            ((IDecoratorComponentCollectionComponent<IThreadDispatcherComponent>) decorator1).Decorate(components, DefaultMetadata);

            components.Single().ShouldEqual(decorator1);
            decorator1.Components.Single().ShouldEqual(decorator2);
            decorator2.Components.SequenceEqual(new[] {component1, component2});
        }

        #endregion

        #region Nested types

        private sealed class TestDecoratorThreadDispatcherComponent : TestDecoratorComponent<IThreadDispatcher, IThreadDispatcherComponent>, IThreadDispatcherComponent
        {
            #region Implementation of interfaces

            public bool CanExecuteInline(ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
            {
                throw new NotSupportedException();
            }

            public bool TryExecute<TState>(ThreadExecutionMode executionMode, object handler, TState state, IReadOnlyMetadataContext? metadata)
            {
                throw new NotSupportedException();
            }

            #endregion
        }

        #endregion
    }
}