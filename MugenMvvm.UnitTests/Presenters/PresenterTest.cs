using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Presenters;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Internal.Internal;
using MugenMvvm.UnitTests.Navigation.Internal;
using MugenMvvm.UnitTests.Presenters.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Presenters
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
            var results = new List<PresenterResult>();
            var cancellationToken = new CancellationTokenSource().Token;
            var request = new TestHasServiceModel<object>();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var result = new PresenterResult(this, i.ToString(), new TestNavigationProvider(), NavigationType.Alert, DefaultMetadata);
                results.Add(result);
                var component = new TestPresenterComponent(presenter)
                {
                    TryShow = (o, arg3, arg4) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(request);
                        arg3.ShouldEqual(DefaultMetadata);
                        arg4.ShouldEqual(cancellationToken);
                        return new[] {result};
                    },
                    Priority = -i
                };
                presenter.AddComponent(component);
            }

            presenter.Show(request, cancellationToken, DefaultMetadata).AsList().ShouldEqual(results);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryCloseShouldBeHandledByComponents(int componentCount)
        {
            var presenter = new Presenter();
            var results = new List<PresenterResult>();
            var cancellationToken = new CancellationTokenSource().Token;
            var request = new TestHasServiceModel<object>();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var result = new PresenterResult(this, i.ToString(), new TestNavigationProvider(), NavigationType.Alert, DefaultMetadata);
                results.Add(result);
                var component = new TestPresenterComponent(presenter)
                {
                    TryClose = (o, arg3, arg4) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(request);
                        arg3.ShouldEqual(DefaultMetadata);
                        arg4.ShouldEqual(cancellationToken);
                        return new[] {result};
                    },
                    Priority = -i
                };
                presenter.AddComponent(component);
            }

            presenter.TryClose(request, cancellationToken, DefaultMetadata).AsList().ShouldEqual(results);
            invokeCount.ShouldEqual(componentCount);
        }

        protected override Presenter GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new Presenter(collectionProvider);

        #endregion
    }
}