using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Tests.Internal;
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

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void ShouldSetDisposedExceptionIfTargetWasDisposed(int count)
        {
            var target = new TestHasDisposeCallback();
            var result = GetPresenterResult(target);
            var type = NavigationCallbackType.Close;
            var callbacks = new List<INavigationCallback>();
            var tokens = new List<ActionToken>();
            target.RegisterDisposeToken = token => tokens.Add(token);

            for (var i = 0; i < count; i++)
            {
                var callback = _callbackManager.TryAddNavigationCallback(NavigationDispatcher, type, result.NavigationId, result.NavigationType, result, Metadata)!;
                callbacks.Add(callback);
                callback.AddCallback(new TestNavigationCallbackListener
                {
                    OnCompleted = _ => throw new NotSupportedException(),
                    OnCanceled = (_, _) => throw new NotSupportedException(),
                    OnError = (context, ex) =>
                    {
                        ex.ShouldBeType<ObjectDisposedException>();
                        context.Target.ShouldEqual(target);
                        context.NavigationMode.ShouldEqual(NavigationMode.Close);
                        context.NavigationProvider.ShouldEqual(NavigationProvider.System);
                        context.NavigationType.ShouldEqual(result.NavigationType);
                        context.NavigationId.ShouldEqual(result.NavigationId);
                        callbacks.Remove(callback);
                    }
                });
            }

            foreach (var token in tokens)
                token.Dispose();
            callbacks.Count.ShouldEqual(0);
            _callbackManager.TryGetNavigationCallbacks(NavigationDispatcher, target, Metadata).ShouldBeEmpty();
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
                    var result = GetPresenterResult(target, null, "t");
                    var callback = _callbackManager.TryAddNavigationCallback(NavigationDispatcher, NavigationCallbackType.Showing, result.NavigationId, result.NavigationType,
                        result,
                        Metadata)!;
                    callback.ShouldNotBeNull();
                    addedCallbacks.Add(callback);
                    _callbackManager.TryGetNavigationCallbacks(NavigationDispatcher, target, Metadata).ShouldEqual(addedCallbacks);
                    _callbackManager.TryGetNavigationCallbacks(NavigationDispatcher, result, Metadata).ShouldEqual(addedCallbacks);
                }
                else
                {
                    var callback = _callbackManager.TryAddNavigationCallback(NavigationDispatcher, NavigationCallbackType.Showing, "t", NavigationType.Popup, target,
                        Metadata)!;
                    callback.ShouldNotBeNull();
                    addedCallbacks.Add(callback);
                    _callbackManager.TryGetNavigationCallbacks(NavigationDispatcher, target, Metadata).ShouldEqual(addedCallbacks);
                }
            }

            addedCallbacks.Count.ShouldEqual(1);
        }

        [Fact]
        public void TryAddNavigationCallbackShouldIgnoreUnknownType()
        {
            var result = GetPresenterResult(new TestMetadataOwner<IMetadataContext>
            {
                HasMetadata = true,
                Metadata = new MetadataContext()
            });
            _callbackManager.TryAddNavigationCallback(NavigationDispatcher, new NavigationCallbackType(int.MinValue), "t", NavigationType.Window, result, Metadata)
                            .ShouldBeNull();
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 1)]
        [InlineData(true, 2)]
        [InlineData(false, 2)]
        [InlineData(true, 10)]
        [InlineData(false, 10)]
        public void TryInvokeNavigationCallbacksShouldInvokeCallbacksCancel(bool metadataOwnerTarget, int count)
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
                var callback = _callbackManager.TryAddNavigationCallback(NavigationDispatcher, type, result.NavigationId, result.NavigationType, result, Metadata)!;
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
            _callbackManager.TryGetNavigationCallbacks(NavigationDispatcher, target, Metadata).ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 1)]
        [InlineData(true, 2)]
        [InlineData(false, 2)]
        [InlineData(true, 10)]
        [InlineData(false, 10)]
        public void TryInvokeNavigationCallbacksShouldInvokeCallbacksError(bool metadataOwnerTarget, int count)
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
                var callback = _callbackManager.TryAddNavigationCallback(NavigationDispatcher, type, result.NavigationId, result.NavigationType, result, Metadata)!;
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
            _callbackManager.TryGetNavigationCallbacks(NavigationDispatcher, target, Metadata).ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 1)]
        [InlineData(true, 2)]
        [InlineData(false, 2)]
        [InlineData(true, 10)]
        [InlineData(false, 10)]
        public void TryInvokeNavigationCallbacksShouldInvokeCallbacksSuccess(bool metadataOwnerTarget, int count)
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
                var callback = _callbackManager.TryAddNavigationCallback(NavigationDispatcher, type, result.NavigationId, result.NavigationType, result, Metadata)!;
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
            _callbackManager.TryGetNavigationCallbacks(NavigationDispatcher, target, Metadata).ShouldBeEmpty();
        }

        protected override INavigationDispatcher GetNavigationDispatcher() => new NavigationDispatcher(ComponentCollectionManager);
    }
}