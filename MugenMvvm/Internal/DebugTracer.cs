#if !ANDROID
#define TRACE
#endif
#if ANDROID
using Android.Util;
#else
using System.Diagnostics;
#endif
using System;
using System.Text;
using System.Threading;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Presentation.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal.Components;
using MugenMvvm.Messaging;
using MugenMvvm.Views;

namespace MugenMvvm.Internal
{
    public static class DebugTracer
    {
        private const string NavigationTag = "Navigation: ";
        private const string BindingTag = "Binding: ";
        private const string ViewTag = "View: ";
        private const string ViewModelTag = "ViewModel: ";
        private const string PresentationTag = "Presentation: ";
        private const string ApplicationTag = "Application: ";
        private const string MessagingTag = "Messaging: ";
        private static ILogger? _logger;

        private static ILogger? Logger => _logger ??= MugenService.Optional<ILogger>()?.GetLogger(typeof(DebugTracer));

        public static void AddTraceLogger(ILogger logger) =>
            logger.AddComponent(new DelegateLogger((level, s, e, _) =>
            {
#if ANDROID
                const string tag = "MugenMvvm";
                if (e != null)
                    s += Environment.NewLine + e.Flatten(true);
                if (level == LogLevel.Error || level == LogLevel.Fatal)
                    Log.Error(tag, s.ToString());
                else if (level == LogLevel.Warning)
                    Log.Warn(tag, s.ToString());
                else
                    Log.Info(tag, s.ToString());
#else
                s = $"{level.Name}/{s}";
                if (e != null)
                    s += Environment.NewLine + e.Flatten(true);
                Trace.WriteLine(s.ToString());
#endif
            }, (_, _) => true));

        public static void TraceApp(IMugenApplication application) => application.AddComponent(new ApplicationTracer());

        public static void TraceBindings(IBindingManager bindingManager) => bindingManager.AddComponent(new GlobalBindingTracer());

        public static void TraceNavigation(INavigationDispatcher navigationDispatcher)
        {
            navigationDispatcher.AddComponent(new NavigatingErrorListener());
            navigationDispatcher.AddComponent(new NavigatedTracer());
        }

        public static void TraceMessenger(IMessenger messenger) => messenger.AddComponent(new MessengerTracer());

        public static void TracePresenter(IPresenter presenter) => presenter.AddComponent(new PresenterTracer());

        public static void TraceViewModel(IViewModelManager viewModelManager) => viewModelManager.AddComponent(new ViewModelTracer());

        public static void TraceView(IViewManager viewManager)
        {
            viewManager.AddComponent(new ViewTracer());
            viewManager.AddComponent(new ViewMappingTracer());
        }

        private static string Dump(INavigationDispatcher navigationDispatcher, INavigationContext context) =>
            $"mode={context.NavigationMode}, type={context.NavigationType}, target={context.Target}, provider={context.NavigationProvider}, metadata={context.GetMetadataOrDefault().Dump()}, prevTarget={GetPrevNavigationTarget(navigationDispatcher, context)}, id={context.NavigationId}";

        private static string Dump(object subscriber, IMessageContext context)
        {
            if (subscriber is IWeakReference w)
                subscriber = w.Target!;
            return $"sender={context.Sender}, receiver={subscriber}, message={context.Message}, metadata={context.GetMetadataOrDefault().Dump()}";
        }

        private static string Dump(ItemOrIReadOnlyList<IPresenterResult> results)
        {
            if (results.Count == 0)
                return " result=empty";

            var builder = new StringBuilder();
            var count = 0;
            foreach (var result in results)
            {
                string r = results.Count == 1 ? ", result=" : $", result_{count++}=";
                builder.Append(
                    $"{r}(type={result.NavigationType}, id={result.NavigationId}, target={result.Target}, provider={result.NavigationProvider}, metadata={result.GetMetadataOrDefault().Dump()}, raw={result})");
            }

            return builder.ToString();
        }

        private static string Dump(ItemOrIReadOnlyList<IViewMapping> results)
        {
            if (results.Count == 0)
                return " result=empty";

            var builder = new StringBuilder();
            var count = 0;
            foreach (var result in results)
            {
                string r = results.Count == 1 ? ", result=" : $", result_{count++}=";
                builder.Append(
                    $"{r}(viewmodel={result.ViewModelType}, view={result.ViewType}, metadata={result.GetMetadataOrDefault().Dump()}, raw={result})");
            }

            return builder.ToString();
        }

