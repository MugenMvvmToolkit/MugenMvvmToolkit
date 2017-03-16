#region Copyright

// ****************************************************************************
// <copyright file="PreferenceInitializationModule.cs">
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

using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

#if APPCOMPAT
using MugenMvvmToolkit.Android.AppCompat.Interfaces.Mediators;
using MugenMvvmToolkit.Android.PreferenceCompat.Infrastructure.Mediators;
using Android.Support.V4.App;
using Android.Support.V7.Preferences;

namespace MugenMvvmToolkit.Android.PreferenceCompat.Modules
#else
using Android.App;
using Android.Preferences;
using MugenMvvmToolkit.Android.Infrastructure.Mediators;
using MugenMvvmToolkit.Android.Interfaces.Mediators;
using PreferenceCompatAttachedMembersRegistration = MugenMvvmToolkit.Android.Binding.AttachedMembersRegistration;

namespace MugenMvvmToolkit.Android.Modules
#endif
{
#if APPCOMPAT
    public class PreferenceCompatInitializationModule : IModule
#else
    public class PreferenceInitializationModule : IModule
#endif
    {
        #region Properties

        public int Priority => ApplicationSettings.ModulePriorityDefault - 1;

        #endregion

        #region Methods

        private static LightDictionaryBase<string, object> GetOrAddAttachedDictionaryHandler(object item, bool addNew)
        {
            //.NET object (Preference) is garbage collected and all attached members too but Java object is still alive.
            //Save values to activity dictionary.
            var pref = item as Preference;
            if (pref.IsAlive() && pref.HasKey)
            {
                try
                {
                    var activityView = pref.Context as IActivityView;
                    if (activityView != null)
                    {
                        var metadata = activityView.Mediator.Metadata;
                        var key = pref.Key + pref.GetType().FullName + pref.GetHashCode();
                        object v;
                        if (!metadata.TryGetValue(key, out v))
                        {
                            if (addNew)
                            {
                                v = new AttachedValueProviderBase.AttachedValueDictionary();
                                metadata[key] = v;
                            }
                        }
                        return (LightDictionaryBase<string, object>)v;
                    }
                }
                catch
                {
                    ;
                }
            }
            return null;
        }

        private static bool ClearAttachedValue(object item)
        {
            var pref = item as Preference;
            if (pref.IsAlive() && pref.HasKey)
            {
                try
                {
                    var activityView = pref.Context as IActivityView;
                    if (activityView != null)
                    {
                        var key = pref.Key + pref.GetType().FullName + pref.GetHashCode();
                        if (activityView.Mediator.Metadata.Remove(key))
                            return true;
                    }
                }
                catch
                {
                    ;
                }
            }
            return false;
        }

        #endregion

        #region Implementation of interfaces

        public bool Load(IModuleContext context)
        {
            IAttachedValueProvider service;
            if (context.IocContainer != null && context.IocContainer.TryGet(out service))
            {
                var attachedValueProvider = service as AttachedValueProvider;
                if (attachedValueProvider != null)
                {
                    attachedValueProvider.ClearHandler += ClearAttachedValue;
                    attachedValueProvider.GetOrAddAttachedDictionaryHandler += GetOrAddAttachedDictionaryHandler;
                }
            }

            var mediatorFactory = PlatformExtensions.MediatorFactory;
            PlatformExtensions.MediatorFactory = (item, dataContext, mediatorType) =>
            {
#if !APPCOMPAT                
                if (item is PreferenceActivity && typeof(IMvvmActivityMediator).IsAssignableFrom(mediatorType))
                    return new MvvmPreferenceActivityMediator((PreferenceActivity)item);
#endif
                if (item is Fragment && typeof(IMvvmFragmentMediator).IsAssignableFrom(mediatorType))
                    return new MvvmPreferenceFragmentMediator((Fragment)item);
                return mediatorFactory?.Invoke(item, dataContext, mediatorType);
            };

            PreferenceCompatAttachedMembersRegistration.RegisterPreferenceMembers();
            PreferenceCompatAttachedMembersRegistration.RegisterEditTextPreferenceMembers();
            PreferenceCompatAttachedMembersRegistration.RegisterListPreferenceMembers();
            PreferenceCompatAttachedMembersRegistration.RegisterPreferenceGroupMembers();
            PreferenceCompatAttachedMembersRegistration.RegisterTwoStatePreferenceMembers();
#if !APPCOMPAT
            PreferenceCompatAttachedMembersRegistration.RegisterMultiSelectListPreferenceMembers();
#endif
            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}