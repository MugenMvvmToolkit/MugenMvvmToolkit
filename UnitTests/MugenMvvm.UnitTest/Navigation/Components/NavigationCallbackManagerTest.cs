using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Presenters;
using MugenMvvm.UnitTest.Internal.Internal;
using MugenMvvm.UnitTest.Metadata.Internal;
using MugenMvvm.UnitTest.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation.Components
{
    public class NavigationCallbackManagerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryAddNavigationCallbackShouldIgnoreUnknownType()
        {
            var result = new PresenterResult(new TestMetadataOwner<IMetadataContext>
            {
                HasMetadata = true,
                Metadata = new MetadataContext()
            }, "t", Default.NavigationProvider, NavigationType.Popup);
            var component = new NavigationCallbackManager();
            component.TryAddNavigationCallback(null!, new NavigationCallbackType(int.MinValue), "t", NavigationType.Window, result, DefaultMetadata).ShouldBeNull();
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
            var component = new NavigationCallbackManager();
            var addedCallbacks = new HashSet<INavigationCallback>(ReferenceEqualityComparer.Instance);
            for (var i = 0; i < count; i++)
            {
                if (wrapTarget)
                {
                    var result = new PresenterResult(target, "t", Default.NavigationProvider, NavigationType.Popup);
                    var callback = component.TryAddNavigationCallback(null!, NavigationCallbackType.Showing, result.NavigationId, result.NavigationType, result, DefaultMetadata)!;
                    callback.ShouldNotBeNull();
                    addedCallbacks.Add(callback);
                    component.TryGetNavigationCallbacks(null!, target, DefaultMetadata).AsList().SequenceEqual(addedCallbacks).ShouldBeTrue();
                    component.TryGetNavigationCallbacks(null!, result, DefaultMetadata).AsList().SequenceEqual(addedCallbacks).ShouldBeTrue();
                }
                else
                {
                    var callback = component.TryAddNavigationCallback(null!, NavigationCallbackType.Showing, "t", NavigationType.Popup, target, DefaultMetadata)!;
                    callback.ShouldNotBeNull();
                    addedCallbacks.Add(callback);
                    component.TryGetNavigationCallbacks(null!, target, DefaultMetadata).AsList().SequenceEqual(addedCallbacks).ShouldBeTrue();
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
            var result = new PresenterResult(target, "t", Default.NavigationProvider, NavigationType.Popup);
            var navigationContext = new NavigationContext(target, Default.NavigationProvider, "t", NavigationType.Popup, NavigationMode.New);
            var component = new NavigationCallbackManager();
            var type = NavigationCallbackType.Closing;
            var callbacks = new List<INavigationCallback>();

            for (var i = 0; i < count; i++)
            {
                var callback = component.TryAddNavigationCallback(null!, type, result.NavigationId, result.NavigationType, result, DefaultMetadata)!;
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

            var wrongIdCtx = new NavigationContext(this, Default.NavigationProvider, "t-", NavigationType.Popup, NavigationMode.New);
            component.TryInvokeNavigationCallbacks(null!, type, wrongIdCtx).ShouldBeFalse();
            callbacks.Count.ShouldEqual(count);

            component.TryInvokeNavigationCallbacks(null!, type, navigationContext).ShouldBeTrue();
            callbacks.Count.ShouldEqual(0);
            component.TryGetNavigationCallbacks(null!, target, DefaultMetadata).AsList().ShouldBeEmpty();
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
            var result = new PresenterResult(target, "t", Default.NavigationProvider, NavigationType.Popup);
            var navigationContext = new NavigationContext(target, Default.NavigationProvider, "t", NavigationType.Popup, NavigationMode.New);
            var component = new NavigationCallbackManager();
            var type = NavigationCallbackType.Closing;
            var callbacks = new List<INavigationCallback>();

            for (var i = 0; i < count; i++)
            {
                var callback = component.TryAddNavigationCallback(null!, type, result.NavigationId, result.NavigationType, result, DefaultMetadata)!;
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

            var wrongIdCtx = new NavigationContext(this, Default.NavigationProvider, "t-", NavigationType.Popup, NavigationMode.New);
            component.TryInvokeNavigationCallbacks(null!, type, wrongIdCtx, exception).ShouldBeFalse();
            callbacks.Count.ShouldEqual(count);

            component.TryInvokeNavigationCallbacks(null!, type, navigationContext, exception).ShouldBeTrue();
            callbacks.Count.ShouldEqual(0);
            component.TryGetNavigationCallbacks(null!, target, DefaultMetadata).AsList().ShouldBeEmpty();
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
            var result = new PresenterResult(target, "t", Default.NavigationProvider, NavigationType.Popup);
            var navigationContext = new NavigationContext(target, Default.NavigationProvider, "t", NavigationType.Popup, NavigationMode.New);
            var component = new NavigationCallbackManager();
            var type = NavigationCallbackType.Closing;
            var callbacks = new List<INavigationCallback>();

            for (var i = 0; i < count; i++)
            {
                var callback = component.TryAddNavigationCallback(null!, type, result.NavigationId, result.NavigationType, result, DefaultMetadata)!;
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

            var wrongIdCtx = new NavigationContext(target, Default.NavigationProvider, "t-", NavigationType.Popup, NavigationMode.New);
            component.TryInvokeNavigationCallbacks(null!, type, wrongIdCtx, cancellationToken).ShouldBeFalse();
            callbacks.Count.ShouldEqual(count);

            component.TryInvokeNavigationCallbacks(null!, type, navigationContext, cancellationToken).ShouldBeTrue();
            callbacks.Count.ShouldEqual(0);
            component.TryGetNavigationCallbacks(null!, target, DefaultMetadata).AsList().ShouldBeEmpty();
        }

        #endregion
    }
}