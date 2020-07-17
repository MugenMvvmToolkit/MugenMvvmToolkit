﻿using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTest.Views.Internal
{
    public class TestViewMappingProviderComponent : IViewMappingProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IViewManager? _viewManager;

        #endregion

        #region Constructors

        public TestViewMappingProviderComponent(IViewManager? viewManager = null)
        {
            _viewManager = viewManager;
        }

        #endregion

        #region Properties

        public Func<object, IReadOnlyMetadataContext?, ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>>>? TryGetMappings { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>> IViewMappingProviderComponent.TryGetMappings(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            _viewManager?.ShouldEqual(viewManager);
            return TryGetMappings?.Invoke(request!, metadata) ?? default;
        }

        #endregion
    }
}