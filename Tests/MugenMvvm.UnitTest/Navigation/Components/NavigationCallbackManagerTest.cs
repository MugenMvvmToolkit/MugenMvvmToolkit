using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
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
            component.TryAddNavigationCallback(new NavigationCallbackType(int.MinValue), result, DefaultMetadata).ShouldBeNull();
        }

        [Fact]
        public void TryAddNavigationCallbackShouldIgnoreWrongTarget()
        {
            var result = new PresenterResult(this, "t", Default.NavigationProvider, NavigationType.Popup);
            var component = new NavigationCallbackManager();
            component.TryAddNavigationCallback(new NavigationCallbackType(int.MinValue), result, DefaultMetadata).ShouldBeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void TryAddNavigationCallbackShouldAddCallbackToTarget(int count)
        {
            var target = new TestMetadataOwner<IMetadataContext>
            {
                HasMetadata = true,
                Metadata = new MetadataContext()
            };
            var result = new PresenterResult(target, "t", Default.NavigationProvider, NavigationType.Popup);
            var component = new NavigationCallbackManager();
            var addedCallbacks = new HashSet<INavigationCallback>(ReferenceEqualityComparer.Instance);
            for (var i = 0; i < count; i++)
            {
                var callback = component.TryAddNavigationCallback(NavigationCallbackType.Showing, result, DefaultMetadata)!;
                callback.ShouldNotBeNull();
                addedCallbacks.Add(callback);
                component.TryGetNavigationCallbacks(result, DefaultMetadata)!.SequenceEqual(addedCallbacks).ShouldBeTrue();
                component.TryGetNavigationCallbacks(target, DefaultMetadata)!.SequenceEqual(addedCallbacks).ShouldBeTrue();
            }
            addedCallbacks.Count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void TryInvokeNavigationCallbacksShouldInvokeCallbacks1(int count)
        {
            var target = new TestMetadataOwner<IMetadataContext>
            {
                HasMetadata = true,
                Metadata = new MetadataContext()
            };
            var result = new PresenterResult(target, "t", Default.NavigationProvider, NavigationType.Popup);
            var navigationContext = new NavigationContext(Default.NavigationProvider, "t", NavigationType.Popup, NavigationMode.Background);
            navigationContext.Metadata.Set(NavigationMetadata.Target, target);
            var component = new NavigationCallbackManager();
            var type = NavigationCallbackType.Closing;
            var callbacks = new List<INavigationCallback>();

            for (var i = 0; i < count; i++)
            {
                var callback = component.TryAddNavigationCallback(type, result, DefaultMetadata)!;
                callbacks.Add(callback);
                callback.AddCallback(new TestNavigationCallbackListener
                {
                    OnCompleted = context =>
                    {
                        context.ShouldEqual(navigationContext.GetMetadataOrDefault());
                        callbacks.Remove(callback);
                    },
                    OnCanceled = (context, token) => throw new NotSupportedException(),
                    OnError = (ex, context) => throw new NotSupportedException()
                });
            }

            var wrongIdCtx = new NavigationContext(Default.NavigationProvider, "t-", NavigationType.Popup, NavigationMode.Background);
            wrongIdCtx.Metadata.Set(NavigationMetadata.Target, target);
            component.TryInvokeNavigationCallbacks(type, wrongIdCtx, DefaultMetadata).ShouldBeFalse();
            callbacks.Count.ShouldEqual(count);

            component.TryInvokeNavigationCallbacks(type, navigationContext, DefaultMetadata).ShouldBeTrue();
            callbacks.Count.ShouldEqual(0);
            component.TryGetNavigationCallbacks(target, DefaultMetadata).ShouldBeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void TryInvokeNavigationCallbacksShouldInvokeCallbacks2(int count)
        {
            var exception = new Exception();
            var target = new TestMetadataOwner<IMetadataContext>
            {
                HasMetadata = true,
                Metadata = new MetadataContext()
            };
            var result = new PresenterResult(target, "t", Default.NavigationProvider, NavigationType.Popup);
            var navigationContext = new NavigationContext(Default.NavigationProvider, "t", NavigationType.Popup, NavigationMode.Background);
            navigationContext.Metadata.Set(NavigationMetadata.Target, target);
            var component = new NavigationCallbackManager();
            var type = NavigationCallbackType.Closing;
            var callbacks = new List<INavigationCallback>();

            for (var i = 0; i < count; i++)
            {
                var callback = component.TryAddNavigationCallback(type, result, DefaultMetadata)!;
                callbacks.Add(callback);
                callback.AddCallback(new TestNavigationCallbackListener
                {
                    OnCompleted = context => throw new NotSupportedException(),
                    OnCanceled = (context, token) => throw new NotSupportedException(),
                    OnError = (ex, context) =>
                    {
                        ex.ShouldEqual(exception);
                        context.ShouldEqual(navigationContext.GetMetadataOrDefault());
                        callbacks.Remove(callback);
                    }
                });
            }

            var wrongIdCtx = new NavigationContext(Default.NavigationProvider, "t-", NavigationType.Popup, NavigationMode.Background);
            wrongIdCtx.Metadata.Set(NavigationMetadata.Target, target);
            component.TryInvokeNavigationCallbacks(type, wrongIdCtx, exception, DefaultMetadata).ShouldBeFalse();
            callbacks.Count.ShouldEqual(count);

            component.TryInvokeNavigationCallbacks(type, navigationContext, exception, DefaultMetadata).ShouldBeTrue();
            callbacks.Count.ShouldEqual(0);
            component.TryGetNavigationCallbacks(target, DefaultMetadata).ShouldBeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void TryInvokeNavigationCallbacksShouldInvokeCallbacks3(int count)
        {
            var cancellationToken = new CancellationTokenSource().Token;
            var target = new TestMetadataOwner<IMetadataContext>
            {
                HasMetadata = true,
                Metadata = new MetadataContext()
            };
            var result = new PresenterResult(target, "t", Default.NavigationProvider, NavigationType.Popup);
            var navigationContext = new NavigationContext(Default.NavigationProvider, "t", NavigationType.Popup, NavigationMode.Background);
            navigationContext.Metadata.Set(NavigationMetadata.Target, target);
            var component = new NavigationCallbackManager();
            var type = NavigationCallbackType.Closing;
            var callbacks = new List<INavigationCallback>();

            for (var i = 0; i < count; i++)
            {
                var callback = component.TryAddNavigationCallback(type, result, DefaultMetadata)!;
                callbacks.Add(callback);
                callback.AddCallback(new TestNavigationCallbackListener
                {
                    OnCompleted = context => throw new NotSupportedException(),
                    OnCanceled = (context, token) =>
                    {
                        token.ShouldEqual(cancellationToken);
                        context.ShouldEqual(navigationContext.GetMetadataOrDefault());
                        callbacks.Remove(callback);
                    },
                    OnError = (ex, context) => throw new NotSupportedException()
                });
            }

            var wrongIdCtx = new NavigationContext(Default.NavigationProvider, "t-", NavigationType.Popup, NavigationMode.Background);
            wrongIdCtx.Metadata.Set(NavigationMetadata.Target, target);
            component.TryInvokeNavigationCallbacks(type, wrongIdCtx, cancellationToken, DefaultMetadata).ShouldBeFalse();
            callbacks.Count.ShouldEqual(count);

            component.TryInvokeNavigationCallbacks(type, navigationContext, cancellationToken, DefaultMetadata).ShouldBeTrue();
            callbacks.Count.ShouldEqual(0);
            component.TryGetNavigationCallbacks(target, DefaultMetadata).ShouldBeNull();
        }

        #endregion
    }
}