﻿using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.UnitTests.Metadata;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Views;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views
{
    public class ViewTest : MetadataOwnerTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var mapping = new ViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var view = new object();
            var testViewModel = new TestViewModel();
            IView v = new View(mapping, view, testViewModel);
            v.Mapping.ShouldEqual(mapping);
            v.Target.ShouldEqual(view);
            v.ViewModel.ShouldEqual(testViewModel);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata) =>
            new View(new ViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this, new TestViewModel(), metadata);

        #endregion
    }
}