﻿using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTest.Navigation.Internal
{
    public class TestNavigationEntryProviderComponent : INavigationEntryProviderComponent, IHasPriority
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;

        #endregion

        #region Constructors

        public TestNavigationEntryProviderComponent(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public Func<IReadOnlyMetadataContext?, ItemOrList<INavigationEntry, IReadOnlyList<INavigationEntry>>>? TryGetNavigationEntries { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<INavigationEntry, IReadOnlyList<INavigationEntry>> INavigationEntryProviderComponent.TryGetNavigationEntries(INavigationDispatcher navigationDispatcher, IReadOnlyMetadataContext? metadata)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            return TryGetNavigationEntries?.Invoke(metadata) ?? default;
        }

        #endregion
    }
}