#region Copyright

// ****************************************************************************
// <copyright file="BindingMemberType.cs">
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

using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    public sealed class BindingMemberType : StringConstantBase<BindingMemberType>
    {
        #region Fields

        public static readonly BindingMemberType Field;

        public static readonly BindingMemberType Property;

        public static readonly BindingMemberType Array;

        public static readonly BindingMemberType DependencyProperty;

        public static readonly BindingMemberType Event;

        public static readonly BindingMemberType BindingContext;

        public static readonly BindingMemberType Empty;

        public static readonly BindingMemberType Unset;

        public static readonly BindingMemberType Attached;

        public static readonly BindingMemberType Dynamic;

        #endregion

        #region Constructors

        static BindingMemberType()
        {
            Field = new BindingMemberType("Field");
            Property = new BindingMemberType("Property");
            Array = new BindingMemberType("Array");
            DependencyProperty = new BindingMemberType("DependencyProperty");
            Event = new BindingMemberType("Event");
            BindingContext = new BindingMemberType("BindingContext");
            Empty = new BindingMemberType("#Empty");
            Unset = new BindingMemberType("#Unset");
            Attached = new BindingMemberType("Attached");
            Dynamic = new BindingMemberType("Dynamic");
        }

        public BindingMemberType(string id)
            : base(id)
        {
        }

        #endregion
    }
}
