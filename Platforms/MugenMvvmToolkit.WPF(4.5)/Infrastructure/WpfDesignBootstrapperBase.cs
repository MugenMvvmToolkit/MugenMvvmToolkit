#region Copyright

// ****************************************************************************
// <copyright file="WpfDesignBootstrapperBase.cs">
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

using System.Windows.Navigation;
using MugenMvvmToolkit.WPF.Interfaces.Navigation;

namespace MugenMvvmToolkit.WPF.Infrastructure
{
    public abstract class WpfDesignBootstrapperBase : WpfBootstrapperBase
    {
        #region Constructors

        protected WpfDesignBootstrapperBase() : base(ServiceProvider.IsDesignMode)
        {
        }

        #endregion

        #region Methods

        public sealed override void Start()
        {
            base.Start();
        }

        protected sealed override INavigationService CreateNavigationService(NavigationWindow window)
        {
            return base.CreateNavigationService(window);
        }

        protected sealed override NavigationWindow CreateNavigationWindow()
        {
            return base.CreateNavigationWindow();
        }

        #endregion
    }
}