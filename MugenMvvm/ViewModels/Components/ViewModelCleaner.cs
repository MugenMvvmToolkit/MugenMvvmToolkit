using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
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

        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly IReflectionManager? _reflectionManager;
        private readonly IViewManager? _viewManager;

        private static readonly Dictionary<Type, object?> TypesToCommandsProperties = new Dictionary<Type, object?>(59, InternalEqualityComparer.Type);

        #endregion

        #region Constructors

        public ViewModelCleaner(IViewManager? viewManager = null, IAttachedValueManager? attachedValueManager = null, IReflectionManager? reflectionManager = null)
        {
            _viewManager = viewManager;
            _attachedValueManager = attachedValueManager;
            _reflectionManager = reflectionManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewModelComponentPriority.PostInitializer;

        public bool CleanupCommands { get; set; } = true;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == ViewModelLifecycleState.Disposed)
                Cleanup(viewModel, lifecycleState, state, metadata);
        }

        #endregion

        #region Methods

        protected virtual void Cleanup(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (CleanupCommands)
                DisposeCommands(viewModel);
            var viewManager = _viewManager.DefaultIfNull();
            foreach (var v in viewManager.GetViews(viewModel, metadata))
                viewManager.TryCleanupAsync(v, state, default, metadata);

            var busyManager = viewModel.TryGetService<IBusyManager>(true);
            if (busyManager != null)
            {
                busyManager.ClearBusy();
                busyManager.ClearComponents(metadata);
            }

            var messenger = viewModel.TryGetService<IMessenger>(true);
            if (messenger != null)
            {
                messenger.UnsubscribeAll(metadata);
                messenger.ClearComponents(metadata);
            }

            viewModel.ClearMetadata(true);
            viewModel.AttachedValues(metadata, _attachedValueManager).Clear();
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
                    var items = ItemOrListEditor.Get<Func<object, ICommand>>();
                    foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (typeof(ICommand).IsAssignableFrom(p.PropertyType) && p.CanRead &&
                            p.GetIndexParameters().Length == 0)
                        {
                            var func = p.GetMemberGetter<object, ICommand>(_reflectionManager);
                            items.Add(func);
                        }
                    }

                    rawValue = items.GetRawValue();
                    TypesToCommandsProperties[type] = rawValue;
                }
            }

            if (rawValue == null)
                return;

            foreach (var invoker in ItemOrList.FromRawValue<Func<object, ICommand>, List<Func<object, ICommand>>>(rawValue))
            {
                try
                {
                    (invoker.Invoke(viewModel) as IDisposable)?.Dispose();
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