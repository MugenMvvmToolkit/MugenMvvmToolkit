using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class ViewLifecycleAwareViewModelDispatcherTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldNotifyViewModel()
        {
            var lifecycleState = ViewLifecycleState.Appeared;
            var state = new object();
            var awareViewModel = new ViewLifecycleAwareViewModel();
            var invokeCount = 0;
            var view = new View(ViewMapping.Undefined, new object(), awareViewModel);

            awareViewModel.OnViewLifecycleChanged = (v, l, s, m) =>
            {
                v.ShouldEqual(view);
                l.ShouldEqual(lifecycleState);
                s.ShouldEqual(state);
                m.ShouldEqual(DefaultMetadata);
                ++invokeCount;
            };
            var component = new ViewLifecycleAwareViewModelDispatcher();
            component.OnLifecycleChanged(null!, view, lifecycleState, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        #endregion

        #region Nested types

        private class ViewLifecycleAwareViewModel : TestViewModelBase, IViewLifecycleAwareViewModel
        {
            #region Properties

            public Action<IView, ViewLifecycleState, object?, IReadOnlyMetadataContext?>? OnViewLifecycleChanged { get; set; }

            #endregion

            #region Implementation of interfaces

            void IViewLifecycleAwareViewModel.OnViewLifecycleChanged(IView view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata) =>
                OnViewLifecycleChanged?.Invoke(view, lifecycleState, state, metadata);

            #endregion
        }

        #endregion
    }
}