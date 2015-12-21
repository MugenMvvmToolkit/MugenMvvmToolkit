#region Copyright

// ****************************************************************************
// <copyright file="PreferenceDataBindingModule.cs">
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Android.Preferences;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Android.Binding.Modules
{
    public partial class PlatformDataBindingModule
    {
        #region Methods

        private static void RegisterPreferenceMembers(IBindingMemberProvider memberProvider)
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.Preference.Click);
            BindingBuilderExtensions.RegisterDefaultBindingMember<TwoStatePreference>(nameof(TwoStatePreference.Checked));
            BindingBuilderExtensions.RegisterDefaultBindingMember<ListPreference>(nameof(ListPreference.Value));
            BindingBuilderExtensions.RegisterDefaultBindingMember<MultiSelectListPreference>(nameof(MultiSelectListPreference.Values));
            BindingBuilderExtensions.RegisterDefaultBindingMember<EditTextPreference>(nameof(EditTextPreference.Text));
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.PreferenceGroup.ItemsSource);
            var changeMember = AttachedBindingMember.CreateEvent(AttachedMembers.Preference.ValueChangedEvent);
            BindingServiceProvider.MemberProvider.Register(changeMember);
            BindingServiceProvider.MemberProvider.Register(typeof(TwoStatePreference), "CheckedChanged", changeMember, true);
            BindingServiceProvider.MemberProvider.Register(typeof(MultiSelectListPreference), "ValuesChanged", changeMember, true);
            BindingServiceProvider.MemberProvider.Register(typeof(ListPreference), "EntryChanged", changeMember, true);
            BindingServiceProvider.MemberProvider.Register(typeof(MultiSelectListPreference), "EntryChanged", changeMember, true);
            BindingServiceProvider.MemberProvider.Register(typeof(EditTextPreference), "TextChanged", changeMember, true);


            //ListPreference
            memberProvider.Register(
                AttachedBindingMember.CreateMember<ListPreference, IEnumerable<string>>("EntryValues",
                    (info, preference) => preference.GetEntryValues(),
                    (info, preference, arg3) =>
                        preference.SetEntryValues(arg3 == null ? Empty.Array<string>() : arg3.ToArray())));
            memberProvider.Register(
                AttachedBindingMember.CreateMember<ListPreference, IEnumerable<string>>("Entries",
                    (info, preference) => preference.GetEntries(),
                    (info, preference, arg3) =>
                        preference.SetEntries(arg3 == null ? Empty.Array<string>() : arg3.ToArray())));

            //MultiSelectListPreference
            memberProvider.Register(
                AttachedBindingMember.CreateMember<MultiSelectListPreference, IEnumerable<string>>("EntryValues",
                    (info, preference) => preference.GetEntryValues(),
                    (info, preference, arg3) =>
                        preference.SetEntryValues(arg3 == null ? Empty.Array<string>() : arg3.ToArray())));
            memberProvider.Register(
                AttachedBindingMember.CreateMember<MultiSelectListPreference, IEnumerable<string>>("Entries",
                    (info, preference) => preference.GetEntries(),
                    (info, preference, arg3) =>
                        preference.SetEntries(arg3 == null ? Empty.Array<string>() : arg3.ToArray())));

            //PreferenceGroup
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.PreferenceGroup.ItemsSource, PreferenceGroupItemsSourceChanged));
            var templateMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.PreferenceGroup.ItemTemplateSelector);
            memberProvider.Register(templateMember);
            memberProvider.Register(AttachedMemberConstants.ItemTemplate, templateMember);
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
