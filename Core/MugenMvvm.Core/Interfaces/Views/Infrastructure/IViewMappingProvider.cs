//using System;
//using System.Collections.Generic;
//using MugenMvvm.Interfaces.Metadata;
//
//namespace MugenMvvm.Interfaces.Views.Infrastructure
//{
//    public interface IViewMappingProvider//todo rewrite
//    {
//        IEnumerable<IViewMappingInfo> Mappings { get; }
//
//        IReadOnlyCollection<IViewMappingInfo>? TryGetMappingsByView(Type viewType, IReadOnlyMetadataContext metadata);
//
//        IReadOnlyCollection<IViewMappingInfo>? TryGetMappingsByViewModel(Type viewModelType, IReadOnlyMetadataContext metadata);
//
//        IViewMappingInfo? TryGetMappingByViewModel(Type viewModelType, IReadOnlyMetadataContext metadata);
//    }
//}