        private static string Dump(IBinding binding) => $"target={binding.Target.Target}, targetPath={binding.Target.Path.Path}{Dump(binding.Source)}, state={binding.State}";

        private static string Dump(ItemOrIReadOnlyList<object?> source)
        {
            if (source.IsEmpty)
                return ", source=null";
            if (source.Item != null)
                return $", source={GetTarget(source.Item)}";
            var builder = new StringBuilder();
            for (var i = 0; i < source.List!.Count; i++)
                builder.Append($", source_{i}={GetTarget(source.List![i])}");
            return builder.ToString();
        }

        private static string GetTarget(object? source)
        {
            if (source is IMemberPathObserver pathObserver)
                return $"{pathObserver.Target ?? InternalConstant.Null}, path={pathObserver.Path.Path}";
            return source?.ToString() ?? InternalConstant.Null;
        }

        private static object? GetPrevNavigationTarget(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext) =>
            navigationDispatcher.GetTopNavigation(navigationContext, (entry, context, _) =>
            {
                if (entry.NavigationType == context.NavigationType && entry.Target != null && !Equals(entry.Target, context.Target))
                    return entry.Target;
                return null;
            }) ?? navigationDispatcher.GetTopNavigation(navigationContext, (entry, context, _) =>
            {
                if (entry.Target != null && !Equals(entry.Target, context.Target))
                    return entry.Target;
                return null;
            });

        private sealed class GlobalBindingTracer : IBindingTargetListener, IBindingSourceListener, IBindingExpressionInitializerComponent
        {
            public void Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
            {
                context.Components["Debug"] = this;
                var tag = context.TryGetParameterValue<string?>(BindingParameterNameConstant.Trace);
                if (!string.IsNullOrEmpty(tag))
                    context.Components[BindingParameterNameConstant.Trace] = new BindingTracer(tag!);
            }

            public void OnSourceUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata) =>
                Logger.Error()?.Log($"{BindingTag}({Dump(binding)}) source update failed", exception);

            public void OnSourceUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata)
            {
            }

