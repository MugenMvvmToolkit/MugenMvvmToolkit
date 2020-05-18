using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Navigation;
using MugenMvvm.UnitTest.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation
{
    public class NavigationCallbackTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(1, "1", "Tab")]
        [InlineData(2, "12", "Window")]
        public void ConstructorShouldInitializeCallback(int callbackType, string operationId, string navigationType)
        {
            var navigationCallbackType = NavigationCallbackType.Parse(callbackType);
            var type = NavigationType.Parse(navigationType);
            var callback = new NavigationCallback(navigationCallbackType, operationId, type);
            callback.IsCompleted.ShouldBeFalse();
            callback.NavigationId.ShouldEqual(operationId);
            callback.CallbackType.ShouldEqual(navigationCallbackType);
            callback.NavigationType.ShouldEqual(type);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetCallbacksRegisterCallbackShouldReturnCallbacks(int count)
        {
            var list = new List<INavigationCallbackListener>();
            var actionTokens = new List<ActionToken>();
            var callback = new NavigationCallback(NavigationCallbackType.Close, "test", NavigationType.Alert);
            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationCallbackListener();
                list.Add(listener);
                var actionToken = callback.RegisterCallback(listener);
                actionTokens.Add(actionToken);
                callback.GetCallbacks().ToArray().SequenceEqual(list).ShouldBeTrue();
            }

            for (var i = 0; i < count; i++)
            {
                actionTokens[i].Dispose();
                var array = callback.GetCallbacks().ToArray();
                array.SequenceEqual(list.Skip(i + 1)).ShouldBeTrue();
            }
        }

        [Theory]
        [InlineData(1, true, false)]
        [InlineData(1, false, false)]
        [InlineData(1, true, true)]
        [InlineData(10, true, false)]
        [InlineData(10, false, false)]
        [InlineData(10, true, true)]
        public void SetResultShouldSetResult(int count, bool trySetResult, bool isCompletedCallback)
        {
            var invokeCount = 0;
            var callback = new NavigationCallback(NavigationCallbackType.Close, "test", NavigationType.Alert);
            callback.IsCompleted.ShouldBeFalse();

            if (isCompletedCallback)
                callback.SetResult(DefaultMetadata);

            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationCallbackListener
                {
                    OnCompleted = context =>
                    {
                        ++invokeCount;
                        context.ShouldEqual(DefaultMetadata);
                    },
                    OnError = (exception, context) => throw new NotSupportedException(),
                    OnCanceled = (context, cancellationToken) => throw new NotSupportedException()
                };
                callback.RegisterCallback(listener);
            }

            if (!isCompletedCallback)
            {
                if (trySetResult)
                {
                    callback.TrySetResult(DefaultMetadata).ShouldBeTrue();
                    callback.TrySetResult(DefaultMetadata).ShouldBeFalse();
                }
                else
                    callback.SetResult(DefaultMetadata);
            }

            invokeCount.ShouldEqual(count);
            callback.IsCompleted.ShouldBeTrue();
        }

        [Theory]
        [InlineData(1, true, false)]
        [InlineData(1, false, false)]
        [InlineData(1, true, true)]
        [InlineData(10, true, false)]
        [InlineData(10, false, false)]
        [InlineData(10, true, true)]
        public void SetExceptionShouldSetResult(int count, bool trySetResult, bool isCompletedCallback)
        {
            var ex = new Exception();
            var invokeCount = 0;
            var callback = new NavigationCallback(NavigationCallbackType.Close, "test", NavigationType.Alert);
            callback.IsCompleted.ShouldBeFalse();

            if (isCompletedCallback)
                callback.SetException(ex, DefaultMetadata);
            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationCallbackListener
                {
                    OnCompleted = context => throw new NotSupportedException(),
                    OnError = (exception, context) =>
                    {
                        exception.ShouldEqual(ex);
                        ++invokeCount;
                        context.ShouldEqual(DefaultMetadata);
                    },
                    OnCanceled = (context, cancellationToken) => throw new NotSupportedException()
                };
                callback.RegisterCallback(listener);
            }

            if (!isCompletedCallback)
            {
                if (trySetResult)
                {
                    callback.TrySetException(ex, DefaultMetadata).ShouldBeTrue();
                    callback.TrySetException(ex, DefaultMetadata).ShouldBeFalse();
                }
                else
                    callback.SetException(ex, DefaultMetadata);
            }

            invokeCount.ShouldEqual(count);
            callback.IsCompleted.ShouldBeTrue();
        }

        [Theory]
        [InlineData(1, true, false)]
        [InlineData(1, false, false)]
        [InlineData(1, true, true)]
        [InlineData(10, true, false)]
        [InlineData(10, false, false)]
        [InlineData(10, true, true)]
        public void SetCanceledShouldSetResult(int count, bool trySetResult, bool isCompletedCallback)
        {
            var token = new CancellationToken(true);
            var invokeCount = 0;
            var callback = new NavigationCallback(NavigationCallbackType.Close, "test", NavigationType.Alert);
            callback.IsCompleted.ShouldBeFalse();
            if (isCompletedCallback)
                callback.SetCanceled(DefaultMetadata, token);
            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationCallbackListener
                {
                    OnCompleted = context => throw new NotSupportedException(),
                    OnError = (exception, context) => throw new NotSupportedException(),
                    OnCanceled = (context, cancellationToken) =>
                    {
                        ++invokeCount;
                        context.ShouldEqual(DefaultMetadata);
                        cancellationToken.ShouldEqual(cancellationToken);
                    }
                };
                callback.RegisterCallback(listener);
            }

            if (!isCompletedCallback)
            {
                if (trySetResult)
                {
                    callback.TrySetCanceled(DefaultMetadata, token).ShouldBeTrue();
                    callback.TrySetCanceled(DefaultMetadata, token).ShouldBeFalse();
                }
                else
                    callback.SetCanceled(DefaultMetadata, token);
            }

            invokeCount.ShouldEqual(count);
            callback.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public void SetResultShouldThrow()
        {
            var callback = new NavigationCallback(NavigationCallbackType.Close, "test", NavigationType.Alert);
            callback.SetResult(DefaultMetadata);

            ShouldThrow<InvalidOperationException>(() => callback.SetResult(DefaultMetadata));
            ShouldThrow<InvalidOperationException>(() => callback.SetException(new Exception(), DefaultMetadata));
            ShouldThrow<InvalidOperationException>(() => callback.SetCanceled(DefaultMetadata, CancellationToken.None));
        }

        [Fact]
        public void TrySetResultShouldReturnFalse()
        {
            var callback = new NavigationCallback(NavigationCallbackType.Close, "test", NavigationType.Alert);
            callback.SetResult(DefaultMetadata);

            callback.TrySetResult(DefaultMetadata).ShouldBeFalse();
            callback.TrySetException(new Exception(), DefaultMetadata).ShouldBeFalse();
            callback.TrySetCanceled(DefaultMetadata, CancellationToken.None).ShouldBeFalse();
        }

        #endregion
    }
}