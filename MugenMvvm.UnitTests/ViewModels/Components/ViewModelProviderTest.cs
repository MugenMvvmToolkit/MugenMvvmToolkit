﻿using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.ViewModels.Components
{
    public class ViewModelProviderTest : UnitTestBase
    {
        private readonly TestServiceProvider _serviceProvider;

        public ViewModelProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _serviceProvider = new TestServiceProvider();
            ViewModelManager.AddComponent(new ViewModelProvider(_serviceProvider));
        }

        [Fact]
        public void ShouldIgnoreNonGuidRequest() => ViewModelManager.TryGetViewModel(this, Metadata).ShouldBeNull();

        [Fact]
        public void ShouldUseServiceResolverAndCheckInitializedState()
        {
            var viewModel = new TestViewModel();
            ViewModelManager.AddComponent(new TestViewModelLifecycleListener
            {
                OnLifecycleChanged = (_, _, _, _, _) => throw new NotSupportedException()
            });
            ViewModelManager.Components.Add(new TestLifecycleTrackerComponent<IViewModelManager, ViewModelLifecycleState>
            {
                IsInState = (_, vm, st, m) =>
                {
                    m.ShouldEqual(Metadata);
                    vm.ShouldEqual(viewModel);
                    return st == ViewModelLifecycleState.Initialized;
                }
            });
            _serviceProvider.GetService = type =>
            {
                type.ShouldEqual(viewModel.GetType());
                return viewModel;
            };

            ViewModelManager.TryGetViewModel(viewModel.GetType(), Metadata).ShouldEqual(viewModel);
        }

        [Fact]
        public void ShouldUseServiceResolverAndNotifyLifecycle()
        {
            var viewModel = new TestViewModel();
            var lifecycleStates = new List<ViewModelLifecycleState>();

            ViewModelManager.AddComponent(new TestViewModelLifecycleListener
            {
                OnLifecycleChanged = (_, vm, state, _, m) =>
                {
                    vm.ShouldEqual(viewModel);
                    m.ShouldEqual(Metadata);
                    lifecycleStates.Add(state);
                }
            });
            _serviceProvider.GetService = type =>
            {
                type.ShouldEqual(viewModel.GetType());
                return viewModel;
            };

            ViewModelManager.TryGetViewModel(viewModel.GetType(), Metadata).ShouldEqual(viewModel);
            lifecycleStates.Count.ShouldEqual(2);
            lifecycleStates[0].ShouldEqual(ViewModelLifecycleState.Initializing);
            lifecycleStates[1].ShouldEqual(ViewModelLifecycleState.Initialized);
        }

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);
    }
}