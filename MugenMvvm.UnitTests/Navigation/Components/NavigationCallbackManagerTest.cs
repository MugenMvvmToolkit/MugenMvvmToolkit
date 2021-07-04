using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Tests.Metadata;
using MugenMvvm.Tests.Navigation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Navigation.Components
{
    public class NavigationCallbackManagerTest : UnitTestBase
    {
        private readonly NavigationCallbackManager _callbackManager;

        public NavigationCallbackManagerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _callbackManager = new NavigationCallbackManager(AttachedValueManager);
            NavigationDispatcher.AddComponent(_callbackManager);
        }

        [Fact]
        public void TryAddNavigationCallbackShouldIgnoreUnknownType()
        {
            var result = GetPresenterResult(new TestMetadataOwner<IMetadataContext>
            {
                HasMetadata = true,
                Metadata = new MetadataContext()
            });
            _callbackManager.TryAddNavigationCallback(NavigationDispatcher, new NavigationCallbackType(int.MinValue), "t", NavigationType.Window, result, DefaultMetadata)
                            .ShouldBeNull();
        }

        protected override INavigationDispatcher GetNavigationDispatcher() => new NavigationDispatcher(ComponentCollectionManager);

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 1)]
        [InlineData(true, 2)]
        [InlineData(false, 2)]
        [InlineData(true, 10)]
        [InlineData(false, 10)]
        public void TryAddNavigationCallbackShouldAddCallbackToTarget(bool wrapTarget, int count)
        {
            var target = wrapTarget
                ? new TestMetadataOwner<IMetadataContext>
                {
                    HasMetadata = true,
                    Metadata = new MetadataContext()
                }
                : new object();

            var addedCallbacks = new HashSet<INavigationCallback>(ReferenceEqualityComparer.Instance);
            for (var i = 0; i < count; i++)
            {
                if (wrapTarget)
                {
                    var result = GetPresenterResult(target, null, "t");
                    var callback = _callbackManager.TryAddNavigationCallback(NavigationDispatcher, NavigationCallbackType.Showing, result.NavigationId, result.NavigationType,
                        result,
                        DefaultMetadata)!;
                    callback.ShouldNotBeNull();
                    addedCallbacks.Add(callback);
                    _callbackManager.TryGetNavigationCallbacks(NavigationDispatcher, target, DefaultMetadata).ShouldEqual(addedCallbacks);
                    _callbackManager.TryGetNavigationCallbacks(NavigationDispatcher, result, DefaultMetadata).ShouldEqual(addedCallbacks);
                }
                else
                {
                    var callback = _callbackManager.TryAddNavigationCallback(NavigationDispatcher, NavigationCallbackType.Showing, "t", NavigationType.Popup, target,
                        DefaultMetadata)!;
                    callback.ShouldNotBeNull();
                    addedCallbacks.Add(callback);
                    _callbackManager.TryGetNavigationCallbacks(NavigationDispatcher, target, DefaultMetadata).ShouldEqual(addedCallbacks);
                }
            }

            addedCallbacks.Count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 1)]
        [InlineData(true, 2)]
        [InlineData(false, 2)]
        [InlineData(true, 10)]
        [InlineData(false, 10)]
        public void TryInvokeNavigationCallbacksShouldInvokeCallbacks1(bool metadataOwnerTarget, int count)
        {
            var target = metadataOwnerTarget
                ? new TestMetadataOwner<IMetadataContext>
                {
                    HasMetadata = true,
                    Metadata = new MetadataContext()
                }
                : new object();
            var result = GetPresenterResult(target);
            var navigationContext = GetNavigationContext(target, NavigationMode.New, result.NavigationType, result.NavigationId, result.NavigationProvider);
            var type = NavigationCallbackType.Closing;
            var callbacks = new List<INavigationCallback>();

            for (var i = 0; i < count; i++)
            {
                var callback = _callbackManager.TryAddNavigationCallback(NavigationDispatcher, type, result.NavigationId, result.NavigationType, result, DefaultMetadata)!;
                callbacks.Add(callback);
                callback.AddCallback(new TestNavigationCallbackListener
                {
                    OnCompleted = context =>
                    {
                        context.ShouldEqual(navigationContext);
                        callbacks.Remove(callback);
                    },
                    OnCanceled = (_, _) => throw new NotSupportedException(),
                    OnError = (_, _) => throw new NotSupportedException()
                });
            }

            var wrongIdCtx = GetNavigationContext(this, NavigationMode.New);
            _callbackManager.TryInvokeNavigationCallbacks(NavigationDispatcher, type, wrongIdCtx).ShouldBeFalse();
            callbacks.Count.ShouldEqual(count);

            _callbackManager.TryInvokeNavigationCallbacks(NavigationDispatcher, type, navigationContext).ShouldBeTrue();
            callbacks.Count.ShouldEqual(0);
            _callbackManager.TryGetNavigationCallbacks(NavigationDispatcher, target, DefaultMetadata).ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 1)]
        [InlineData(true, 2)]
        [InlineData(false, 2)]
        [InlineData(true, 10)]
        [InlineData(false, 10)]
        public void TryInvokeNavigationCallbacksShouldInvokeCallbacks2(bool metadataOwnerTarget, int count)
        {
            var exception = new Exception();
            var target = metadataOwnerTarget
                ? new TestMetadataOwner<IMetadataContext>
                {
                    HasMetadata = true,
                    Metadata = new MetadataContext()
                }
                : new object();
            var result = GetPresenterResult(target);
            var navigationContext = GetNavigationContext(target, NavigationMode.New, result.NavigationType, result.NavigationId, result.NavigationProvider);
            var type = NavigationCallbackType.Closing;
            var callbacks = new List<INavigationCallback>();

            for (var i = 0; i < count; i++)
            {
                var callback = _callbackManager.TryAddNavigationCallback(NavigationDispatcher, type, result.NavigationId, result.NavigationType, result, DefaultMetadata)!;
                callbacks.Add(callback);
                callback.AddCallback(new TestNavigationCallbackListener
                {
                    OnCompleted = _ => throw new NotSupportedException(),
                    OnCanceled = (_, _) => throw new NotSupportedException(),
                    OnError = (context, ex) =>
                    {
                        ex.ShouldEqual(exception);
                        context.ShouldEqual(navigationContext);
                        callbacks.Remove(callback);
                    }
                });
            }

            var wrongIdCtx = GetNavigationContext(target, NavigationMode.New);
            _callbackManager.TryInvokeNavigationCallbacks(NavigationDispatcher, type, wrongIdCtx, exception).ShouldBeFalse();
            callbacks.Count.ShouldEqual(count);

            _callbackManager.TryInvokeNavigationCallbacks(NavigationDispatcher, type, navigationContext, exception).ShouldBeTrue();
            callbacks.Count.ShouldEqual(0);
            _callbackManager.TryGetNavigationCallbacks(NavigationDispatcher, target, DefaultMetadata).ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 1)]
        [InlineData(true, 2)]
        [InlineData(false, 2)]
        [InlineData(true, 10)]
        [InlineData(false, 10)]
        public void TryInvokeNavigationCallbacksShouldInvokeCallbacks3(bool metadataOwnerTarget, int count)
        {
            var target = metadataOwnerTarget
                ? new TestMetadataOwner<IMetadataContext>
                {
                    HasMetadata = true,
                    Metadata = new MetadataContext()
                }
                : new object();
            var result = GetPresenterResult(target);
            var navigationContext = GetNavigationContext(target, NavigationMode.New, result.NavigationType, result.NavigationId, result.NavigationProvider);
            var type = NavigationCallbackType.Closing;
            var callbacks = new List<INavigationCallback>();

            for (var i = 0; i < count; i++)
            {
                var callback = _callbackManager.TryAddNavigationCallback(NavigationDispatcher, type, result.NavigationId, result.NavigationType, result, DefaultMetadata)!;
                callbacks.Add(callback);
                callback.AddCallback(new TestNavigationCallbackListener
                {
                    OnCompleted = _ => throw new NotSupportedException(),
                    OnCanceled = (context, token) =>
                    {
                        token.ShouldEqual(DefaultCancellationToken);
                        context.ShouldEqual(navigationContext);
                        callbacks.Remove(callback);
                    },
                    OnError = (_, _) => throw new NotSupportedException()
                });
            }

            var wrongIdCtx = GetNavigationContext(target, NavigationMode.New);
            _callbackManager.TryInvokeNavigationCallbacks(NavigationDispatcher, type, wrongIdCtx, DefaultCancellationToken).ShouldBeFalse();
            callbacks.Count.ShouldEqual(count);

            _callbackManager.TryInvokeNavigationCallbacks(NavigationDispatcher, type, navigationContext, DefaultCancellationToken).ShouldBeTrue();
            callbacks.Count.ShouldEqual(0);
            _callbackManager.TryGetNavigationCallbacks(NavigationDispatcher, target, DefaultMetadata).ShouldBeEmpty();
        }
    }
}