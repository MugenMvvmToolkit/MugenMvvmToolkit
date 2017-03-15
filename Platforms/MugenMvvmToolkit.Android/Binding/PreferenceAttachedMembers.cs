#region Copyright

// ****************************************************************************
// <copyright file="PreferenceAttachedMembers.cs">
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

using System.Collections;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

#if APPCOMPAT
using MugenMvvmToolkit.Android.Binding;
using Pref = Android.Support.V7.Preferences.Preference;
using PrefGroup = Android.Support.V7.Preferences.PreferenceGroup;

namespace MugenMvvmToolkit.Android.PreferenceCompat
{
    public static class PreferenceCompatAttachedMembers
#else
using Pref = Android.Preferences.Preference;
using PrefGroup = Android.Preferences.PreferenceGroup;

namespace MugenMvvmToolkit.Android.Binding
{
    partial class AttachedMembers
#endif
    {
        #region Nested types

        public abstract class Preference : AttachedMembers.Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<Pref, bool> Click;
            public static readonly BindingMemberDescriptor<Pref, IEventListener> ValueChangedEvent;

            #endregion

            #region Constructors

            static Preference()
            {
                Click = new BindingMemberDescriptor<Pref, bool>("PreferenceClick");
                ValueChangedEvent = new BindingMemberDescriptor<Pref, IEventListener>("ValueChanged");
            }

            #endregion
        }

        public abstract class PreferenceGroup : AttachedMembers.Preference
        {
            #region Fields

            public static readonly BindingMemberDescriptor<PrefGroup, IEnumerable> ItemsSource;
            public static readonly BindingMemberDescriptor<PrefGroup, IItemsSourceGenerator> ItemsSourceGenerator;
            public static readonly BindingMemberDescriptor<PrefGroup, IDataTemplateSelector> ItemTemplateSelector;
            public static readonly BindingMemberDescriptor<PrefGroup, ICollectionViewManager> CollectionViewManager;

            #endregion

            #region Constructors

            static PreferenceGroup()
            {
                ItemsSource = new BindingMemberDescriptor<PrefGroup, IEnumerable>(AttachedMemberConstants.ItemsSource);
                ItemsSourceGenerator = ItemsSourceGeneratorBase.MemberDescriptor.Override<PrefGroup>();
                ItemTemplateSelector = new BindingMemberDescriptor<PrefGroup, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
                CollectionViewManager = AttachedMembers.ViewGroup.CollectionViewManager.Override<PrefGroup>();
            }

            #endregion
        }

        #endregion
    }
}