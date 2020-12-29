using System;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.ViewModels;

namespace MugenMvvm.UnitTests.ViewModels.Internal
{
    public class TestViewModelBase : ViewModelBase
    {
        #region Constructors

        public TestViewModelBase(IViewModelManager? viewModelManager = null) : base(viewModelManager)
        {
        }

        #endregion

        #region Properties

        public Action<bool>? OnDisposeHandler { get; set; }

        public Action<IBusyManager, IBusyToken, IReadOnlyMetadataContext?>? OnBeginBusyHandler { get; set; }

        public Action<IBusyManager, IReadOnlyMetadataContext?>? OnBusyStateChangedHandler { get; set; }

        #endregion

        #region Methods

        public new T GetViewModel<T>(IReadOnlyMetadataContext? metadata = null) where T : IViewModelBase => base.GetViewModel<T>(metadata);

        public new IViewModelBase GetViewModel(Type viewModelType, IReadOnlyMetadataContext? metadata = null) => base.GetViewModel(viewModelType, metadata);

        protected override void OnBeginBusy(IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata)
        {
            OnBeginBusyHandler?.Invoke(busyManager, busyToken, metadata);
            base.OnBeginBusy(busyManager, busyToken, metadata);
        }

        protected override void OnBusyStateChanged(IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            OnBusyStateChangedHandler?.Invoke(busyManager, metadata);
            base.OnBusyStateChanged(busyManager, metadata);
        }

        protected override void OnDispose(bool disposing)
        {
            OnDisposeHandler?.Invoke(disposing);
            base.OnDispose(disposing);
        }

        #endregion
    }
}