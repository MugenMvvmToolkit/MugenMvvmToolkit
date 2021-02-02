using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Presenters;
using MugenMvvm.UnitTests.Metadata.Internal;
using MugenMvvm.UnitTests.Navigation.Internal;
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
        }

        [Fact]
        public void TryAddNavigationCallbackShouldIgnoreUnknownType()
        {
            var result = new PresenterResult(new TestMetadataOwner<IMetadataContext>
            {
                HasMetadata = true,
                Metadata = new MetadataContext()
            }, "t", NavigationProvider.System, NavigationType.Popup);
            _callbackManager.TryAddNavigationCallback(null!, new NavigationCallbackType(int.MinValue), "t", NavigationType.Window, result, DefaultMetadata).ShouldBeNull();
        }

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
                    var result = new PresenterResult(target, "t", NavigationProvider.System, NavigationType.Popup);
                    var callback = _callbackManager.TryAddNavigationCallback(null!, NavigationCallbackType.Showing, result.NavigationId, result.NavigationType, result,
                        DefaultMetadata)!;
                    callback.ShouldNotBeNull();
                    addedCallbacks.Add(callback);
                    _callbackManager.TryGetNavigationCallbacks(null!, target, DefaultMetadata).AsList().ShouldEqual(addedCallbacks);
                    _callbackManager.TryGetNavigationCallbacks(null!, result, DefaultMetadata).AsList().ShouldEqual(addedCallbacks);
                }
                else
                {
                    var callback = _callbackManager.TryAddNavigationCallback(null!, NavigationCallbackType.Showing, "t", NavigationType.Popup, target, DefaultMetadata)!;
                    callback.ShouldNotBeNull();
                    addedCallbacks.Add(callback);
                    _callbackManager.TryGetNavigationCallbacks(null!, target, DefaultMetadata).AsList().ShouldEqual(addedCallbacks);
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
            var result = new PresenterResult(target, "t", NavigationProvider.System, NavigationType.Popup);
            var navigationContext = new NavigationContext(target, NavigationProvider.System, "t", NavigationType.Popup, NavigationMode.New);
            var type = NavigationCallbackType.Closing;
            var callbacks = new List<INavigationCallback>();

            for (var i = 0; i < count; i++)
            {
                var callback = _callbackManager.TryAddNavigationCallback(null!, type, result.NavigationId, result.NavigationType, result, DefaultMetadata)!;
                callbacks.Add(callback);
                callback.AddCallback(new TestNavigationCallbackListener
                {
                    OnCompleted = context =>
                    {
                        context.ShouldEqual(navigationContext);
                        callbacks.Remove(callback);
                    },
                    OnCanceled = (context, token) => throw new NotSupportedException(),
                    OnError = (ex, context) => throw new NotSupportedException()
                });
            }

            var wrongIdCtx = new NavigationContext(this, NavigationProvider.System, "t-", NavigationType.Popup, NavigationMode.New);
            _callbackManager.TryInvokeNavigationCallbacks(null!, type, wrongIdCtx).ShouldBeFalse();
            callbacks.Count.ShouldEqual(count);

            _callbackManager.TryInvokeNavigationCallbacks(null!, type, navigationContext).ShouldBeTrue();
            callbacks.Count.ShouldEqual(0);
            _callbackManager.TryGetNavigationCallbacks(null!, target, DefaultMetadata).AsList().ShouldBeEmpty();
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
            var result = new PresenterResult(target, "t", NavigationProvider.System, NavigationType.Popup);
            var navigationContext = new NavigationContext(target, NavigationProvider.System, "t", NavigationType.Popup, NavigationMode.New);
            var type = NavigationCallbackType.Closing;
            var callbacks = new List<INavigationCallback>();

            for (var i = 0; i < count; i++)
            {
                var callback = _callbackManager.TryAddNavigationCallback(null!, type, result.NavigationId, result.NavigationType, result, DefaultMetadata)!;
                callbacks.Add(callback);
                callback.AddCallback(new TestNavigationCallbackListener
                {
                    OnCompleted = context => throw new NotSupportedException(),
                    OnCanceled = (context, token) => throw new NotSupportedException(),
                    OnError = (context, ex) =>
                    {
                        ex.ShouldEqual(exception);
                        context.ShouldEqual(navigationContext);
                        callbacks.Remove(callback);
                    }
                });
            }

            var wrongIdCtx = new NavigationContext(this, NavigationProvider.System, "t-", NavigationType.Popup, NavigationMode.New);
            _callbackManager.TryInvokeNavigationCallbacks(null!, type, wrongIdCtx, exception).ShouldBeFalse();
            callbacks.Count.ShouldEqual(count);

            _callbackManager.TryInvokeNavigationCallbacks(null!, type, navigationContext, exception).ShouldBeTrue();
            callbacks.Count.ShouldEqual(0);
            _callbackManager.TryGetNavigationCallbacks(null!, target, DefaultMetadata).AsList().ShouldBeEmpty();
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
            var cancellationToken = new CancellationTokenSource().Token;
            var target = metadataOwnerTarget
                ? new TestMetadataOwner<IMetadataContext>
                {
                    HasMetadata = true,
                    Metadata = new MetadataContext()
                }
                : new object();
            var result = new PresenterResult(target, "t", NavigationProvider.System, NavigationType.Popup);
            var navigationContext = new NavigationContext(target, NavigationProvider.System, "t", NavigationType.Popup, NavigationMode.New);
            var type = NavigationCallbackType.Closing;
            var callbacks = new List<INavigationCallback>();

            for (var i = 0; i < count; i++)
            {
                var callback = _callbackManager.TryAddNavigationCallback(null!, type, result.NavigationId, result.NavigationType, result, DefaultMetadata)!;
                callbacks.Add(callback);
                callback.AddCallback(new TestNavigationCallbackListener
                {
                    OnCompleted = context => throw new NotSupportedException(),
                    OnCanceled = (context, token) =>
                    {
                        token.ShouldEqual(cancellationToken);
                        context.ShouldEqual(navigationContext);
                        callbacks.Remove(callback);
                    },
                    OnError = (ex, context) => throw new NotSupportedException()
                });
            }

            var wrongIdCtx = new NavigationContext(target, NavigationProvider.System, "t-", NavigationType.Popup, NavigationMode.New);
            _callbackManager.TryInvokeNavigationCallbacks(null!, type, wrongIdCtx, cancellationToken).ShouldBeFalse();
            callbacks.Count.ShouldEqual(count);

            _callbackManager.TryInvokeNavigationCallbacks(null!, type, navigationContext, cancellationToken).ShouldBeTrue();
            callbacks.Count.ShouldEqual(0);
            _callbackManager.TryGetNavigationCallbacks(null!, target, DefaultMetadata).AsList().ShouldBeEmpty();
        }
    }
}