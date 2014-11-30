using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure
{
    public class MappingViewModel : ViewModelBase
    {
    }


    public class MappingView
    {

    }

    [ViewModel(typeof(MappingViewModel))]
    public class MappingViewWithAttribute
    {

    }

    [ViewModel(typeof(MappingViewModel))]
    public class MappingViewWithAttributeDuplicate
    {

    }

    [ViewModel(typeof(MappingViewModel), ViewMappingProviderTest.VmName)]
    public class MappingViewWithName
    {

    }

    [ViewModel(typeof(MappingViewModel), Uri = ViewMappingProviderTest.TestUrl, UriKind = UriKind.Absolute)]
    public class MappingViewWithUri
    {

    }

    [ViewModel(typeof(GridViewModel<>))]
    public class MappingViewGeneric
    {
    }

    [TestClass]
    public class ViewMappingProviderTest : TestBase
    {
        #region Nested types

        private sealed class ViewMappingProviderEx : ViewMappingProvider
        {
            public ViewMappingProviderEx(IList<Type> types)
                : base(Empty.Array<Assembly>())
            {
                InitializeMapping(types);
            }
        }

        #endregion

        #region Fields

        public const string TestUrl = "http://test.com/";
        public const string VmName = "VmName";

        #endregion

        #region Methods

        [TestMethod]
        public void VmProviderShouldReturnMappingWithoutNameBasedOnNamedConvention()
        {
            IViewMappingProvider viewMappingProvider = GetViewMappingProvider(typeof(MappingViewModel),
                typeof(MappingView));
            IViewMappingItem viewMap = viewMappingProvider.FindMappingForViewModel(typeof(MappingViewModel), null, true);
            viewMap.Name.ShouldBeNull();
            viewMap.ViewModelType.ShouldEqual(typeof(MappingViewModel));
            viewMap.ViewType.ShouldEqual(typeof(MappingView));
        }

        [TestMethod]
        public void VmProviderShouldReturnMappingWithoutNameUsingAttribute()
        {
            IViewMappingProvider viewMappingProvider = GetViewMappingProvider(typeof(MappingViewModel),
                typeof(MappingView), typeof(MappingViewWithAttribute));
            IViewMappingItem viewMap = viewMappingProvider.FindMappingForViewModel(typeof(MappingViewModel), null, true);
            viewMap.Name.ShouldBeNull();
            viewMap.ViewModelType.ShouldEqual(typeof(MappingViewModel));
            viewMap.ViewType.ShouldEqual(typeof(MappingViewWithAttribute));
        }

        [TestMethod]
        public void VmProviderShouldReturnMappingWithNameUsingAttribute()
        {
            IViewMappingProvider viewMappingProvider = GetViewMappingProvider(typeof(MappingViewModel),
                typeof(MappingView), typeof(MappingViewWithAttribute), typeof(MappingViewWithName));
            IViewMappingItem viewMap = viewMappingProvider.FindMappingForViewModel(typeof(MappingViewModel), VmName, true);
            viewMap.Name.ShouldEqual(VmName);
            viewMap.ViewModelType.ShouldEqual(typeof(MappingViewModel));
            viewMap.ViewType.ShouldEqual(typeof(MappingViewWithName));
        }

        [TestMethod]
        public void VmProviderShouldReturnMappingWithUriUsingAttribute()
        {
            IViewMappingProvider viewMappingProvider = GetViewMappingProvider(typeof(MappingViewModel),
                typeof(MappingView), typeof(MappingViewWithUri));
            IViewMappingItem viewMap = viewMappingProvider.FindMappingForViewModel(typeof(MappingViewModel), null, true);
            viewMap.Name.ShouldBeNull();
            viewMap.Uri.ShouldEqual(new Uri(TestUrl, UriKind.Absolute));
            viewMap.ViewModelType.ShouldEqual(typeof(MappingViewModel));
            viewMap.ViewType.ShouldEqual(typeof(MappingViewWithUri));
        }

        [TestMethod]
        public void VmProviderShouldResolveGenericVm()
        {
            IViewMappingProvider viewMappingProvider = GetViewMappingProvider(typeof(MappingViewGeneric));
            IViewMappingItem mappingItem = viewMappingProvider.FindMappingForViewModel(typeof(GridViewModel<object>), null, true);
            mappingItem.ViewModelType.ShouldEqual(typeof(GridViewModel<>));
            mappingItem.ViewType.ShouldEqual(typeof(MappingViewGeneric));
            mappingItem.Name.ShouldBeNull();
        }

        [TestMethod]
        public void VmProviderShouldThrowExceptionOnDuplicateMapping()
        {
            ShouldThrow<InvalidOperationException>(() => GetViewMappingProvider(typeof(MappingViewWithAttribute), typeof(MappingViewWithAttributeDuplicate)));
        }

        [TestMethod]
        public void VmProviderShouldThrowExceptionIfMappingNotFoundUsingViewTrue()
        {
            var provider = GetViewMappingProvider();
            ShouldThrow<InvalidOperationException>(() => provider.FindMappingsForView(typeof(object), true));
        }

        [TestMethod]
        public void VmProviderShouldNotThrowExceptionIfMappingNotFoundUsingViewFalse()
        {
            var provider = GetViewMappingProvider();
            provider.FindMappingsForView(typeof(object), false).ShouldBeEmpty();
        }

        [TestMethod]
        public void PageProviderShouldThrowExceptionIfMappingNotFoundUsingViewModelTrue()
        {
            var provider = GetViewMappingProvider();
            ShouldThrow<InvalidOperationException>(() => provider.FindMappingForViewModel(typeof(IViewModel), null, true));
        }

        [TestMethod]
        public void PageProviderShouldNotThrowExceptionIfMappingNotFoundUsingViewModelFalse()
        {
            var provider = GetViewMappingProvider();
            provider.FindMappingForViewModel(typeof(IViewModel), null, false).ShouldBeNull();
        }

        protected virtual IViewMappingProvider GetViewMappingProvider(params Type[] types)
        {
            return new ViewMappingProviderEx(types);
        }

        #endregion
    }
}