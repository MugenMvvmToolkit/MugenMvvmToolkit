using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Presenters;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Internal.Internal;
using MugenMvvm.UnitTest.Navigation.Internal;
using MugenMvvm.UnitTest.Presenters.Internal;
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
            var results = new List<PresenterResult>();
            var cancellationToken = new CancellationTokenSource().Token;
            var request = new TestHasServiceModel<object>();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var result = new PresenterResult(this, i.ToString(), new TestNavigationProvider(), NavigationType.Alert, DefaultMetadata);
                results.Add(result);
                var component = new TestPresenterComponent
                {
                    TryShow = (p, o, type, arg3, arg4) =>
                    {
                        ++invokeCount;
                        p.ShouldEqual(presenter);
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

            presenter.Show(request, cancellationToken, DefaultMetadata).AsList().SequenceEqual(results).ShouldBeTrue();
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
                var component = new TestPresenterComponent
                {
                    TryClose = (p, o, type, arg3, arg4) =>
                    {
                        ++invokeCount;
                        p.ShouldEqual(presenter);
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

            presenter.TryClose(request, cancellationToken, DefaultMetadata).AsList().SequenceEqual(results).ShouldBeTrue();
            invokeCount.ShouldEqual(componentCount);
        }

        protected override Presenter GetComponentOwner(IComponentCollectionManager? collectionProvider = null)
        {
            return new Presenter(collectionProvider);
        }

        #endregion
    }
}