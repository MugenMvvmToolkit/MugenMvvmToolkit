#region Copyright
// ****************************************************************************
// <copyright file="InternalExtensions.cs">
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
using System;
using System.Reflection;

namespace MugenMvvmToolkit.Binding
{
    internal static class InternalExtensions
    {
        public static bool IsAssignableFrom(this Type type, Type c)
        {
            if (c == null)
                return false;
            return type.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
        }
    }
}