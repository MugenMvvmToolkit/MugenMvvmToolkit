#region Copyright

// ****************************************************************************
// <copyright file="InternalExtensions.cs">
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
using System.Reflection;
using Windows.UI.Xaml;

namespace MugenMvvmToolkit.Binding
{
    internal static class InternalExtensions
    {
        #region Methods

        public static bool IsAssignableFrom(this Type type, Type c)
        {
            if (c == null)
                return false;
            return type.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
        }

        internal static bool Contains(this ResourceDictionary dictionary, string key)
        {
            return dictionary.ContainsKey(key);
        }

        #endregion
    }
}