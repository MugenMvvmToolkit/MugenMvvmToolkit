using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.App
{
    public class TestUnhandledExceptionHandlerComponent : IUnhandledExceptionHandlerComponent, IHasPriority
    {
        public Action<IMugenApplication, Exception, UnhandledExceptionType, IReadOnlyMetadataContext?>? OnUnhandledException { get; set; }

        public int Priority { get; set; }

        void IUnhandledExceptionHandlerComponent.OnUnhandledException(IMugenApplication application, Exception exception, UnhandledExceptionType type,
            IReadOnlyMetadataContext? metadata) =>
            OnUnhandledException?.Invoke(application, exception, type, metadata);
    }
}