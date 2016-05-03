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

        [CanBeNull]
        protected abstract PreferenceManager PreferenceManager { get; }

        #endregion

        #region Events

        public virtual event EventHandler<TTarget, EventArgs> DataContextChanged;

        #endregion

        #region Methods

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

        public virtual void OnResume(Action baseOnResume)
        {
            baseOnResume();
            SetPreferenceListener();
        }

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
                if (!viewModel.Settings.Metadata.TryGetData(ViewModelConstants.StateNotNeeded, out data) || !data)
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
                var pairs = ContextCache.Where(pair => ReferenceEquals(pair.Value, sender)).ToList();
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
