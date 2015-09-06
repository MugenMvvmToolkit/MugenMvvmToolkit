#region Copyright

// ****************************************************************************
// <copyright file="MediatorBase.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using System.Xml;
using System.Xml.Linq;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Binding;
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
        #region Nested types

        private sealed class PreferenceChangeListener : Java.Lang.Object, ISharedPreferencesOnSharedPreferenceChangeListener
        {
            #region Fields

            private readonly PreferenceManager _preferenceManager;
            public bool State;

            #endregion

            #region Constructors

            public PreferenceChangeListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            public PreferenceChangeListener(PreferenceManager preferenceManager)
            {
                _preferenceManager = preferenceManager;
            }

            #endregion

            #region Implementation of ISharedPreferencesOnSharedPreferenceChangeListener

            public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
            {
                if (_preferenceManager == null)
                {
                    sharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
                    return;
                }
                var preference = _preferenceManager.FindPreference(key);
                if (preference != null)
                    preference.TryRaiseAttachedEvent(AttachedMembers.Preference.ValueChangedEvent);
            }

            #endregion
        }

        #endregion

        #region Fields

        internal const string IgnoreStateKey = "#$@noState";
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
        private PreferenceChangeListener _preferenceChangeListener;
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

        /// <summary>
        ///     Gets or sets the current target object.
        /// </summary>
        [NotNull]
        protected TTarget Target
        {
            get { return _target; }
        }

        /// <summary>
        ///     Gets the current preference manager.
        /// </summary>
        [CanBeNull]
        protected abstract PreferenceManager PreferenceManager { get; }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        public virtual event EventHandler<TTarget, EventArgs> DataContextChanged;

        #endregion

        #region Methods

        /// <summary>
        ///     Called as part of the activity lifecycle when an activity is going into
        ///     the background, but has not (yet) been killed.
        /// </summary>
        public virtual void OnPause(Action baseOnPause)
        {
            var manager = PreferenceManager;
            if (manager != null)
            {
                if (_preferenceChangeListener != null)
                {
                    manager.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(_preferenceChangeListener);
                    _preferenceChangeListener.State = false;
                }
            }
            baseOnPause();
        }

        /// <summary>
        ///     Called after <c>OnRestoreInstanceState(Android.OS.Bundle)</c>, <c>OnRestart</c>, or <c>OnPause</c>, for your activity to start interacting with the user.
        /// </summary>
        public virtual void OnResume(Action baseOnResume)
        {
            baseOnResume();
            SetPreferenceListener();
        }

        /// <summary>
        ///     Perform any final cleanup before an activity is destroyed.
        /// </summary>
        public virtual void OnDestroy(Action baseOnDestroy)
        {
            var viewModel = DataContext as IViewModel;
            if (viewModel != null && !viewModel.IsDisposed && viewModel.IocContainer != null && !viewModel.IocContainer.IsDisposed)
                Get<IViewManager>().CleanupViewAsync(viewModel);
            if (_preferenceChangeListener != null)
            {
                _preferenceChangeListener.Dispose();
                _preferenceChangeListener = null;
            }
            DataContext = null;
            DataContextChanged = null;
            _isDestroyed = true;
            baseOnDestroy();
        }

        /// <summary>
        ///     Called after <c>OnCreate(Android.OS.Bundle)</c> or after <c>OnRestart</c> when the activity had been stopped, but is now again being displayed to the user.
        /// </summary>
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
                object currentStateManager;
                if (!viewModel.Settings.Metadata.TryGetData(ViewModelConstants.StateManager, out currentStateManager) || currentStateManager == this)
                {
                    bool data;
                    if (!viewModel.Settings.Metadata.TryGetData(ViewModelConstants.StateNotNeeded, out data) || !data)
                    {
                        PreserveViewModel(viewModel, outState);
                        saved = true;
                    }
                }
                if (!saved)
                    outState.PutString(IgnoreStateKey, null);
            }
            baseOnSaveInstanceState(outState);
        }

        /// <summary>
        ///     Called when the target is starting.
        /// </summary>
        protected void OnCreate(Bundle bundle)
        {
            if (_id == Guid.Empty)
                _id = Guid.NewGuid();
            if (bundle == null)
                return;
            var oldId = bundle.GetString(IdKey);
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

        /// <summary>
        ///     Tries to restore instance context.
        /// </summary>
        protected virtual void RestoreContext(TTarget target, object dataContext)
        {
            var viewModel = dataContext as IViewModel;
            if (viewModel == null)
                DataContext = dataContext;
            else
            {
                Get<IViewManager>().InitializeViewAsync(viewModel, target).WithTaskExceptionHandler(this);
                viewModel.Disposed -= ClearCacheOnDisposeDelegate;
                Get<IViewModelPresenter>().Restore(viewModel, CreateRestorePresenterContext(target));
            }
        }

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        protected virtual void OnDataContextChanged(object oldValue, object newValue)
        {
        }

        /// <summary>
        ///     Restores the view model.
        /// </summary>
        [CanBeNull]
        protected virtual IViewModel RestoreViewModel([NotNull] Type viewModelType, [NotNull] Bundle bundle)
        {
            var context = new DataContext
            {
                {InitializationConstants.ViewModelType, viewModelType}
            };
            return Get<IViewModelProvider>().RestoreViewModel(RestoreViewModelState(bundle), context, false);
        }

        /// <summary>
        ///     Restores the view model state.
        /// </summary>
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

        /// <summary>
        ///     Preserves the view model.
        /// </summary>
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

        protected virtual void InitializePreferences(PreferenceScreen preferenceScreen, int preferencesResId)
        {
            preferenceScreen.SetBindingMemberValue(AttachedMembers.Object.Parent, Target);
            SetPreferenceParent(preferenceScreen);
            using (XmlReader reader = preferenceScreen.Context.Resources.GetXml(preferencesResId))
            {
                var document = new XmlDocument();
                document.Load(reader);
                var xDocument = XDocument.Parse(document.InnerXml);
                foreach (var descendant in xDocument.Descendants())
                {
                    var bindAttr = descendant
                        .Attributes()
                        .FirstOrDefault(xAttribute => xAttribute.Name.LocalName.Equals("bind", StringComparison.OrdinalIgnoreCase));
                    if (bindAttr == null)
                        continue;
                    var attribute = descendant.Attribute(XName.Get("key", "http://schemas.android.com/apk/res/android"));
                    if (attribute == null)
                    {
                        Tracer.Error("Preference {0} must have a key to use it with bindings", descendant);
                        continue;
                    }
                    var preference = preferenceScreen.FindPreference(attribute.Value);
                    BindingServiceProvider.BindingProvider.CreateBindingsFromString(preference, bindAttr.Value);
                }
            }
            SetPreferenceListener();
        }

        protected T Get<T>()
        {
            var viewModel = DataContext as IViewModel;
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

        private void SetPreferenceListener()
        {
            var manager = PreferenceManager;
            if (manager != null)
            {
                if (_preferenceChangeListener == null)
                    _preferenceChangeListener = new PreferenceChangeListener(manager);
                if (!_preferenceChangeListener.State)
                {
                    manager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(_preferenceChangeListener);
                    _preferenceChangeListener.State = true;
                }
            }
        }

        protected static void ClearCacheOnDisposeViewModel(IDisposableObject sender, EventArgs args)
        {
            sender.Disposed -= ClearCacheOnDisposeDelegate;
            lock (ContextCache)
            {
                var pairs = ContextCache.Where(pair => ReferenceEquals(pair.Value, sender)).ToArray();
                foreach (var pair in pairs)
                    ContextCache.Remove(pair.Key);
            }
        }

        private static void SetPreferenceParent(Preference preference)
        {
            var @group = preference as PreferenceGroup;
            if (@group == null)
                return;
            for (int i = 0; i < @group.PreferenceCount; i++)
            {
                var p = @group.GetPreference(i);
                p.SetBindingMemberValue(AttachedMembers.Object.Parent, @group);
                SetPreferenceParent(p);
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