using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Android.Preferences;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Binding;

namespace MugenMvvmToolkit.Android
{
    public static class PreferenceExtensions
    {
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
            //todo fix remove XElement
            preferenceScreen.SetBindingMemberValue(AttachedMembers.Object.Parent, parent);
            SetPreferenceParent(preferenceScreen);
            using (var reader = preferenceScreen.Context.Resources.GetXml(preferencesResId))
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