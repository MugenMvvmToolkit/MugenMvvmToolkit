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
using System.Linq;
using Android.OS;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Utils;
using IViewManager = MugenMvvmToolkit.Interfaces.IViewManager;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    public abstract class MediatorBase<TTarget>
        where TTarget : class
    {
        #region Fields

        // ReSharper disable once StaticFieldInGenericType
        private static readonly Dictionary<Guid, object> ContextCache;

        private IBindingContext _context;
        private readonly TTarget _target;
        private Guid _id;
        private object _dataContext;
        private const string IdKey = "~ctxid~";

        #endregion

        #region Constructors

        static MediatorBase()
        {
            ContextCache = new Dictionary<Guid, object>();
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

        protected TTarget Target
        {
            get { return _target; }
        }

        protected IBindingContext BindingContext
        {
            get
            {
                if (_context == null)
                    _context = BindingProvider.Instance.ContextManager.GetBindingContext(_target);
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
            var viewModel = BindingContext.DataContext as IViewModel;
            if (viewModel != null)
                Get<IViewManager>().CleanupViewAsync(viewModel);
            baseOnDestroy();
        }

        /// <summary>
        ///     Called to ask the view to save its current dynamic state, so it
        ///     can later be reconstructed in a new instance of its process is
        ///     restarted.
        /// </summary>
        public virtual void OnSaveInstanceState(Bundle outState, Action<Bundle> baseOnSaveInstanceState)
        {
            lock (ContextCache)
                ContextCache[_id] = BindingContext.DataContext;
            outState.PutString(IdKey, _id.ToString());
            var viewModel = BindingContext.DataContext as IViewModel;
            if (viewModel != null)
                viewModel.Disposed += ClearCacheOnDispose;
            baseOnSaveInstanceState(outState);
        }

        /// <summary>
        ///     Called when the target is starting.
        /// </summary>
        protected void OnCreate(Bundle savedInstanceState)
        {
            if (_context == null)
                _context = BindingProvider.Instance.ContextManager.GetBindingContext(_target);
            if (savedInstanceState != null)
            {
                var oldId = savedInstanceState.GetString(IdKey);
                if (!string.IsNullOrEmpty(oldId))
                    TryRestoreContext(GetFromCache(Guid.Parse(oldId)));
            }
            if (_id == Guid.Empty)
                _id = Guid.NewGuid();
        }

        /// <summary>
        /// Tries to restore instance context.
        /// </summary>
        protected virtual void TryRestoreContext(object oldContext)
        {
            var viewModel = oldContext as IViewModel;
            if (viewModel == null)
                BindingContext.DataContext = oldContext;
            else
            {
                var viewManager = Get<IViewManager>();
                var view = Target as IView ?? viewManager.WrapToView(Target, Models.DataContext.Empty);
                MvvmUtils.WithTaskExceptionHandler(viewManager.InitializeViewAsync(viewModel, view), this);
                viewModel.Disposed -= ClearCacheOnDispose;
            }
        }

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        protected virtual void OnDataContextChanged(object oldValue, object newValue)
        {
        }

        protected T Get<T>()
        {
            var viewModel = BindingContext.DataContext as IViewModel;
            if (viewModel == null)
                return ServiceProvider.IocContainer.Get<T>();
            return MvvmUtils.GetIocContainer(viewModel, true).Get<T>();
        }

        protected void ClearContextCache()
        {
            if (_id == Guid.Empty)
                return;
            lock (ContextCache)
                ContextCache.Remove(_id);
        }

        private static void ClearCacheOnDispose(IDisposableObject sender, EventArgs args)
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