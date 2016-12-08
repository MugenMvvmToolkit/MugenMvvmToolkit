#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModule.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.WinForms.Binding.Infrastructure;

namespace MugenMvvmToolkit.WinForms.Binding.Modules
{
    public class PlatformDataBindingModule : IModule
    {
        #region Properties

        public int Priority => ApplicationSettings.ModulePriorityBinding + 1;

        #endregion

        #region Implementation of interfaces

        public bool Load(IModuleContext context)
        {
            if (context.PlatformInfo.Platform == PlatformType.WinForms)
                BindingServiceProvider.Initialize(errorProvider: new WinFormsBindingErrorProvider(), converter: BindingReflectionExtensions.Convert);

            context.TryRegisterDataTemplateSelectorsAndValueConverters(null);
            MugenMvvmToolkit.Binding.AttachedMembersRegistration.RegisterDefaultMembers();

            AttachedMembersRegistration.RegisterObjectMembers();
            AttachedMembersRegistration.RegisterButtonMembers();
            AttachedMembersRegistration.RegisterTextBoxMembers();
            AttachedMembersRegistration.RegisterLabelMembers();
            AttachedMembersRegistration.RegisterCheckBoxMembers();
            AttachedMembersRegistration.RegisterProgressBarMembers();
            AttachedMembersRegistration.RegisterFormMembers();
            AttachedMembersRegistration.RegisterControlMembers();
            AttachedMembersRegistration.RegisterDateTimePickerMembers();
            AttachedMembersRegistration.RegisterToolStripItemMembers();
            AttachedMembersRegistration.RegisterTabControlMembers();
            AttachedMembersRegistration.RegisterComboBoxMembers();
            AttachedMembersRegistration.RegisterDataGridViewMembers();
            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}