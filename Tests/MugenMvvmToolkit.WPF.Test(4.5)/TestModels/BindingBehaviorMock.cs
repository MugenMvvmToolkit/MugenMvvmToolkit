#region Copyright

// ****************************************************************************
// <copyright file="BindingBehaviorMock.cs">
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

using System;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class BindingBehaviorMock : IBindingBehavior
    {
        #region Properties

        public Action<IDataBinding> Detach { get; set; }

        public Func<IDataBinding, bool> Attach { get; set; }

        #endregion

        #region Implementation of IBindingBehavior

        public Guid Id { get; set; }

        public int Priority { get; set; }

        void IBindingBehavior.Detach(IDataBinding binding)
        {
            Detach(binding);
        }

        bool IBindingBehavior.Attach(IDataBinding binding)
        {
            return Attach(binding);
        }

        public IBindingBehavior Clone()
        {
            return this;
        }

        #endregion
    }
}
