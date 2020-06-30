using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Internal;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;

namespace MugenMvvm.ViewModels.Components
{
    public class ViewModelCleaner : IViewModelLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly IAttachedValueManager? _attachedValueProvider;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private readonly IViewManager? _viewManager;

        private static readonly TypeLightDictionary<object?> TypesToCommandsProperties = new TypeLightDictionary<object?>(59);

        #endregion

        #region Constructors

        public ViewModelCleaner(IViewManager? viewManager = null, IAttachedValueManager? attachedValueProvider = null, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            _viewManager = viewManager;
            _attachedValueProvider = attachedValueProvider;
            _reflectionDelegateProvider = reflectionDelegateProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewModelComponentPriority.PostInitializer;

        public bool CleanupCommands { get; set; } = true;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged<TState>(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == ViewModelLifecycleState.Disposed)
                Cleanup(viewModel, lifecycleState, state, metadata);
        }

        #endregion

        #region Methods

        protected virtual void Cleanup<TState>(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (CleanupCommands)
                DisposeCommands(viewModel);
            var viewManager = _viewManager.DefaultIfNull();
            var views = viewManager.GetViews(viewModel, metadata);
            for (var i = 0; i < views.Count(); i++)
                viewManager.CleanupAsync(views.Get(i), state, default, metadata);

            var busyManager = viewModel.TryGetOptionalService<IBusyManager>();
            if (busyManager != null)
            {
                busyManager.ClearBusy();
                busyManager.ClearComponents(metadata);
            }

            var messenger = viewModel.TryGetOptionalService<IMessenger>();
            if (messenger != null)
            {
                messenger.UnsubscribeAll(metadata);
                messenger.ClearComponents(metadata);
            }

            viewModel.ClearMetadata(true);
            _attachedValueProvider.DefaultIfNull().Clear(viewModel);
            (viewModel as IValueHolder<IWeakReference>)?.ReleaseWeakReference();
        }

        protected void DisposeCommands(IViewModelBase viewModel)
        {
            object? rawValue;
            Type type = viewModel.GetType();
            lock (TypesToCommandsProperties)
            {
                if (!TypesToCommandsProperties.TryGetValue(type, out rawValue))
                {
                    ItemOrList<Func<object, ICommand>, List<Func<object, ICommand>>> items = default;
                    foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (typeof(ICommand).IsAssignableFrom(p.PropertyType) && p.CanRead &&
                            p.GetIndexParameters().Length == 0)
                        {
                            var func = p.GetMemberGetter<object, ICommand>(_reflectionDelegateProvider);
                            items.Add(func);
                        }
                    }

                    rawValue = items.GetRawValue();
                    TypesToCommandsProperties[type] = rawValue;
                }
            }

            if (rawValue == null)
                return;

            var list = ItemOrList<Func<object, ICommand>, List<Func<object, ICommand>>>.FromRawValue(rawValue);
            for (var index = 0; index < MugenExtensions.Count(list); index++)
            {
                try
                {
                    (MugenExtensions.Get(list, index).Invoke(viewModel) as IDisposable)?.Dispose();
                }
                catch (Exception)
                {
                    //To avoid method access exception.
                }
            }
        }

        #endregion
    }
}