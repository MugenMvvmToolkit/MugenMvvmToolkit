using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.App.Components
{
    public interface IUnhandledExceptionHandlerComponent : IComponent<IMugenApplication>
    {
        void OnUnhandledException(IMugenApplication application, Exception exception, UnhandledExceptionType type, IReadOnlyMetadataContext? metadata);
    }
}