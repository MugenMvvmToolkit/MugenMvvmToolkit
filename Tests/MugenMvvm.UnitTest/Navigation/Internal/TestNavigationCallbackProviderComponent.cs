using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.UnitTest.Navigation.Internal
{
    public class TestNavigationCallbackProviderComponent : INavigationCallbackProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<object, Type, IReadOnlyMetadataContext?, IReadOnlyList<INavigationCallback>?>? TryGetCallbacks { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyList<INavigationCallback>? INavigationCallbackProviderComponent.TryGetNavigationCallbacks<TTarget>(in TTarget target, IReadOnlyMetadataContext? metadata)
        {
            return TryGetCallbacks?.Invoke(target!, typeof(TTarget), metadata);
        }

        #endregion
    }
}