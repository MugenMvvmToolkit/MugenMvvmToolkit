using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Presenters;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Models;
using MugenMvvm.UnitTest.Navigation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Presenters
{
    public class PresenterTest : ComponentOwnerTestBase<Presenter>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShowShouldBeHandledByComponents(int componentCount)
        {
            var presenter = new Presenter();
            var result = new PresenterResult("-", new TestNavigationProvider(), NavigationType.Alert, DefaultMetadata);
            var cancellationToken = new CancellationTokenSource().Token;
            var request = new TestHasServiceModel<object>();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestPresenterComponent
                {
                    TryShow = (o, type, arg3, arg4) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(request);
                        type.ShouldEqual(request.GetType());
                        arg3.ShouldEqual(DefaultMetadata);
                        arg4.ShouldEqual(cancellationToken);
                        if (isLast)
                            return result;
                        return default;
                    },
                    Priority = -i
                };
                presenter.AddComponent(component);
            }

            presenter.Show(request, DefaultMetadata, cancellationToken).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CloseShouldBeHandledByComponents(int componentCount)
        {
            var presenter = new Presenter();
            var results = new List<PresenterResult>();
            var cancellationToken = new CancellationTokenSource().Token;
            var request = new TestHasServiceModel<object>();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var result = new PresenterResult(i.ToString(), new TestNavigationProvider(), NavigationType.Alert, DefaultMetadata);
                results.Add(result);
                var component = new TestPresenterComponent
                {
                    TryClose = (o, type, arg3, arg4) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(request);
                        type.ShouldEqual(request.GetType());
                        arg3.ShouldEqual(DefaultMetadata);
                        arg4.ShouldEqual(cancellationToken);
                        return new[] { result };
                    },
                    Priority = -i
                };
                presenter.AddComponent(component);
            }

            presenter.TryClose(request, DefaultMetadata, cancellationToken).SequenceEqual(results).ShouldBeTrue();
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RestoreShouldBeHandledByComponents(int componentCount)
        {
            var presenter = new Presenter();
            var results = new List<PresenterResult>();
            var cancellationToken = new CancellationTokenSource().Token;
            var request = new TestHasServiceModel<object>();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var result = new PresenterResult(i.ToString(), new TestNavigationProvider(), NavigationType.Alert, DefaultMetadata);
                results.Add(result);
                var component = new TestPresenterComponent
                {
                    TryRestore = (o, type, arg3, arg4) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(request);
                        type.ShouldEqual(request.GetType());
                        arg3.ShouldEqual(DefaultMetadata);
                        arg4.ShouldEqual(cancellationToken);
                        return new[] { result };
                    },
                    Priority = -i
                };
                presenter.AddComponent(component);
            }

            presenter.TryRestore(request, DefaultMetadata, cancellationToken).SequenceEqual(results).ShouldBeTrue();
            invokeCount.ShouldEqual(componentCount);
        }

        protected override Presenter GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new Presenter(collectionProvider);
        }

        #endregion
    }
}