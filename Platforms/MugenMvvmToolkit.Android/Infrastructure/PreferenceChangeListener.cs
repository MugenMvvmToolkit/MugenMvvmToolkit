#region Copyright

// ****************************************************************************
// <copyright file="PreferenceChangeListener.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using Android.Content;
using Android.Runtime;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Binding;
using Object = Java.Lang.Object;

#if APPCOMPAT
using Android.Support.V7.Preferences;

namespace MugenMvvmToolkit.Android.PreferenceCompat.Infrastructure
#else
using Android.Preferences;

namespace MugenMvvmToolkit.Android.Infrastructure
#endif
{
    public sealed class PreferenceChangeListener : Object, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        #region Fields

        private readonly PreferenceManager _preferenceManager;
        internal bool State;

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
            _preferenceManager.FindPreference(key)?.TryRaiseAttachedEvent(AttachedMembers.Preference.ValueChangedEvent.Override<Preference>());
        }

        #endregion        
    }
}