using System;
using System.Threading.Tasks;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal.Components
{
    [Collection(SharedContext)]
    public class AsyncInitializationAssertBehaviorTest : UnitTestBase, IDisposable
    {
        private readonly AsyncInitializationAssertBehavior _behavior;
        private bool _isInitializing;

        public AsyncInitializationAssertBehaviorTest()
        {
            _isInitializing = true;
            _behavior = new AsyncInitializationAssertBehavior(() => _isInitializing, null, false);
            MugenService.Configuration.InitializeInstance<IComponentCollectionManager>(new ComponentCollectionManager());
        }

        public void Dispose()
        {
            MugenService.Configuration.FallbackConfiguration = null;
            MugenService.Configuration.Clear<IComponentCollectionManager>();
        }

        [Fact]
        public void ShouldAddListenerIfInitializing()
        {
            var collectionManager = new ComponentCollectionManager();
            collectionManager.AddComponent(_behavior);

            _isInitializing = true;
            var collection = collectionManager.TryGetComponentCollection(this);
            collection.GetComponent<AsyncInitializationAssertBehavior>().ShouldEqual(_behavior);

            _isInitializing = false;
            collection = collectionManager.TryGetComponentCollection(this);
            collection.GetComponentOptional<AsyncInitializationAssertBehavior>().ShouldBeNull();
            collectionManager.GetComponentOptional<AsyncInitializationAssertBehavior>().ShouldBeNull();
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
    }
}