#region Copyright

// ****************************************************************************
// <copyright file="PreferenceAttachedMembersRegistration.cs">
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
using System.Collections.Generic;
using System.Linq;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;

#if APPCOMPAT
using Android.Support.V7.Preferences;
using MugenMvvmToolkit.Binding.Interfaces;
using AttachedMembers = MugenMvvmToolkit.Android.PreferenceCompat.PreferenceCompatAttachedMembers;

namespace MugenMvvmToolkit.Android.PreferenceCompat
{
    public static class PreferenceCompatAttachedMembersRegistration
#else
using Android.Preferences;
using MugenMvvmToolkit.Android.Binding.Infrastructure;

namespace MugenMvvmToolkit.Android.Binding
{
    partial class AttachedMembersRegistration
#endif
    {
        private static IBindingMemberInfo _valueChangedEventMember;

#if APPCOMPAT

        #region Properties

        private static IBindingMemberProvider MemberProvider => BindingServiceProvider.MemberProvider;

        #endregion
#endif

        #region Methods

#if !APPCOMPAT
        public static void RegisterMultiSelectListPreferenceMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<MultiSelectListPreference>(nameof(MultiSelectListPreference.Values));
            if (_valueChangedEventMember != null)
            {
                BindingServiceProvider.MemberProvider.Register(typeof(MultiSelectListPreference), "ValuesChanged", _valueChangedEventMember, true);
                BindingServiceProvider.MemberProvider.Register(typeof(MultiSelectListPreference), "EntryChanged", _valueChangedEventMember, true);
            }
            MemberProvider.Register(
                AttachedBindingMember.CreateMember<MultiSelectListPreference, IEnumerable<string>>("EntryValues",
                    (info, preference) => preference.GetEntryValues(),
                    (info, preference, arg3) =>
                        preference.SetEntryValues(arg3?.ToArray() ?? Empty.Array<string>())));
            MemberProvider.Register(
                AttachedBindingMember.CreateMember<MultiSelectListPreference, IEnumerable<string>>("Entries",
                    (info, preference) => preference.GetEntries(),
                    (info, preference, arg3) =>
                        preference.SetEntries(arg3?.ToArray() ?? Empty.Array<string>())));
        }
#endif

        public static void RegisterPreferenceMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.Preference.Click);
            var changeMember = AttachedBindingMember.CreateEvent(AttachedMembers.Preference.ValueChangedEvent);
            BindingServiceProvider.MemberProvider.Register(changeMember);
            _valueChangedEventMember = changeMember;
        }

        public static void RegisterTwoStatePreferenceMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<TwoStatePreference>(nameof(TwoStatePreference.Checked));
            if (_valueChangedEventMember != null)
                BindingServiceProvider.MemberProvider.Register(typeof(TwoStatePreference), "CheckedChanged", _valueChangedEventMember, true);
        }

        public static void RegisterListPreferenceMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<ListPreference>(nameof(ListPreference.Value));
            if (_valueChangedEventMember != null)
                BindingServiceProvider.MemberProvider.Register(typeof(ListPreference), "EntryChanged", _valueChangedEventMember, true);
            MemberProvider.Register(
                AttachedBindingMember.CreateMember<ListPreference, IEnumerable<string>>("EntryValues",
                    (info, preference) => preference.GetEntryValues(),
                    (info, preference, arg3) =>
                        preference.SetEntryValues(arg3?.ToArray() ?? Empty.Array<string>())));
            MemberProvider.Register(
                AttachedBindingMember.CreateMember<ListPreference, IEnumerable<string>>("Entries",
                    (info, preference) => preference.GetEntries(),
                    (info, preference, arg3) =>
                        preference.SetEntries(arg3?.ToArray() ?? Empty.Array<string>())));
        }

        public static void RegisterEditTextPreferenceMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<EditTextPreference>(nameof(EditTextPreference.Text));
            if (_valueChangedEventMember != null)
                BindingServiceProvider.MemberProvider.Register(typeof(EditTextPreference), "TextChanged", _valueChangedEventMember, true);
        }

        public static void RegisterPreferenceGroupMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.PreferenceGroup.ItemsSource);
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.PreferenceGroup.ItemsSource, PreferenceGroupItemsSourceChanged));
            var templateMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.PreferenceGroup.ItemTemplateSelector);
            MemberProvider.Register(templateMember);
            MemberProvider.Register(AttachedMemberConstants.ItemTemplate, templateMember);
        }

        private static void PreferenceGroupItemsSourceChanged(PreferenceGroup preference, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var sourceGenerator = preference.GetBindingMemberValue(AttachedMembers.PreferenceGroup.ItemsSourceGenerator);
            if (sourceGenerator == null)
            {
                sourceGenerator = new PreferenceItemsSourceGenerator(preference);
                preference.SetBindingMemberValue(AttachedMembers.PreferenceGroup.ItemsSourceGenerator, sourceGenerator);
            }
            sourceGenerator.SetItemsSource(args.NewValue);
        }

        #endregion
    }
}