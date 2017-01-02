using System;
using Android.Preferences;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Android.Infrastructure.Mediators
{
    public class MvvmPreferenceActivityMediator : MvvmActivityMediator
    {
        #region Fields

        private PreferenceChangeListener _preferenceChangeListener;

        #endregion

        #region Constructors

        public MvvmPreferenceActivityMediator([NotNull] PreferenceActivity target) : base(target)
        {
        }

        #endregion

        #region Properties

        protected PreferenceManager PreferenceManager => (Target as PreferenceActivity)?.PreferenceManager;

        #endregion

        #region Methods

        public override void OnPause(Action baseOnPause)
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
            base.OnPause(baseOnPause);
        }

        public override void OnDestroy(Action baseOnDestroy)
        {
            if (_preferenceChangeListener != null)
            {
                _preferenceChangeListener.Dispose();
                _preferenceChangeListener = null;
            }
            base.OnDestroy(baseOnDestroy);
        }

        public override void OnResume(Action baseOnResume)
        {
            base.OnResume(baseOnResume);
            PreferenceManager.InitializePreferenceListener(ref _preferenceChangeListener);
        }

        public override void AddPreferencesFromResource(Action<int> baseAddPreferencesFromResource, int preferencesResId)
        {
            var activity = Target as PreferenceActivity;
            if (activity == null)
            {
                Tracer.Error("The AddPreferencesFromResource method supported only for PreferenceActivity");
                return;
            }
            baseAddPreferencesFromResource(preferencesResId);
            InitializePreferences(activity.PreferenceScreen, preferencesResId);
        }

        protected virtual void InitializePreferences(PreferenceScreen preferenceScreen, int preferencesResId)
        {
            PreferenceExtensions.InitializePreferences(preferenceScreen, preferencesResId, Target);
            PreferenceManager.InitializePreferenceListener(ref _preferenceChangeListener);
        }

        #endregion
    }
}