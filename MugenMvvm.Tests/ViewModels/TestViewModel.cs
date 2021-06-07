using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;

namespace MugenMvvm.Tests.ViewModels
{
    public class TestViewModel : MetadataOwnerBase, IViewModelBase, IDisposable
    {
        public TestViewModel(IReadOnlyMetadataContext? metadata = null)
            : base(metadata)
        {
        }

        public Action? Dispose { get; set; }

        void IDisposable.Dispose() => Dispose?.Invoke();
    }
}