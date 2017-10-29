#region Copyright

// ****************************************************************************
// <copyright file="UwpDesignBootstrapperBase.cs">
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

namespace MugenMvvmToolkit.UWP.Infrastructure
{
    public abstract class UwpDesignBootstrapperBase : UwpBootstrapperBase
    {
        #region Constructors

        protected UwpDesignBootstrapperBase() : base(ToolkitServiceProvider.IsDesignMode)
        {
        }

        #endregion

        #region Methods

        public sealed override void Start()
        {
            base.Start();
        }

        #endregion
    }
}