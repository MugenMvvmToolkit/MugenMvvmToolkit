﻿using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class ViewLifecycleAwareViewModelHandlerTest : UnitTestBase
    {
        [Fact]
        public void ShouldNotifyViewModel()
        {
            var lifecycleState = ViewLifecycleState.Appeared;
            var state = new object();
            var awareViewModel = new ViewLifecycleAwareViewModel(ViewModelManager);
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
            var component = new ViewLifecycleAwareViewModelHandler();
            component.OnLifecycleChanged(null!, new ViewInfo(view), lifecycleState, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);

        private class ViewLifecycleAwareViewModel : TestViewModelBase, IViewLifecycleAwareViewModel
        {
            public ViewLifecycleAwareViewModel(IViewModelManager? viewModelManager = null, IReadOnlyMetadataContext? metadata = null) : base(viewModelManager, metadata)
            {
            }

            public Action<IView, ViewLifecycleState, object?, IReadOnlyMetadataContext?>? OnViewLifecycleChanged { get; set; }

            void IViewLifecycleAwareViewModel.OnViewLifecycleChanged(IView view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata) =>
                OnViewLifecycleChanged?.Invoke(view, lifecycleState, state, metadata);
        }
    }
}