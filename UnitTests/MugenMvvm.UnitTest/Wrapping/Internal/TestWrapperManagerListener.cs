using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.UnitTest.Wrapping.Internal
{
    public class TestWrapperManagerListener : IWrapperManagerListener
    {
        #region Properties

        public Action<IWrapperManager, object, object, Type, IReadOnlyMetadataContext?>? OnWrapped { get; set; }

        #endregion

        #region Implementation of interfaces

        void IWrapperManagerListener.OnWrapped<TRequest>(IWrapperManager wrapperManager, object wrapper, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            OnWrapped?.Invoke(wrapperManager, wrapper, request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}