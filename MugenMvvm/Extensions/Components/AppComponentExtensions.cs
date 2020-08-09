using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class AppComponentExtensions
    {
        #region Methods

        public static void OnLifecycleChanged(this IApplicationLifecycleDispatcherComponent[] components, IMugenApplication application, ApplicationLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(application, nameof(application));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            for (var i = 0; i < components.Length; i++)
                components[i].OnLifecycleChanged(application, lifecycleState, state, metadata);
        }

        public static void OnUnhandledException(this IApplicationUnhandledExceptionComponent[] components, IMugenApplication application, Exception exception, UnhandledExceptionType type,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(application, nameof(application));
            Should.NotBeNull(exception, nameof(exception));
            for (var i = 0; i < components.Length; i++)
                components[i].OnUnhandledException(application, exception, type, metadata);
        }

        #endregion
    }
}