#region Copyright

// ****************************************************************************
// <copyright file="BindingSyntaxExtensionsAttribute.cs">
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

namespace MugenMvvmToolkit.Binding.Attributes
{
    /// <summary>
    ///     Indicates that the class can be used as syntax extension, class should be static and have a method <c>(Expression ProvideExpression(IBuilderSyntaxContext context))</c> method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class BindingSyntaxExtensionsAttribute : Attribute
    {
    }

    /// <summary>
    ///     When applied to the member of a type, specifies that the member is binding member name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class BindingSyntaxMemberAttribute : Attribute
    {
    }
}