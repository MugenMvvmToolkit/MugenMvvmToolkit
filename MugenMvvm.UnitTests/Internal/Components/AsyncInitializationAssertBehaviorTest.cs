using System;
using System.Threading.Tasks;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal.Components
{
    [Collection(SharedContext)]
    public class AsyncInitializationAssertBehaviorTest : UnitTestBase
    {
        private readonly AsyncInitializationAssertBehavior _behavior;
        private bool _isInitializing;

        public AsyncInitializationAssertBehaviorTest()
        {
            _isInitializing = true;
            _behavior = new AsyncInitializationAssertBehavior(() => _isInitializing, null, false);
            RegisterDisposeToken(WithGlobalService(ComponentCollectionManager));
        }

        [Fact]
        public void ShouldAddListenerIfInitializing()
        {
            ComponentCollectionManager.AddComponent(_behavior);

            _isInitializing = true;
            var collection = ComponentCollectionManager.TryGetComponentCollection(this)!;
            collection.GetComponent<AsyncInitializationAssertBehavior>().ShouldEqual(_behavior);

            _isInitializing = false;
            collection = ComponentCollectionManager.TryGetComponentCollection(this)!;
            collection.GetComponentOptional<AsyncInitializationAssertBehavior>().ShouldBeNull();
            ComponentCollectionManager.GetComponentOptional<AsyncInitializationAssertBehavior>().ShouldBeNull();
        }

        [Fact]
        public async Task ShouldAssertOnDecorate()
        {
            _isInitializing = true;
            var componentCollection = new ComponentCollection(this, ComponentCollectionManager);
            componentCollection.AddComponent(_behavior);

            componentCollection.Add("");
            componentCollection.Add(new object());
            componentCollection.Add(this);

            componentCollection.Get<string>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => Task.Run(() => componentCollection.Get<object>()));

            _isInitializing = false;
            await Task.Run(() => componentCollection.Get<AsyncInitializationAssertBehaviorTest>());
            componentCollection.GetComponentOptional<AsyncInitializationAssertBehavior>().ShouldBeNull();
        }

        [Fact]
        public async Task ShouldDecorateMemberManager()
        {
            _isInitializing = true;
            var componentCollection = new ComponentCollection(this, ComponentCollectionManager);
            componentCollection.AddComponent(_behavior);

            var item = componentCollection.Get<IMemberManagerComponent>().Item;
            item.ShouldEqual(_behavior);

            _behavior.TryGetMembers(null!, GetType(), default, default, this, default).IsEmpty.ShouldBeTrue();
            await Assert.ThrowsAsync<InvalidOperationException>(() => Task.Run(() => _behavior.TryGetMembers(null!, GetType(), default, default, this, default)));

            _isInitializing = false;
            await Task.Run(() => _behavior.TryGetMembers(null!, GetType(), default, default, this, default).IsEmpty.ShouldBeTrue());
        }

        [Fact]
        public async Task ShouldInitializeFallbackConfiguration()
        {
            _behavior.Optional<string>().ShouldBeNull();
            await Assert.ThrowsAsync<InvalidOperationException>(() => Task.Run(() => _behavior.Optional<string>()));
            await Assert.ThrowsAsync<InvalidOperationException>(() => Task.Run(() => _behavior.Instance<string>()));

            _isInitializing = false;
            await Task.Run(() => _behavior.Optional<string>());
        }

        protected override void OnDispose() => MugenService.Configuration.FallbackConfiguration = null;
    }
}