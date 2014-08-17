#region Copyright
// ****************************************************************************
// <copyright file="IObserverProvider.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the observer provider that allows to create an observer.
    /// </summary>
    public interface IObserverProvider
    {
        /// <summary>
        ///     Attempts to track the value change using the binding path.
        /// </summary>
        [NotNull]
        IObserver Observe([NotNull] object target, [NotNull] IBindingPath path, bool ignoreAttachedMembers);
    }
}