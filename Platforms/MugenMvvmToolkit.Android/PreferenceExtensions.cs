#region Copyright

// ****************************************************************************
// <copyright file="PreferenceExtensions.cs">
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

using System.Collections.Generic;
using System.Xml;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Binding;

#if APPCOMPAT
using Android.Support.V7.Preferences;
using MugenMvvmToolkit.Android.PreferenceCompat.Infrastructure;

namespace MugenMvvmToolkit.Android.PreferenceCompat
#else
using Android.Preferences;
using MugenMvvmToolkit.Android.Infrastructure;

namespace MugenMvvmToolkit.Android
#endif
{
    public static class PreferenceExtensions
    {
        #region Fields

        private static readonly Dictionary<int, List<KeyValuePair<string, string>>> PreferenceBindMapping;

        #endregion

        #region Constructors

        static PreferenceExtensions()
        {
            PreferenceBindMapping = new Dictionary<int, List<KeyValuePair<string, string>>>();
        }

        #endregion

        #region Methods

        public static void InitializePreferenceListener(this PreferenceManager manager, ref PreferenceChangeListener preferenceChangeListener)
        {
            if (manager != null)
            {
                if (preferenceChangeListener == null)
                    preferenceChangeListener = new PreferenceChangeListener(manager);
                if (!preferenceChangeListener.State)
                {
                    manager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(preferenceChangeListener);
                    preferenceChangeListener.State = true;
                }
            }
        }

        public static void InitializePreferences(PreferenceScreen preferenceScreen, int preferencesResId, object parent)
        {
            preferenceScreen.SetBindingMemberValue(AttachedMembersBase.Object.Parent, parent);
            SetPreferenceParent(preferenceScreen);

            List<KeyValuePair<string, string>> bindings;
            if (!PreferenceBindMapping.TryGetValue(preferencesResId, out bindings))
            {
                bindings = new List<KeyValuePair<string, string>>();
                using (var reader = preferenceScreen.Context.Resources.GetXml(preferencesResId))
                {
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                string bind = null;
                                for (int attInd = 0; attInd < reader.AttributeCount; attInd++)
                                {
                                    reader.MoveToAttribute(attInd);
                                    if (reader.Name == "bind")
                                    {
                                        bind = reader.Value;
                                        break;
                                    }
                                }
                                reader.MoveToElement();
                                var key = reader.GetAttribute("key", "http://schemas.android.com/apk/res/android");
                                if (string.IsNullOrEmpty(bind))
                                    break;
                                if (string.IsNullOrEmpty(key))
                                {
                                    Tracer.Error("Preference {0} must have a key to use it with bindings", reader.Name);
                                    break;
                                }
                                bindings.Add(new KeyValuePair<string, string>(key, bind));
                                break;
                        }
                    }
                }
                PreferenceBindMapping[preferencesResId] = bindings;
            }

            foreach (var map in bindings)
            {
                var preference = preferenceScreen.FindPreference(map.Key);
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(preference, map.Value);
            }
        }

        private static void SetPreferenceParent(Preference preference)
        {
            var group = preference as PreferenceGroup;
            if (group == null)
                return;
            for (var i = 0; i < group.PreferenceCount; i++)
            {
                var p = group.GetPreference(i);
                p.SetBindingMemberValue(AttachedMembersBase.Object.Parent, group);
                SetPreferenceParent(p);
            }
        }

        #endregion
    }
}