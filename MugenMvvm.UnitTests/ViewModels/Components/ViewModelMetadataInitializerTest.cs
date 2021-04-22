﻿using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.ViewModels.Components
{
    public class ViewModelMetadataInitializerTest : UnitTestBase
    {
        private readonly IViewModelManager _viewModelManager;
        private readonly ViewModelMetadataInitializer _initializer;
        private readonly TestViewModel _viewModel;

        public ViewModelMetadataInitializerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModelManager = new ViewModelManager(ComponentCollectionManager);
            _initializer = new ViewModelMetadataInitializer();
            _viewModelManager.AddComponent(_initializer);
            _viewModel = new TestViewModel();
        }

        [Fact]
        public void ShouldMergeMetadataCreatedLifecycle()
        {
            var k1 = MetadataContextKey.FromKey<int>(NewId());
            var k2 = MetadataContextKey.FromKey<string>(NewId());
            _initializer.MetadataMergeKeys.Clear();
            _initializer.MetadataMergeKeys.Add(k1);
            _initializer.MetadataMergeKeys.Add(k2);

            var metadata = new MetadataContext {{k1, 1}, {k2, k2.Id}};
            _viewModelManager.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Created, null, metadata);
            _viewModel.Metadata.Count.ShouldEqual(2);
            _viewModel.Metadata.Get(k1).ShouldEqual(metadata.Get(k1));
            _viewModel.Metadata.Get(k2).ShouldEqual(metadata.Get(k2));
        }

        [Fact]
        public void ShouldNotdMergeMetadataNotCreatedLifecycle()
        {
            var k1 = MetadataContextKey.FromKey<int>(NewId());
            var k2 = MetadataContextKey.FromKey<string>(NewId());
            _initializer.MetadataMergeKeys.Clear();
            _initializer.MetadataMergeKeys.Add(k1);
            _initializer.MetadataMergeKeys.Add(k2);

            var metadata = new MetadataContext {{k1, 1}, {k2, k2.Id}};
            _viewModelManager.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Initialized, null, metadata);
            _viewModel.Metadata.Count.ShouldEqual(0);
        }
    }
}