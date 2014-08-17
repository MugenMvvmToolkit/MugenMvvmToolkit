#region Copyright
// ****************************************************************************
// <copyright file="ObserverProvider.cs">
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
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the observer provider that allows to create an observer.
    /// </summary>
    public class ObserverProvider : IObserverProvider
    {
        #region Implementation of IObserverProvider

        /// <summary>
        ///     Attempts to track the value change using the binding path.
        /// </summary>
        public virtual IObserver Observe(object target, IBindingPath path, bool ignoreAttachedMembers)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNull(path, "path");
            if (path.IsSingle)
                return new SinglePathObserver(target, path, ignoreAttachedMembers);
            if (path.IsEmpty)
                return new EmptyPathObserver(target, path);
            return new MultiPathObserver(target, path, ignoreAttachedMembers);
        }

        #endregion
    }
}