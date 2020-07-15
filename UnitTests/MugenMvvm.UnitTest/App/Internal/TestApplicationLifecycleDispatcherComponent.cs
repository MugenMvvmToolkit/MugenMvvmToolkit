using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.App.Internal
{
    public class TestApplicationLifecycleDispatcherComponent : IApplicationLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly IMugenApplication? _owner;

        #endregion

        #region Constructors

        public TestApplicationLifecycleDispatcherComponent(IMugenApplication? owner = null)
        {
            _owner = owner;
        }

        #endregion

        #region Properties

        public Action<ApplicationLifecycleState, object?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IApplicationLifecycleDispatcherComponent.OnLifecycleChanged(IMugenApplication application, ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            _owner?.ShouldEqual(application);
            OnLifecycleChanged?.Invoke(lifecycleState, state, metadata);
        }

        #endregion
    }
}