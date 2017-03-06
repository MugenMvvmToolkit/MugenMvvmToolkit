#region Copyright

// ****************************************************************************
// <copyright file="AndroidDataBindingModule.cs">
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

using Android.App;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Android.Binding.Modules
{
    public class AndroidDataBindingModule : IModule
    {
        #region Properties

        public int Priority => ApplicationSettings.ModulePriorityInitialization + 1;

        #endregion

        #region Implementation of interfaces

        public bool Load(IModuleContext context)
        {
            if (context.PlatformInfo.Platform == PlatformType.Android)
            {
                BindingServiceProvider.Initialize(errorProvider: new AndroidBindingErrorProvider(), converter: BindingConverterExtensions.Convert);
                BindingServiceProvider.BindingProvider.DefaultBehaviors.Add(DisableEqualityCheckingBehavior.TargetTrueNotTwoWay);
            }

            PlatformExtensions.ItemsSourceAdapterFactory = (o, ctx, arg3) => new ItemsSourceAdapter(o, ctx, !ReferenceEquals(ViewGroupItemsSourceGenerator.Context, arg3));
            context.TryRegisterDataTemplateSelectorsAndValueConverters(null);
            MugenMvvmToolkit.Binding.AttachedMembersRegistration.RegisterDefaultMembers();

            if (PlatformExtensions.IsApiGreaterThanOrEqualTo14)
            {
                var isActionBar = PlatformExtensions.IsActionBar;
                var isFragment = PlatformExtensions.IsFragment;
                PlatformExtensions.IsActionBar = o => isActionBar(o) || o is ActionBar;
                PlatformExtensions.IsFragment = o => isFragment(o) || o is Fragment;

                AttachedMembersRegistration.RegisterActionBarBaseMembers();
                AttachedMembersRegistration.RegisterActionBarMembers();
                AttachedMembersRegistration.RegisterActionBarTabMembers();
            }

            AttachedMembersRegistration.RegisterObjectMembers();
            AttachedMembersRegistration.RegisterViewBaseMembers();
            AttachedMembersRegistration.RegisterViewMembers();
            AttachedMembersRegistration.RegisterDialogMembers();
            AttachedMembersRegistration.RegisterActivityMembers();
            AttachedMembersRegistration.RegisterRatingBarMembers();
            AttachedMembersRegistration.RegisterAdapterViewBaseMembers();
            AttachedMembersRegistration.RegisterAdapterViewMembers();
            AttachedMembersRegistration.RegisterViewGroupMembers();
            AttachedMembersRegistration.RegisterTabHostMembers();
            AttachedMembersRegistration.RegisterTextViewMembers();
            AttachedMembersRegistration.RegisterAutoCompleteTextViewMembers();
            AttachedMembersRegistration.RegisterDatePickerMembers();
            AttachedMembersRegistration.RegisterTimePickerMembers();
            AttachedMembersRegistration.RegisterImageViewMembers();
            AttachedMembersRegistration.RegisterToolbarMembers();
            AttachedMembersRegistration.RegisterButtonMembers();
            AttachedMembersRegistration.RegisterCompoundButtonMembers();
            AttachedMembersRegistration.RegisterSeekBarMembers();
            AttachedMembersRegistration.RegisterMenuMembers();
            AttachedMembersRegistration.RegisterMenuItemBaseMembers();
            AttachedMembersRegistration.RegisterMenuItemMembers();
            AttachedMembersRegistration.RegisterPreferenceMembers();
            AttachedMembersRegistration.RegisterTwoStatePreferenceMembers();
            AttachedMembersRegistration.RegisterListPreferenceMembers();
            AttachedMembersRegistration.RegisterMultiSelectListPreferenceMembers();
            AttachedMembersRegistration.RegisterEditTextPreferenceMembers();
            AttachedMembersRegistration.RegisterPreferenceGroupMembers();
            AttachedMembersRegistration.RegisterPopupMenuMembers();
            AttachedMembersRegistration.RegisterMenuItemActionViewMembers();
            AttachedMembersRegistration.RegisterMenuItemActionProviderMembers();
            AttachedMembersRegistration.RegisterSearchViewMembers();
            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}