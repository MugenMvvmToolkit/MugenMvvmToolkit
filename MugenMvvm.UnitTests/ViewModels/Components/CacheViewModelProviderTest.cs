using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.ViewModels.Components
{
    public class CacheViewModelProviderTest : UnitTestBase
    {
        public CacheViewModelProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            ViewModelManager.AddComponent(new CacheViewModelProvider(true, WeakReferenceManager));
        }

        [Fact]
        public void ShouldIgnoreNonStringRequest() => ViewModelManager.TryGetViewModel(this, DefaultMetadata).ShouldBeNull();

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void ShouldTrackCreatedViewModels(int count, bool isWeak)
        {
            ViewModelManager.RemoveComponents<CacheViewModelProvider>();
            var component = new CacheViewModelProvider(isWeak, WeakReferenceManager);
            ViewModelManager.AddComponent(component);

            var vms = new List<TestViewModel>();
            //created
            for (var i = 0; i < count; i++)
            {
                var testViewModel = new TestViewModel();
                vms.Add(testViewModel);
                ViewModelManager.TryGetViewModel(testViewModel.Metadata.Get(ViewModelMetadata.Id)!).ShouldBeNull();
                ViewModelManager.OnLifecycleChanged(testViewModel, ViewModelLifecycleState.Created, i, DefaultMetadata);
                ViewModelManager.TryGetViewModel(testViewModel.Metadata.Get(ViewModelMetadata.Id)!).ShouldEqual(testViewModel);
            }

            //restored
            for (var i = 0; i < count; i++)
            {
                var testViewModel = vms[i];
                var newId = Guid.NewGuid().ToString("N");
                var oldId = testViewModel.Metadata.Get(ViewModelMetadata.Id);
                testViewModel.Metadata.Set(ViewModelMetadata.Id, newId);
                ViewModelManager.OnLifecycleChanged(testViewModel, ViewModelLifecycleState.Restored, i, DefaultMetadata);
                ViewModelManager.TryGetViewModel(oldId!).ShouldBeNull();
                ViewModelManager.TryGetViewModel(newId).ShouldEqual(testViewModel);
            }

            //disposed
            for (var i = 0; i < count; i++)
            {
                var testViewModel = vms[i];
                ViewModelManager.OnLifecycleChanged(testViewModel, ViewModelLifecycleState.Disposed, null, DefaultMetadata);
                ViewModelManager.TryGetViewModel(testViewModel.Metadata.Get(ViewModelMetadata.Id)!).ShouldBeNull();
            }

            if (!isWeak)
                return;
            for (var i = 0; i < count; i++)
            {
                var testViewModel = new TestViewModel();
                vms.Add(testViewModel);
                ViewModelManager.OnLifecycleChanged(testViewModel, ViewModelLifecycleState.Created, i, DefaultMetadata);
                ViewModelManager.TryGetViewModel(testViewModel.Metadata.Get(ViewModelMetadata.Id)!).ShouldEqual(testViewModel);
            }

            var array = vms.Select(model => model.Metadata.Get(ViewModelMetadata.Id)).ToArray();
            vms.Clear();
            GcCollect();
            for (var i = 0; i < count; i++)
                ViewModelManager.TryGetViewModel(array[i]!).ShouldBeNull();
        }
    }
}