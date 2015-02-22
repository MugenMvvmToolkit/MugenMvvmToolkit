#region Copyright

// ****************************************************************************
// <copyright file="BindingConstants.cs">
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

        #endregion

        #region Constructors

        static BindingConstants()
        {
            Source = DataConstant.Create(() => Source, true);
            DoNothing = DataConstant.Create(() => DoNothing);
            UnsetValue = DataConstant.Create(() => UnsetValue);
            InvalidValue = DataConstant.Create(() => InvalidValue);
            Binding = DataConstant.Create(() => Binding, true);
            CurrentEventArgs = DataConstant.Create(() => CurrentEventArgs, false);
        }

        #endregion
    }
}