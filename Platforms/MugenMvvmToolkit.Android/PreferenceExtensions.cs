using System.Collections.Generic;
using System.Xml;
using Android.Preferences;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Binding;

namespace MugenMvvmToolkit.Android
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
            preferenceScreen.SetBindingMemberValue(AttachedMembers.Object.Parent, parent);
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
                                var bind = reader.GetAttribute("bind");
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
                p.SetBindingMemberValue(AttachedMembers.Object.Parent, group);
                SetPreferenceParent(p);
            }
        }

        #endregion
    }
}