using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
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
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal.Components;
using MugenMvvm.Messaging;

namespace MugenMvvm.Internal
{
    public static class DebugTracer
    {
        #region Fields

        private const string NavigationTag = "Navigation: ";
        private const string BindingTag = "Binding: ";
        private const string ViewTag = "View: ";
        private const string ViewModelTag = "ViewModel: ";
        private const string PresentationTag = "Presentation: ";
        private const string ApplicationTag = "Application: ";
        private const string MessagingTag = "Messaging: ";

        #endregion

        #region Methods

        public static void AddConsoleTracer(ITracer tracer) =>
            tracer.AddComponent(new DelegateTracer((level, s, arg3, arg4) => Console.Out.WriteLine($"{level.Name}: {s} {arg3?.Flatten()}"), (level, context) => true));

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

        public static void TraceView(IViewManager viewManager) => viewManager.AddComponent(new ViewTracer());

        private static string Dump(INavigationDispatcher navigationDispatcher, INavigationContext context) =>
            $"mode={context.NavigationMode}, type={context.NavigationType}, target={context.Target}, provider={context.NavigationProvider}, metadata={context.GetMetadataOrDefault().Dump()}, prevTarget={GetPrevNavigationTarget(navigationDispatcher, context)}, id={context.NavigationId}";

        private static string Dump(object subscriber, IMessageContext context)
        {
            if (subscriber is IWeakReference w)
                subscriber = w.Target!;
            return $"sender={context.Sender}, receiver={subscriber}, message={context.Message}, metadata={context.GetMetadataOrDefault().Dump()}";
        }

        private static string Dump(ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> results)
        {
            if (results.Count == 0)
                return " result=empty";

            var builder = new StringBuilder();
            var count = 0;
            foreach (var result in results)
            {
                string r = results.Count == 1 ? ", result=" : $", result_{count++}=";
                builder.Append(
                    $"{r}(type={result.NavigationType}, id={result.NavigationId}, target={result.Target}, provider={result.NavigationProvider}, metadata={result.GetMetadataOrDefault().Dump()})");
            }

            return builder.ToString();
        }

        private static string Dump(IBinding binding) => $"target={binding.Target.Target}, targetPath={binding.Target.Path.Path}{Dump(binding.Source)}, state={binding.State}";

        private static string Dump(ItemOrList<object?, object?[]> source)
        {
            var list = source.List;
            if (source.Item == null && list == null)
                return ", source=null";
            if (source.Item != null)
                return $", source={GetTarget(source.Item)}";
            var builder = new StringBuilder();
            for (var i = 0; i < list!.Length; i++)
                builder.Append($", source_{i}={GetTarget(list[i])}");
            return builder.ToString();
        }

        private static string GetTarget(object? source)
        {
            if (source is IMemberPathObserver pathObserver)
                return $"{pathObserver.Target ?? "null"}, path={pathObserver.Path.Path}";
            return source?.ToString() ?? "null";
        }

        private static object? GetPrevNavigationTarget(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext) =>
            navigationDispatcher.GetTopNavigation(navigationContext, (entry, context, m) =>
            {
                if (entry.NavigationType == context.NavigationType && entry.Target != null && !Equals(entry.Target, context.Target))
                    return entry.Target;
                return null;
            }) ?? navigationDispatcher.GetTopNavigation(navigationContext, (entry, context, m) =>
            {
                if (entry.Target != null && !Equals(entry.Target, context.Target))
                    return entry.Target;
                return null;
            });

        #endregion

        #region Nested types

        private sealed class GlobalBindingTracer : IBindingTargetListener, IBindingSourceListener, IBindingExpressionInitializerComponent
        {
            #region Implementation of interfaces

            public void Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
            {
                context.BindingComponents["Debug"] = this;
                var tag = context.TryGetParameterValue<string?>(BindingParameterNameConstant.Trace);
                if (!string.IsNullOrEmpty(tag))
                    context.BindingComponents[BindingParameterNameConstant.Trace] = new BindingTracer(tag!);
            }

            public void OnSourceUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata) =>
                Tracer.Info()?.Trace($"{BindingTag}({Dump(binding)}) source update failed: {exception.Flatten(true)}");

            public void OnSourceUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata)
            {
            }

