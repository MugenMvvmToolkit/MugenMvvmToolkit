using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Presentation;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Tests.Navigation;
using MugenMvvm.Tests.Presentation;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Presentation
{
    public class PresenterTest : ComponentOwnerTestBase<Presenter>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShowShouldBeHandledByComponents(int componentCount)
        {
            var results = new List<PresenterResult>();
            var request = new TestHasServiceModel<object>();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var result = new PresenterResult(this, i.ToString(), TestNavigationProvider.Instance, NavigationType.Alert, DefaultMetadata);
                results.Add(result);
                var component = new TestPresenterComponent
                {
                    TryShow = (p, o, arg3, arg4) =>
                    {
                        ++invokeCount;
                        p.ShouldEqual(Presenter);
                        o.ShouldEqual(request);
                        arg3.ShouldEqual(DefaultMetadata);
                        arg4.ShouldEqual(DefaultCancellationToken);
                        return new[] { result };
                    },
                    Priority = -i
                };
                Presenter.AddComponent(component);
            }

            Presenter.Show(request, DefaultCancellationToken, DefaultMetadata).AsList().ShouldEqual(results);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryCloseShouldBeHandledByComponents(int componentCount)
        {
            var results = new List<PresenterResult>();
            var request = new TestHasServiceModel<object>();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var result = new PresenterResult(this, i.ToString(), new TestNavigationProvider(), NavigationType.Alert, DefaultMetadata);
                results.Add(result);
                var component = new TestPresenterComponent
                {
                    TryClose = (p, o, arg3, arg4) =>
                    {
                        ++invokeCount;
                        p.ShouldEqual(Presenter);
                        o.ShouldEqual(request);
                        arg3.ShouldEqual(DefaultMetadata);
                        arg4.ShouldEqual(DefaultCancellationToken);
                        return new[] { result };
                    },
                    Priority = -i
                };
                Presenter.AddComponent(component);
            }

            Presenter.TryClose(request, DefaultCancellationToken, DefaultMetadata).AsList().ShouldEqual(results);
            invokeCount.ShouldEqual(componentCount);
        }

        protected override IPresenter GetPresenter() => GetComponentOwner(ComponentCollectionManager);

        protected override Presenter GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}