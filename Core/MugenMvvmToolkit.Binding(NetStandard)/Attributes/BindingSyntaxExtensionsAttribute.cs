#region Copyright

// ****************************************************************************
// <copyright file="BindingSyntaxExtensionsAttribute.cs">
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
using System.Reflection;

namespace MugenMvvmToolkit.Binding.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class BindingSyntaxExtensionsAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class BindingSyntaxMemberAttribute : Attribute
    {
        #region Fields

        private readonly string _memberName;

        #endregion

        #region Constructors

        public BindingSyntaxMemberAttribute(string memberName)
        {
            _memberName = memberName;
        }

        public BindingSyntaxMemberAttribute()
        {
        }

        #endregion

        #region Methods

        public string GetMemberName(MemberInfo member)
        {
            if (string.IsNullOrEmpty(_memberName))
                return member.Name;
            return _memberName;
        }

        #endregion
    }
}