            public void OnSourceUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata)
            {
            }

            public void OnTargetUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata) =>
                Tracer.Info()?.Trace($"{BindingTag}({Dump(binding)}) target update failed: {exception.Flatten(true)}");

            public void OnTargetUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata)
            {
            }

            public void OnTargetUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata)
            {
            }

            #endregion
        }

        private sealed class BindingTracer : IBindingTargetListener, IBindingSourceListener
        {
            #region Fields

            private readonly string _tag;

            #endregion

            #region Constructors

            public BindingTracer(string tag)
            {
                _tag = tag;
            }

            #endregion

            #region Implementation of interfaces

            public void OnSourceUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata)
            {
            }

            public void OnSourceUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata) => Tracer.Info()?.Trace($"{_tag}: ({Dump(binding)}) source update canceled");

            public void OnSourceUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata) => Tracer.Info()?.Trace($"{_tag}: ({Dump(binding)}) source updated newValue={newValue}");

            public void OnTargetUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata)
            {
            }

            public void OnTargetUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata) => Tracer.Info()?.Trace($"{_tag}: ({Dump(binding)}) target update canceled");

            public void OnTargetUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata) => Tracer.Info()?.Trace($"{_tag}: ({Dump(binding)}) target updated newValue={newValue}");

            #endregion
        }

        private sealed class ApplicationTracer : ComponentDecoratorBase<IMugenApplication, IApplicationLifecycleDispatcherComponent>, IApplicationLifecycleDispatcherComponent
        {
            #region Constructors

            public ApplicationTracer() : base(ComponentPriority.Max)
            {
            }

            #endregion

            #region Implementation of interfaces

            public void OnLifecycleChanged(IMugenApplication application, ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
            {
                Tracer.Info()?.Trace($"{ApplicationTag}before lifecycle changed {lifecycleState}, state={state ?? "null"}, metadata={metadata.Dump()}");
                Components.OnLifecycleChanged(application, lifecycleState, state, metadata);
                Tracer.Info()?.Trace($"{ApplicationTag}after lifecycle changed {lifecycleState}, state={state ?? "null"}, metadata={metadata.Dump()}");
            }

            #endregion
        }

        private sealed class NavigatingErrorListener : ComponentDecoratorBase<INavigationDispatcher, INavigationErrorListener>, INavigationErrorListener
        {
            #region Constructors

            public NavigatingErrorListener() : base(ComponentPriority.Max)
            {
            }

            #endregion

            #region Implementation of interfaces

            public void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
            {
                Tracer.Info()?.Trace($"{NavigationTag}before failed {Dump(navigationDispatcher, navigationContext)}");
                Components.OnNavigationFailed(navigationDispatcher, navigationContext, exception);
                Tracer.Info()?.Trace($"{NavigationTag}after failed {Dump(navigationDispatcher, navigationContext)}");
            }

            public void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
            {
                Tracer.Info()?.Trace($"{NavigationTag}before canceled {Dump(navigationDispatcher, navigationContext)}");
                Components.OnNavigationCanceled(navigationDispatcher, navigationContext, cancellationToken);
                Tracer.Info()?.Trace($"{NavigationTag}after canceled {Dump(navigationDispatcher, navigationContext)}");
            }

            #endregion
        }

        private sealed class NavigatedTracer : ComponentDecoratorBase<INavigationDispatcher, INavigationListener>, INavigationListener
        {
            #region Constructors

            public NavigatedTracer() : base(ComponentPriority.Max)
            {
            }

            #endregion

            #region Implementation of interfaces

            public void OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                Tracer.Info()?.Trace($"{NavigationTag}before navigating {Dump(navigationDispatcher, navigationContext)}");
                Components.OnNavigating(navigationDispatcher, navigationContext);
                Tracer.Info()?.Trace($"{NavigationTag}after navigating {Dump(navigationDispatcher, navigationContext)}");
            }

            public void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                Tracer.Info()?.Trace($"{NavigationTag}before navigated {Dump(navigationDispatcher, navigationContext)}");
                Components.OnNavigated(navigationDispatcher, navigationContext);
                Tracer.Info()?.Trace($"{NavigationTag}after navigated {Dump(navigationDispatcher, navigationContext)}");
            }

            #endregion
        }

        private sealed class MessengerTracer : ComponentDecoratorBase<IMessenger, IMessengerSubscriberComponent>, IMessengerSubscriberComponent
        {
            #region Constructors

            public MessengerTracer() : base(ComponentPriority.Max)
            {
            }

            #endregion

            #region Implementation of interfaces

            public bool TrySubscribe(IMessenger messenger, object subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata)
                => Components.TrySubscribe(messenger, subscriber, executionMode, metadata);

            public bool TryUnsubscribe(IMessenger messenger, object subscriber, IReadOnlyMetadataContext? metadata)
                => Components.TryUnsubscribe(messenger, subscriber, metadata);

            public bool TryUnsubscribeAll(IMessenger messenger, IReadOnlyMetadataContext? metadata)
                => Components.TryUnsubscribeAll(messenger, metadata);

            public ItemOrList<MessengerHandler, IReadOnlyList<MessengerHandler>> TryGetMessengerHandlers(IMessenger messenger, Type messageType, IReadOnlyMetadataContext? metadata)
            {
                var editor = ItemOrListEditor.Get<MessengerHandler>(handler => handler.IsEmpty);
                var handlers = Components.TryGetMessengerHandlers(messenger, messageType, metadata);
                foreach (var messengerHandler in handlers)
                {
                    messengerHandler.Deconstruct(out var subscriber, out var mode, out var handler, out var state);
                    editor.Add(new MessengerHandler((o, context, s) =>
                    {
                        Tracer.Info()?.Trace($"{MessagingTag}handling message {Dump(o, context)}");
                        var result = handler!(o, context, s);
                        Tracer.Info()?.Trace($"{MessagingTag}handled message result={result}, {Dump(o, context)}");
                        return result;
                    }, subscriber!, mode, state));
                }

                return editor.ToItemOrList<IReadOnlyList<MessengerHandler>>();
            }

            public ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>> TryGetSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata)
                => Components.TryGetSubscribers(messenger, metadata);

            #endregion
        }

        private sealed class PresenterTracer : ComponentDecoratorBase<IPresenter, IPresenterComponent>, IPresenterComponent
        {
            #region Constructors

            public PresenterTracer() : base(ComponentPriority.Max)
            {
            }

            #endregion

            #region Implementation of interfaces

            public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            {
                Tracer.Info()?.Trace($"{PresentationTag}showing request={request}, metadata={metadata.Dump()}");
                var result = Components.TryShow(presenter, request, cancellationToken, metadata);
                Tracer.Info()?.Trace($"{PresentationTag}shown request={request}{Dump(result)}, metadata={metadata.Dump()}");
                return result;
            }

            public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            {
                Tracer.Info()?.Trace($"{PresentationTag}closing request={request}, metadata={metadata.Dump()}");
                var result = Components.TryClose(presenter, request, cancellationToken, metadata);
                Tracer.Info()?.Trace($"{PresentationTag}closed request={request}{Dump(result)}, metadata={metadata.Dump()}");
                return result;
            }

            #endregion
        }

        private sealed class ViewModelTracer : ComponentDecoratorBase<IViewModelManager, IViewModelLifecycleDispatcherComponent>, IViewModelLifecycleDispatcherComponent
        {
            #region Fields

            private int _createdCount;
            private int _disposedCount;
            private int _finalizedCount;

            #endregion

            #region Constructors

            public ViewModelTracer() : base(ComponentPriority.Max)
            {
            }

            #endregion

            #region Implementation of interfaces

            public void OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
            {
                Tracer.Info()?.Trace($"{ViewModelTag}before ({viewModel}) lifecycle changed {lifecycleState}, state={state ?? "null"}, metadata={metadata.Dump()}");
                Components.OnLifecycleChanged(viewModelManager, viewModel, lifecycleState, state, metadata);
                Tracer.Info()?.Trace($"{ViewModelTag}after ({viewModel}) lifecycle changed {lifecycleState}, state={state ?? "null"}, metadata={metadata.Dump()}");
                if (lifecycleState == ViewModelLifecycleState.Created)
                    ++_createdCount;
                else if (lifecycleState == ViewModelLifecycleState.Disposed)
                    ++_disposedCount;
                else if (lifecycleState == ViewModelLifecycleState.Finalized)
                    ++_finalizedCount;
                Tracer.Info()?.Trace($"{ViewModelTag}created={_createdCount}, disposed={_disposedCount}, finalized={_finalizedCount}");
            }

            #endregion
        }

        private sealed class ViewTracer : ComponentDecoratorBase<IViewManager, IViewLifecycleDispatcherComponent>, IViewLifecycleDispatcherComponent
        {
            #region Constructors

            public ViewTracer() : base(ComponentPriority.Max)
            {
            }

            #endregion

            #region Implementation of interfaces

            public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
            {
                Tracer.Info()?.Trace($"{ViewTag}before ({Dump(viewManager, view)}) lifecycle changed {lifecycleState}, state={state ?? "null"}, metadata={metadata.Dump()}");
                Components.OnLifecycleChanged(viewManager, view, lifecycleState, state, metadata);
                Tracer.Info()?.Trace($"{ViewTag}after ({Dump(viewManager, view)}) lifecycle changed {lifecycleState}, state={state ?? "null"}, metadata={metadata.Dump()}");
            }

            #endregion

            #region Methods

            private static string Dump(IViewManager viewManager, object view)
            {
                var views = viewManager.GetViews(view);
                if (views.Count == 0)
                    return $"view={view}";
                if (views.Count == 1)
                    return $"view={views.Item}, viewmodel={views.Item.ViewModel}";
                var stringBuilder = new StringBuilder();
                var count = 0;
                foreach (var v in views)
                    stringBuilder.Append($"view_{count++}={v.Target}, viewmodel={v.ViewModel}; ");
                return stringBuilder.ToString();
            }

            #endregion
        }

        #endregion
    }
}