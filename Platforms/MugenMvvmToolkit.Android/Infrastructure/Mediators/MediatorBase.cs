#region Copyright
// ****************************************************************************
// <copyright file="MediatorBase.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using IViewManager = MugenMvvmToolkit.Interfaces.IViewManager;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    public abstract class MediatorBase<TTarget>
        where TTarget : class
    {
        #region Fields

        // ReSharper disable StaticFieldInGenericType
        private static readonly Dictionary<Guid, object> ContextCache;
        private static readonly string StateKey;
        private static readonly string ViewModelTypeNameKey;
        private static readonly string IdKey;
        // ReSharper restore StaticFieldInGenericType

        private IBindingContext _context;
        private readonly TTarget _target;
        private Guid _id;
        private object _dataContext;
        private bool _isDestroyed;

        #endregion

        #region Constructors

        static MediatorBase()
        {
            ContextCache = new Dictionary<Guid, object>();
            StateKey = "!~state" + typeof(TTarget).Name;
            ViewModelTypeNameKey = "~vmtype" + typeof(TTarget).Name;
            IdKey = "~ctxid~" + typeof(TTarget).Name;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MediatorBase{TTarget}" /> class.
        /// </summary>
        protected MediatorBase([NotNull] TTarget target)
        {
            Should.NotBeNull(target, "target");
            _target = target;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the data context.
        /// </summary>
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
                var handler = DataContextChanged;
                if (handler != null)
                    handler(Target, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Returns true if the final <c>OnDestroy</c> call has been made on the Target, so this instance is now dead.
        /// </summary>
        public bool IsDestroyed
        {
            get { return _isDestroyed; }
        }

        protected TTarget Target
        {
            get { return _target; }
        }

        protected IBindingContext BindingContext
        {
            get
            {
                if (_context == null)
                    _context = BindingServiceProvider.ContextManager.GetBindingContext(_target);
                return _context;
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        public event EventHandler<TTarget, EventArgs> DataContextChanged;

        #endregion

        #region Methods

        /// <summary>
        ///     Perform any final cleanup before an activity is destroyed.
        /// </summary>
        public virtual void OnDestroy(Action baseOnDestroy)
        {
            _isDestroyed = true;
            var viewModel = BindingContext.Value as IViewModel;
            if (viewModel != null && !viewModel.IsDisposed)
                Get<IViewManager>().CleanupViewAsync(viewModel);
            baseOnDestroy();
        }

        /// <summary>
        ///     Called after <c>OnCreate(Android.OS.Bundle)</c> or after <c>OnRestart</c> when the activity had been stopped, but is now again being displayed to the user.
        /// </summary>
        public virtual void OnSaveInstanceState(Bundle outState, Action<Bundle> baseOnSaveInstanceState)
        {
            lock (ContextCache)
                ContextCache[_id] = BindingContext.Value;
            outState.PutString(IdKey, _id.ToString());
            var viewModel = BindingContext.Value as IViewModel;
            if (viewModel != null)
            {
                viewModel.Disposed += ClearCacheOnDispose;
                object currentStateManager;
                if (!viewModel.Settings.Metadata.TryGetData(ViewModelConstants.StateManager, out currentStateManager) || currentStateManager == this)
                {
                    bool data;
                    if (!viewModel.Settings.Metadata.TryGetData(ViewModelConstants.StateNotNeeded, out data) || !data)
                    {
                        outState.PutString(ViewModelTypeNameKey, viewModel.GetType().AssemblyQualifiedName);
                        PreserveState(outState, viewModel);
                    }
                }
            }
            baseOnSaveInstanceState(outState);
        }

        /// <summary>
        ///     Called when the target is starting.
        /// </summary>
        protected void OnCreate(Bundle savedInstanceState)
        {
            if (_context == null)
                _context = BindingServiceProvider.ContextManager.GetBindingContext(_target);
            if (savedInstanceState != null)
            {
                var oldId = savedInstanceState.GetString(IdKey);
                if (!string.IsNullOrEmpty(oldId))
                {
                    var oldContext = RestoreState(savedInstanceState, GetFromCache(Guid.Parse(oldId)));
                    if (!ReferenceEquals(BindingContext.Value, oldContext))
                        RestoreContext(oldContext);
                }
            }
            if (_id == Guid.Empty)
                _id = Guid.NewGuid();
        }

        /// <summary>
        ///     Tries to restore instance context.
        /// </summary>
        protected virtual void RestoreContext(object dataContext)
        {
            var viewModel = dataContext as IViewModel;
            if (viewModel == null)
                BindingContext.Value = dataContext;
            else
            {
                Get<IViewManager>().InitializeViewAsync(viewModel, Target).WithTaskExceptionHandler(this);
                viewModel.Disposed -= ClearCacheOnDispose;
                Get<IViewModelPresenter>().Restore(viewModel, CreateRestorePresenterContext());
            }
        }

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        protected virtual void OnDataContextChanged(object oldValue, object newValue)
        {
        }

        protected virtual void PreserveState(Bundle bundle, IViewModel viewModel)
        {
            if (viewModel == null || bundle == null)
                return;
            var state = Get<IViewModelProvider>().PreserveViewModel(viewModel, Models.DataContext.Empty);
            if (state.Count == 0)
                bundle.Remove(StateKey);
            else
            {
                using (var stream = Get<ISerializer>().Serialize(state))
                    bundle.PutByteArray(StateKey, stream.ToArray());
            }
        }

        protected virtual object RestoreState(Bundle bundle, object dataContext)
        {
            if (bundle == null)
                return dataContext;

            var vmTypeName = bundle.GetString(ViewModelTypeNameKey);
            if (vmTypeName == null)
                return dataContext;

            bundle.Remove(ViewModelTypeNameKey);
            var vmType = Type.GetType(vmTypeName, false);
            if (vmType == null || (dataContext != null && dataContext.GetType().Equals(vmType)))
                return dataContext;

            IDataContext state = null;
            var bytes = bundle.GetByteArray(StateKey);
            if (bytes != null)
            {
                bundle.Remove(StateKey);
                using (var ms = new MemoryStream(bytes))
                    state = (IDataContext)Get<ISerializer>().Deserialize(ms);
            }
            var context = new DataContext
            {
                {InitializationConstants.ViewModelType, vmType}
            };
            return Get<IViewModelProvider>().RestoreViewModel(state, context, false);
        }

        protected virtual IDataContext CreateRestorePresenterContext()
        {
            return new DataContext
            {
                {NavigationConstants.SuppressPageNavigation, true}
            };
        }

        protected T Get<T>()
        {
            var viewModel = BindingContext.Value as IViewModel;
            if (viewModel == null)
                return ServiceProvider.IocContainer.Get<T>();
            return viewModel.GetIocContainer(true).Get<T>();
        }

        protected void ClearContextCache()
        {
            if (_id == Guid.Empty)
                return;
            lock (ContextCache)
                ContextCache.Remove(_id);
        }

        protected static void ClearCacheOnDispose(IDisposableObject sender, EventArgs args)
        {
            sender.Disposed -= ClearCacheOnDispose;
            lock (ContextCache)
            {
                var pairs = ContextCache.Where(pair => ReferenceEquals(pair.Value, sender)).ToArray();
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