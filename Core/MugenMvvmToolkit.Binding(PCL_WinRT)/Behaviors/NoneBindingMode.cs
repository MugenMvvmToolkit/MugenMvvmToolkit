#region Copyright

// ****************************************************************************
// <copyright file="NoneBindingMode.cs">
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

using System;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    public sealed class NoneBindingMode : IBindingBehavior
    {
        #region Fields

        public static readonly NoneBindingMode Instance;

        #endregion

        #region Constructors

        static NoneBindingMode()
        {
            Instance = new NoneBindingMode();
        }

        private NoneBindingMode()
        {
        }

        #endregion

        #region Implementation of IBindingBehavior

        public Guid Id
        {
            get { return BindingModeBase.IdBindingMode; }
        }

        public int Priority
        {
            get { return int.MinValue; }
        }

        bool IBindingBehavior.Attach(IDataBinding binding)
        {
            return true;
        }

        void IBindingBehavior.Detach(IDataBinding binding)
        {
        }

        public IBindingBehavior Clone()
        {
            return this;
        }

        #endregion
    }
}
