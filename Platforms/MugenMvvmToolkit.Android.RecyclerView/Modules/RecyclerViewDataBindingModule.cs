#region Copyright

// ****************************************************************************
// <copyright file="RecyclerViewDataBindingModule.cs">
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

using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Android.RecyclerView.Modules
{
    public class RecyclerViewDataBindingModule : IModule
    {
        #region Implementation of IModule

        public int Priority => ApplicationSettings.ModulePriorityInitialization;

        public bool Load(IModuleContext context)
        {
            AttachedMembersRegistration.RegisterRecyclerViewMembers();
            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}