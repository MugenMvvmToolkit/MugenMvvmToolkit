#region Copyright

// ****************************************************************************
// <copyright file="DesignModule.cs">
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

using MugenMvvmToolkit.Android.Design.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;

namespace MugenMvvmToolkit.Android.Design.Modules
{
    public class DesignModule : IModule
    {
        #region Implementation of IModule

        public int Priority => ApplicationSettings.ModulePriorityBinding - 1;

        public bool Load(IModuleContext context)
        {
            AttachedMembersRegistration.RegisterNavigationViewMembers();
            AttachedMembersRegistration.RegisterTabLayoutMembers();
            AttachedMembersRegistration.RegisterTabLayoutTabMembers();
            AttachedMembersRegistration.RegisterTextInputLayoutMembers();
            AttachedMembersRegistration.RegisterSnakbarMembers();
            if (context.IocContainer != null)
            {
                IToastPresenter toastPresenter;
                context.IocContainer.TryGet(out toastPresenter);
                context.IocContainer.BindToConstant<IToastPresenter>(new SnackbarToastPresenter(context.IocContainer.Get<IThreadManager>(), toastPresenter));
            }

            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}