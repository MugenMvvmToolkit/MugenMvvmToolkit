#region Copyright

// ****************************************************************************
// <copyright file="MediatorBase.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.OS;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Android.Infrastructure.Mediators
{
    public abstract class MediatorBase<TTarget>
        where TTarget : class
    {
        #region Fields

        protected const string IgnoreStateKey = "#$@noState";
        // ReSharper disable StaticFieldInGenericType
        private static readonly Dictionary<Guid, object> ContextCache;
        private static readonly string StateKey;
        private static readonly string ViewModelTypeNameKey;
        private static readonly string IdKey;
        private static readonly EventHandler<IDisposableObject, EventArgs> ClearCacheOnDisposeDelegate;
        // ReSharper restore StaticFieldInGenericType

        private Guid _id;
        private object _dataContext;
        private bool _isDestroyed;
        private readonly TTarget _target;

        #endregion

        #region Constructors

        static MediatorBase()
        {
            ContextCache = new Dictionary<Guid, object>();
            StateKey = "!~state" + typeof(TTarget).Name;
            ViewModelTypeNameKey = "~vmtype" + typeof(TTarget).Name;
            IdKey = "~ctxid~" + typeof(TTarget).Name;
            ClearCacheOnDisposeDelegate = ClearCacheOnDisposeViewModel;
        }

        protected MediatorBase([NotNull] TTarget target)
        {
            Should.NotBeNull(target, nameof(target));
            _target = target;
        }

        #endregion

        #region Properties

        public virtual object DataContext
        {
            get { return _dataContext; }
            set
            {
                if (ReferenceEquals(value, _dataContext))
                    return;
                var oldValue = _dataContext;
                _dataContext = value;
                OnDataContextChanged(oldValue, _dataContext);
                DataContextChanged?.Invoke(Target, EventArgs.Empty);
            }
        }

        public bool IsDestroyed => _isDestroyed;

        [NotNull]
        protected TTarget Target => _target;

        #endregion

        #region Events

        public virtual event EventHandler<TTarget, EventArgs> DataContextChanged;

        #endregion

        #region Methods

        public virtual void OnPause(Action baseOnPause)
        {
            baseOnPause();
        }

        public virtual void OnResume(Action baseOnResume)
        {
            baseOnResume();
        }

        public virtual void OnDestroy(Action baseOnDestroy)
        {
            var viewModel = DataContext as IViewModel;
            if (viewModel != null && !viewModel.IsDisposed && viewModel.IocContainer != null && !viewModel.IocContainer.IsDisposed)
                ServiceProvider.ViewManager.CleanupViewAsync(viewModel);
            DataContext = null;
            DataContextChanged = null;
            _isDestroyed = true;
            baseOnDestroy();
        }

        public virtual void OnSaveInstanceState(Bundle outState, Action<Bundle> baseOnSaveInstanceState)
        {
            lock (ContextCache)
                ContextCache[_id] = DataContext;
            outState.PutString(IdKey, _id.ToString());
            var viewModel = DataContext as IViewModel;
            if (viewModel != null)
            {
                viewModel.Disposed += ClearCacheOnDisposeDelegate;
                outState.PutString(ViewModelTypeNameKey, viewModel.GetType().AssemblyQualifiedName);

                bool saved = false;
                bool data;
                if (!viewModel.IsDisposed && (!viewModel.Settings.Metadata.TryGetData(ViewModelConstants.StateNotNeeded, out data) || !data))
                {
                    PreserveViewModel(viewModel, outState);
                    saved = true;
                }
                if (!saved)
                    outState.PutString(IgnoreStateKey, null);
            }
            baseOnSaveInstanceState(outState);
        }

        protected void OnCreate(Bundle bundle)
        {
            if (_id == Guid.Empty)
                _id = Guid.NewGuid();
            var oldId = bundle?.GetString(IdKey);
            if (string.IsNullOrEmpty(oldId))
                return;
            var cacheDataContext = GetFromCache(Guid.Parse(oldId));
            var vmTypeName = bundle.GetString(ViewModelTypeNameKey);
            if (vmTypeName == null)
                return;
            bundle.Remove(ViewModelTypeNameKey);
            var vmType = Type.GetType(vmTypeName, false);
            if (vmType != null && (cacheDataContext == null || !cacheDataContext.GetType().Equals(vmType)))
            {
                if (!bundle.ContainsKey(IgnoreStateKey))
                    cacheDataContext = RestoreViewModel(vmType, bundle);
            }
            if (!ReferenceEquals(DataContext, cacheDataContext))
                RestoreContext(Target, cacheDataContext);
        }

        protected virtual void RestoreContext(TTarget target, object dataContext)
        {
            var viewModel = dataContext as IViewModel;
            if (viewModel == null)
                DataContext = dataContext;
            else
            {
                ServiceProvider.ViewManager.InitializeViewAsync(viewModel, target);
                viewModel.Disposed -= ClearCacheOnDisposeDelegate;
                Get<IViewModelPresenter>().Restore(viewModel, CreateRestorePresenterContext(target));
            }
        }

        protected virtual void OnDataContextChanged(object oldValue, object newValue)
        {
        }

        [CanBeNull]
        protected virtual IViewModel RestoreViewModel([NotNull] Type viewModelType, [NotNull] Bundle bundle)
        {
            var context = new DataContext
            {
                {InitializationConstants.ViewModelType, viewModelType}
            };
            return Get<IViewModelProvider>().RestoreViewModel(RestoreViewModelState(bundle), context, true);
        }

        [NotNull]
        protected virtual IDataContext RestoreViewModelState([NotNull] Bundle bundle)
        {
            var bytes = bundle.GetByteArray(StateKey);
            if (bytes == null)
                return MugenMvvmToolkit.Models.DataContext.Empty;
            bundle.Remove(StateKey);
            using (var ms = new MemoryStream(bytes))
                return (IDataContext)Get<ISerializer>().Deserialize(ms);
        }

        protected virtual void PreserveViewModel([NotNull] IViewModel viewModel, [NotNull] Bundle bundle)
        {
            var state = Get<IViewModelProvider>().PreserveViewModel(viewModel, MugenMvvmToolkit.Models.DataContext.Empty);
            if (state.Count == 0)
                bundle.Remove(StateKey);
            else
            {
                using (var stream = Get<ISerializer>().Serialize(state))
                    bundle.PutByteArray(StateKey, stream.ToArray());
            }
        }

        protected virtual IDataContext CreateRestorePresenterContext(TTarget target)
        {
            return new DataContext
            {
                {NavigationConstants.SuppressPageNavigation, true}
            };
        }

        protected T Get<T>()
        {
            var viewModel = DataContext as IViewModel;
            if (viewModel == null)
                return ServiceProvider.Get<T>();
            return viewModel.GetIocContainer(true).Get<T>();
        }

        protected void ClearContextCache()
        {
            if (_id == Guid.Empty)
                return;
            lock (ContextCache)
                ContextCache.Remove(_id);
        }

        protected static void ClearCacheOnDisposeViewModel(IDisposableObject sender, EventArgs args)
        {
            sender.Disposed -= ClearCacheOnDisposeDelegate;
            lock (ContextCache)
            {
                var pairs = ContextCache.Where(pair => ReferenceEquals(pair.Value, sender)).ToList();
                foreach (var pair in pairs)
                    ContextCache.Remove(pair.Key);
            }
        }

        private static object GetFromCache(Guid id)
        {
            lock (ContextCache)
            {
                object value;
                if (ContextCache.TryGetValue(id, out value))
                    ContextCache.Remove(id);
                return value;
            }
        }

        #endregion
    }
}