            public void OnSourceUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata)
            {
            }

            public void OnTargetUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata) =>
                Logger.Error()?.Log($"{BindingTag}({Dump(binding)}) target update failed", exception);

            public void OnTargetUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata)
            {
            }

            public void OnTargetUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata)
            {
            }
        }

        private sealed class BindingTracer : IBindingTargetListener, IBindingSourceListener
        {
            private readonly string _tag;

            public BindingTracer(string tag)
            {
                _tag = tag;
            }

            public void OnSourceUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata)
            {
            }

            public void OnSourceUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata) => Logger.Trace()?.Log($"{_tag}: ({Dump(binding)}) source update canceled");

            public void OnSourceUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata) =>
                Logger.Trace()?.Log($"{_tag}: ({Dump(binding)}) source updated newValue={newValue}");

            public void OnTargetUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata)
            {
            }

            public void OnTargetUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata) => Logger.Trace()?.Log($"{_tag}: ({Dump(binding)}) target update canceled");

            public void OnTargetUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata) =>
                Logger.Trace()?.Log($"{_tag}: ({Dump(binding)}) target updated newValue={newValue}");
        }

        private sealed class ApplicationTracer : ComponentDecoratorBase<IMugenApplication, IApplicationLifecycleListener>, IApplicationLifecycleListener,
            IUnhandledExceptionHandlerComponent
        {
            public ApplicationTracer() : base(ComponentPriority.Max)
            {
            }

            public void OnLifecycleChanged(IMugenApplication application, ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
            {
                Logger.Trace()?.Log($"{ApplicationTag}before ({lifecycleState}) state={state ?? InternalConstant.Null}, metadata={metadata.Dump()}");
                Components.OnLifecycleChanged(application, lifecycleState, state, metadata);
                Logger.Trace()?.Log($"{ApplicationTag}after ({lifecycleState}) state={state ?? InternalConstant.Null}, metadata={metadata.Dump()}");
            }

            public void OnUnhandledException(IMugenApplication application, Exception exception, UnhandledExceptionType type, IReadOnlyMetadataContext? metadata) =>
                Logger.Error()?.Log($"{ApplicationTag}unhandled exception ({type})", exception);
        }

        private sealed class NavigatingErrorListener : ComponentDecoratorBase<INavigationDispatcher, INavigationErrorListener>, INavigationErrorListener
        {
            public NavigatingErrorListener() : base(ComponentPriority.Max)
            {
            }

            public void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
            {
                Logger.Trace()?.Log($"{NavigationTag}before failed {Dump(navigationDispatcher, navigationContext)}");
                Components.OnNavigationFailed(navigationDispatcher, navigationContext, exception);
                Logger.Trace()?.Log($"{NavigationTag}after failed {Dump(navigationDispatcher, navigationContext)}");
            }

            public void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
            {
                Logger.Trace()?.Log($"{NavigationTag}before canceled {Dump(navigationDispatcher, navigationContext)}");
                Components.OnNavigationCanceled(navigationDispatcher, navigationContext, cancellationToken);
                Logger.Trace()?.Log($"{NavigationTag}after canceled {Dump(navigationDispatcher, navigationContext)}");
            }
        }

        private sealed class NavigatedTracer : ComponentDecoratorBase<INavigationDispatcher, INavigationListener>, INavigationListener
        {
            public NavigatedTracer() : base(ComponentPriority.Max)
            {
            }

            public void OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                Logger.Trace()?.Log($"{NavigationTag}before navigating {Dump(navigationDispatcher, navigationContext)}");
                Components.OnNavigating(navigationDispatcher, navigationContext);
                Logger.Trace()?.Log($"{NavigationTag}after navigating {Dump(navigationDispatcher, navigationContext)}");
            }

            public void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                Logger.Trace()?.Log($"{NavigationTag}before navigated {Dump(navigationDispatcher, navigationContext)}");
                Components.OnNavigated(navigationDispatcher, navigationContext);
                Logger.Trace()?.Log($"{NavigationTag}after navigated {Dump(navigationDispatcher, navigationContext)}");
            }
        }

        private sealed class MessengerTracer : ComponentDecoratorBase<IMessenger, IMessengerSubscriberComponent>, IMessengerSubscriberComponent
        {
            public MessengerTracer() : base(ComponentPriority.Max)
            {
            }

            public bool TrySubscribe(IMessenger messenger, object subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata)
                => Components.TrySubscribe(messenger, subscriber, executionMode, metadata);

            public bool TryUnsubscribe(IMessenger messenger, object subscriber, IReadOnlyMetadataContext? metadata)
                => Components.TryUnsubscribe(messenger, subscriber, metadata);

            public bool TryUnsubscribeAll(IMessenger messenger, IReadOnlyMetadataContext? metadata)
                => Components.TryUnsubscribeAll(messenger, metadata);

            public bool HasSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata) => Components.HasSubscribers(messenger, metadata);

            public ItemOrIReadOnlyList<MessengerHandler> TryGetMessengerHandlers(IMessenger messenger, Type messageType, IReadOnlyMetadataContext? metadata)
            {
                var editor = new ItemOrListEditor<MessengerHandler>();
                var handlers = Components.TryGetMessengerHandlers(messenger, messageType, metadata);
                foreach (var messengerHandler in handlers)
                {
                    messengerHandler.Deconstruct(out var subscriber, out var mode, out var handler, out var state);
                    editor.Add(new MessengerHandler((o, context, s) =>
                    {
                        Logger.Trace()?.Log($"{MessagingTag}handling message {Dump(o, context)}");
                        var result = handler!(o, context, s);
                        Logger.Trace()?.Log($"{MessagingTag}handled message result={result}, {Dump(o, context)}");
                        return result;
                    }, subscriber!, mode, state));
                }

                return editor.ToItemOrList();
            }

            public ItemOrIReadOnlyList<MessengerSubscriberInfo> TryGetSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata)
                => Components.TryGetSubscribers(messenger, metadata);
        }

        private sealed class PresenterTracer : ComponentDecoratorBase<IPresenter, IPresenterComponent>, IPresenterComponent
        {
            public PresenterTracer() : base(ComponentPriority.Max)
            {
            }

            public ItemOrIReadOnlyList<IPresenterResult> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            {
                Logger.Trace()?.Log($"{PresentationTag}showing request={request}, metadata={metadata.Dump()}");
                var result = Components.TryShow(presenter, request, cancellationToken, metadata);
                Logger.Trace()?.Log($"{PresentationTag}shown request={request}{Dump(result)}, metadata={metadata.Dump()}");
                return result;
            }

            public ItemOrIReadOnlyList<IPresenterResult> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            {
                Logger.Trace()?.Log($"{PresentationTag}closing request={request}, metadata={metadata.Dump()}");
                var result = Components.TryClose(presenter, request, cancellationToken, metadata);
                Logger.Trace()?.Log($"{PresentationTag}closed request={request}{Dump(result)}, metadata={metadata.Dump()}");
                return result;
            }
        }

        private sealed class ViewModelTracer : ComponentDecoratorBase<IViewModelManager, IViewModelLifecycleListener>, IViewModelLifecycleListener
        {
            private int _createdCount;
            private int _disposedCount;
            private int _finalizedCount;

            public ViewModelTracer() : base(ComponentPriority.Max)
            {
            }

            public void OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state,
                IReadOnlyMetadataContext? metadata)
            {
                Logger.Trace()?.Log($"{ViewModelTag}before ({lifecycleState}) viewmodel={viewModel}, state={state ?? InternalConstant.Null}, metadata={metadata.Dump()}");
                Components.OnLifecycleChanged(viewModelManager, viewModel, lifecycleState, state, metadata);
                Logger.Trace()?.Log($"{ViewModelTag}after ({lifecycleState}) viewmodel={viewModel}, state={state ?? InternalConstant.Null}, metadata={metadata.Dump()}");
                if (lifecycleState.BaseState == ViewModelLifecycleState.Created)
                    ++_createdCount;
                else if (lifecycleState.BaseState == ViewModelLifecycleState.Disposed)
                    ++_disposedCount;
                else if (lifecycleState.BaseState == ViewModelLifecycleState.Finalized)
                    ++_finalizedCount;
                Logger.Trace()?.Log($"{ViewModelTag}created={_createdCount}, disposed={_disposedCount}, finalized={_finalizedCount}");
            }
        }

        private sealed class ViewMappingTracer : ComponentDecoratorBase<IViewManager, IViewMappingProviderComponent>, IViewMappingProviderComponent
        {
            public ViewMappingTracer() : base(ComponentPriority.Max)
            {
            }

            public ItemOrIReadOnlyList<IViewMapping> TryGetMappings(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
            {
                var mappings = Components.TryGetMappings(viewManager, request, metadata);
                var vm = MugenExtensions.TryGetViewModelView(request, out object? view);
                Logger.Trace()?.Log($"{ViewTag}found mappings for ({request}, viewmodel={vm}, view={view}) {Dump(mappings)}, metadata={metadata.Dump()}");
                return mappings;
            }
        }

        private sealed class ViewTracer : ComponentDecoratorBase<IViewManager, IViewLifecycleListener>, IViewLifecycleListener
        {
            public ViewTracer() : base(ComponentPriority.Max)
            {
            }

            public void OnLifecycleChanged(IViewManager viewManager, ViewInfo view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
            {
                Logger.Trace()?.Log($"{ViewTag}before ({lifecycleState}) {Dump(viewManager, view)}, state={state ?? InternalConstant.Null}, metadata={metadata.Dump()}");
                Components.OnLifecycleChanged(viewManager, view, lifecycleState, state, metadata);
                Logger.Trace()?.Log($"{ViewTag}after ({lifecycleState}) {Dump(viewManager, view)}, state={state ?? InternalConstant.Null}, metadata={metadata.Dump()}");
            }

            private static string Dump(IViewManager viewManager, object view)
            {
                var views = viewManager.GetViews(view);
                if (views.Count == 0)
                    return $"view={view}";
                if (views.Count == 1)
                    return $"view={views.Item}, viewmodel={views.Item!.ViewModel}";
                var stringBuilder = new StringBuilder();
                var count = 0;
                foreach (var v in views)
                    stringBuilder.Append($"view_{count++}={v.Target}, viewmodel={v.ViewModel}; ");
                return stringBuilder.ToString();
            }
        }
    }
}