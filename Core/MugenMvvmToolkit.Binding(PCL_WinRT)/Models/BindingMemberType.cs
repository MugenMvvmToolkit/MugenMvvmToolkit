#region Copyright
// ****************************************************************************
// <copyright file="BindingMemberType.cs">
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
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the type of member
    /// </summary>
    public class BindingMemberType : StringConstantBase<BindingMemberType>
    {
        #region Fields

        /// <summary>
        ///     Indicates that the type is a field.
        /// </summary>
        public static readonly BindingMemberType Field = new BindingMemberType("Field");

        /// <summary>
        ///     Indicates that the type is a property.
        /// </summary>
        public static readonly BindingMemberType Property = new BindingMemberType("Property");

        /// <summary>
        ///     Indicates that the type is a array.
        /// </summary>
        public static readonly BindingMemberType Array = new BindingMemberType("Array");

        /// <summary>
        ///     Indicates that the type is a dependency property.
        /// </summary>
        public static readonly BindingMemberType DependencyProperty = new BindingMemberType("DependencyProperty");

        /// <summary>
        ///     Indicates that the type is an event.
        /// </summary>
        public static readonly BindingMemberType Event = new BindingMemberType("Event");

        /// <summary>
        ///     Indicates that the type is a binding context.
        /// </summary>
        public static readonly BindingMemberType BindingContext = new BindingMemberType("BindingContext");

        /// <summary>
        ///     Indicates that the type is empty value.
        /// </summary>
        public static readonly BindingMemberType Empty = new BindingMemberType("#Empty");

        /// <summary>
        ///     Indicates that the type is unset value.
        /// </summary>
        public static readonly BindingMemberType Unset = new BindingMemberType("#Unset");

        /// <summary>
        ///     Indicates that the type is attached member.
        /// </summary>
        public static readonly BindingMemberType Attached = new BindingMemberType("Attached");

        /// <summary>
        ///     Indicates that the type is a dynamic object.
        /// </summary>
        public static readonly BindingMemberType Dynamic = new BindingMemberType("Dynamic");

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberType" /> class.
        /// </summary>
        public BindingMemberType(string id) : base(id)
        {
        }

        #endregion
    }
}