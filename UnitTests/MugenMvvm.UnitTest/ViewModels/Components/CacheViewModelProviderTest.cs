using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.ViewModels.Components
{
    public class CacheViewModelProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldIgnoreNonStringRequest()
        {
            var component = new CacheViewModelProvider();
            component.TryGetViewModel(null!, this, DefaultMetadata).ShouldBeNull();
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void ShouldTrackCreatedViewModels(int count, bool isWeak)
        {
            var manager = new ViewModelManager();
            var component = new CacheViewModelProvider(isWeak);
            manager.AddComponent(component);

            var vms = new List<TestViewModel>();
            //created
            for (var i = 0; i < count; i++)
            {
                var testViewModel = new TestViewModel();
                vms.Add(testViewModel);
                manager.TryGetViewModel(testViewModel.Metadata.Get(ViewModelMetadata.Id)!).ShouldBeNull();
                manager.OnLifecycleChanged(testViewModel, ViewModelLifecycleState.Created, i, DefaultMetadata);
                manager.TryGetViewModel(testViewModel.Metadata.Get(ViewModelMetadata.Id)!).ShouldEqual(testViewModel);
            }

            //restored
            for (var i = 0; i < count; i++)
            {
                var testViewModel = vms[i];
                var newId = Guid.NewGuid().ToString("N");
                var oldId = testViewModel.Metadata.Get(ViewModelMetadata.Id);
                testViewModel.Metadata.Set(ViewModelMetadata.Id, newId);
                manager.OnLifecycleChanged(testViewModel, ViewModelLifecycleState.Restored, i, DefaultMetadata);
                manager.TryGetViewModel(oldId!).ShouldBeNull();
                manager.TryGetViewModel(newId).ShouldEqual(testViewModel);
            }

            //disposed
            for (var i = 0; i < count; i++)
            {
                var testViewModel = vms[i];
                manager.OnLifecycleChanged(testViewModel, ViewModelLifecycleState.Disposed, null, DefaultMetadata);
                manager.TryGetViewModel(testViewModel.Metadata.Get(ViewModelMetadata.Id)!).ShouldBeNull();
            }

            if (!isWeak)
                return;
            for (var i = 0; i < count; i++)
            {
                var testViewModel = new TestViewModel();
                vms.Add(testViewModel);
                manager.OnLifecycleChanged(testViewModel, ViewModelLifecycleState.Created, i, DefaultMetadata);
                manager.TryGetViewModel(testViewModel.Metadata.Get(ViewModelMetadata.Id)!).ShouldEqual(testViewModel);
            }

            var array = vms.Select(model => model.Metadata.Get(ViewModelMetadata.Id)).ToArray();
            vms.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            for (var i = 0; i < count; i++)
                manager.TryGetViewModel(array[i]!).ShouldBeNull();
        }

        #endregion
    }
}