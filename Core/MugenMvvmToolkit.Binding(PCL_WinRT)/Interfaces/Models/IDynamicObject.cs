#region Copyright
// ****************************************************************************
// <copyright file="IDynamicObject.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    /// <summary>
    ///     Provides a simple ineface that can be inherited from to create an object with dynamic behavior at runtime.
    /// </summary>
    public interface IDynamicObject
    {
        /// <summary>
        ///     Provides the implementation of getting a member.
        /// </summary>
        /// <returns>The result of the get operation.</returns>
        [CanBeNull]
        object GetMember([NotNull]string member, IList<object> args);

        /// <summary>
        ///     Provides the implementation of setting a member.
        /// </summary>
        void SetMember([NotNull]string member, IList<object> args);
    }
}