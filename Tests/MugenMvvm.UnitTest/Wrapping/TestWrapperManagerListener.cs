using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.UnitTest.Wrapping
{
    public class TestWrapperManagerListener : IWrapperManagerListener
    {
        #region Properties

        public Action<IWrapperManager, object, object, Type, IReadOnlyMetadataContext?>? OnWrapped { get; set; }

        #endregion

        #region Implementation of interfaces

        void IWrapperManagerListener.OnWrapped(IWrapperManager wrapperManager, object wrapper, object item, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            OnWrapped?.Invoke(wrapperManager, wrapper, item, wrapperType, metadata);
        }

        #endregion
    }
}