#region Copyright

// ****************************************************************************
// <copyright file="Toolkit.Annotations.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Annotations
{
    /// <summary>
    ///     Indicates that the class can be used in context action inherit from.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [BaseTypeRequired(typeof(IViewModel))]
    public sealed class BaseViewModelAttribute : Attribute
    {
        #region Properties

        /// <summary>
        ///     A number indicating the priority of the action. Affects the position of the action in the pop-up menu.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        ///     Gets or sets the name, default is interface name.
        /// </summary>
        public string Name { get; set; }

        #endregion
    }

    /// <summary>
    ///     Indicates that the class can be used in context action wrap to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    [BaseTypeRequired(typeof(IViewModel))]
    public sealed class WrapperAttribute : Attribute
    {
        #region Properties

        /// <summary>
        ///     A number indicating the priority of the action. Affects the position of the action in the pop-up menu.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        ///     Gets or sets the name, default is member name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the supported types.
        /// </summary>
        public Type[] Types { get; set; }

        #endregion
    }

    /// <summary>
    ///     Indicates that the value of the marked parameter could never be <c>empty</c>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class NotEmptyParamsAttribute : Attribute
    {
    }

    /// <summary>
    ///     When applied to a target attribute, specifies a requirement for any parameter marked with
    ///     the target attribute to implement or inherit view model type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class ViewModelTypeRequiredAttribute : Attribute
    {
    }

    /// <summary>
    ///     When applied to the member of a type, specifies that the member is not required an TaskBusyHandler.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property,
        AllowMultiple = false, Inherited = true)]
    public sealed class SuppressTaskBusyHandlerAttribute : Attribute
    {
    }
}