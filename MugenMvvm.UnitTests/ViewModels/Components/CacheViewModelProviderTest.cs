using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.ViewModels.Components
{
    public class CacheViewModelProviderTest : UnitTestBase
    {
        private readonly ViewModelManager _viewModelManager;

        public CacheViewModelProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModelManager = new ViewModelManager(ComponentCollectionManager);
            _viewModelManager.AddComponent(new CacheViewModelProvider());
        }

        [Fact]
        public void ShouldIgnoreNonStringRequest()
        {
            _viewModelManager.TryGetViewModel(this, DefaultMetadata).ShouldBeNull();
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void ShouldTrackCreatedViewModels(int count, bool isWeak)
        {
            _viewModelManager.RemoveComponents<CacheViewModelProvider>();
            var component = new CacheViewModelProvider(isWeak);
            _viewModelManager.AddComponent(component);

            var vms = new List<TestViewModel>();
            //created
            for (var i = 0; i < count; i++)
            {
                var testViewModel = new TestViewModel();
                vms.Add(testViewModel);
                _viewModelManager.TryGetViewModel(testViewModel.Metadata.Get(ViewModelMetadata.Id)!).ShouldBeNull();
                _viewModelManager.OnLifecycleChanged(testViewModel, ViewModelLifecycleState.Created, i, DefaultMetadata);
                _viewModelManager.TryGetViewModel(testViewModel.Metadata.Get(ViewModelMetadata.Id)!).ShouldEqual(testViewModel);
            }

            //restored
            for (var i = 0; i < count; i++)
            {
                var testViewModel = vms[i];
                var newId = Guid.NewGuid().ToString("N");
                var oldId = testViewModel.Metadata.Get(ViewModelMetadata.Id);
                testViewModel.Metadata.Set(ViewModelMetadata.Id, newId);
                _viewModelManager.OnLifecycleChanged(testViewModel, ViewModelLifecycleState.Restored, i, DefaultMetadata);
                _viewModelManager.TryGetViewModel(oldId!).ShouldBeNull();
                _viewModelManager.TryGetViewModel(newId).ShouldEqual(testViewModel);
            }

            //disposed
            for (var i = 0; i < count; i++)
            {
                var testViewModel = vms[i];
                _viewModelManager.OnLifecycleChanged(testViewModel, ViewModelLifecycleState.Disposed, null, DefaultMetadata);
                _viewModelManager.TryGetViewModel(testViewModel.Metadata.Get(ViewModelMetadata.Id)!).ShouldBeNull();
            }

            if (!isWeak)
                return;
            for (var i = 0; i < count; i++)
            {
                var testViewModel = new TestViewModel();
                vms.Add(testViewModel);
                _viewModelManager.OnLifecycleChanged(testViewModel, ViewModelLifecycleState.Created, i, DefaultMetadata);
                _viewModelManager.TryGetViewModel(testViewModel.Metadata.Get(ViewModelMetadata.Id)!).ShouldEqual(testViewModel);
            }

            var array = vms.Select(model => model.Metadata.Get(ViewModelMetadata.Id)).ToArray();
            vms.Clear();
            GcCollect();
            for (var i = 0; i < count; i++)
                _viewModelManager.TryGetViewModel(array[i]!).ShouldBeNull();
        }
    }
}