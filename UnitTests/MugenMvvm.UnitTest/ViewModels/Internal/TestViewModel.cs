using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;

namespace MugenMvvm.UnitTest.ViewModels.Internal
{
    public class TestViewModel : MetadataOwnerBase, IViewModelBase, IDisposable
    {
        #region Constructors

        public TestViewModel(IReadOnlyMetadataContext? metadata = null, IMetadataContextManager? metadataContextManager = null)
            : base(metadata, metadataContextManager)
        {
        }

        #endregion

        #region Properties

        public Action? Dispose { get; set; }

        #endregion

        #region Implementation of interfaces

        void IDisposable.Dispose()
        {
            Dispose?.Invoke();
        }

        #endregion
    }
}