#region Copyright

// ****************************************************************************
// <copyright file="FragmentInitializationModule.cs">
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

using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Modules;
#if APPCOMPAT
using MugenMvvmToolkit.Android.AppCompat.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Android.AppCompat.Modules
#else
using MugenMvvmToolkit.Android.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Android.Modules
#endif
{
    public class FragmentInitializationModule : ModuleBase
    {
        #region Constructors

        public FragmentInitializationModule()
            : base(false, MugenMvvmToolkit.Models.LoadMode.All, InitializationModulePriority - 1)
        {
        }

        #endregion

        #region Overrides of ModuleBase

        protected override bool LoadInternal()
        {
            IViewModelPresenter service;
#if APPCOMPAT
            if (!IocContainer.TryGet(out service))
                return false;
#else
            if (!PlatformExtensions.IsApiGreaterThanOrEqualTo17 || !IocContainer.TryGet(out service))
                return false;
#endif
            service.DynamicPresenters.Add(IocContainer.Get<DynamicViewModelWindowPresenter>());
            return true;
        }

        protected override void UnloadInternal()
        {
        }

        #endregion
    }
}
