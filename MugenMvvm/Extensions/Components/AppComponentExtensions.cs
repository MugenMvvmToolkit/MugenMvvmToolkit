using System;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class AppComponentExtensions
    {
        public static void OnLifecycleChanged(this ItemOrArray<IApplicationLifecycleListener> components, IMugenApplication application, ApplicationLifecycleState lifecycleState,
            object? state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(application, nameof(application));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            foreach (var c in components)
                c.OnLifecycleChanged(application, lifecycleState, state, metadata);
        }

        public static void OnUnhandledException(this ItemOrArray<IUnhandledExceptionHandlerComponent> components, IMugenApplication application, Exception exception,
            UnhandledExceptionType type,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(application, nameof(application));
            Should.NotBeNull(exception, nameof(exception));
            foreach (var c in components)
                c.OnUnhandledException(application, exception, type, metadata);
        }
    }
}