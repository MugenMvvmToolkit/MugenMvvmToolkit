#region Copyright

// ****************************************************************************
// <copyright file="BindingConstants.cs">
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

using System;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.DataConstants
{
    public static class BindingConstants
    {
        #region Fields

        public static readonly DataConstant DoNothing;
        public static readonly DataConstant UnsetValue;
        public static readonly DataConstant InvalidValue;
        public static readonly DataConstant<IDataBinding> Binding;
        public static readonly DataConstant<object> CurrentEventArgs;
        public static readonly DataConstant<WeakReference> Source;
        public static readonly DataConstant<bool> ClearErrors;

        #endregion

        #region Constructors

        static BindingConstants()
        {
            var type = typeof(BindingConstants);
            Source = DataConstant.Create<WeakReference>(type, nameof(Source), true);
            DoNothing = DataConstant.Create(type, nameof(DoNothing));
            UnsetValue = DataConstant.Create(type, nameof(UnsetValue));
            InvalidValue = DataConstant.Create(type, nameof(InvalidValue));
            Binding = DataConstant.Create<IDataBinding>(type, nameof(Binding), true);
            CurrentEventArgs = DataConstant.Create<object>(type, nameof(CurrentEventArgs), false);
            ClearErrors = DataConstant.Create<bool>(type, nameof(ClearErrors));
        }

        #endregion
    }
}
