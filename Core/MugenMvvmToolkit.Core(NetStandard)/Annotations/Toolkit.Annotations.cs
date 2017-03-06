#region Copyright

// ****************************************************************************
// <copyright file="Toolkit.Annotations.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [BaseTypeRequired(typeof(IViewModel))]
    public sealed class BaseViewModelAttribute : Attribute
    {
        #region Properties

        public int Priority { get; set; }

        public string Name { get; set; }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    [BaseTypeRequired(typeof(IViewModel))]
    public sealed class WrapperAttribute : Attribute
    {
        #region Properties

        public int Priority { get; set; }

        public string Name { get; set; }

        public Type[] Types { get; set; }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class NotEmptyParamsAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class ViewModelTypeRequiredAttribute : Attribute
    {
    }

    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property,
        AllowMultiple = false, Inherited = true)]
    public sealed class SuppressTaskBusyHandlerAttribute : Attribute
    {
    }
}
