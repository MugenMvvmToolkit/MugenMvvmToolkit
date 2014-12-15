#region Copyright
// ****************************************************************************
// <copyright file="FragmentInitializationModule.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Modules;

#if APPCOMPAT
using MugenMvvmToolkit.AppCompat.Infrastructure.Presenters;

namespace MugenMvvmToolkit.AppCompat.Modules
#else
using MugenMvvmToolkit.FragmentSupport.Infrastructure.Presenters;

namespace MugenMvvmToolkit.FragmentSupport.Modules
#endif
{
    public class FragmentInitializationModule : ModuleBase
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModuleBase" /> class.
        /// </summary>
        public FragmentInitializationModule()
            : base(false, MugenMvvmToolkit.Models.LoadMode.All, InitializationModulePriority - 1)
        {
        }

        #endregion

        #region Overrides of ModuleBase

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        protected override bool LoadInternal()
        {
            IViewModelPresenter service;
            if (!IocContainer.TryGet(out service))
                return false;
            service.DynamicPresenters.Add(new DynamicViewModelWindowPresenter(IocContainer.Get<IViewMappingProvider>(),
                IocContainer.Get<IViewManager>(),
                IocContainer.Get<IWrapperManager>(), IocContainer.Get<IThreadManager>(),
                IocContainer.Get<IOperationCallbackManager>()));
            return true;
        }

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        protected override void UnloadInternal()
        {
        }

        #endregion
    }
}