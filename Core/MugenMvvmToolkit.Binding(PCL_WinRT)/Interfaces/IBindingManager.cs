#region Copyright
// ****************************************************************************
// <copyright file="IBindingManager.cs">
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
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the binding manager.
    /// </summary>
    public interface IBindingManager
    {
        /// <summary>
        ///     Registers the specified binding.
        /// </summary>
        /// <param name="target">The specified target.</param>
        /// <param name="path">The specified path.</param>
        /// <param name="binding">The specified <see cref="IDataBinding" />.</param>
        /// <param name="context">The specified <see cref="IDataContext"/>, if any.</param>
        void Register([NotNull] object target, [NotNull] string path, [NotNull] IDataBinding binding, IDataContext context = null);

        /// <summary>
        ///     Determine whether the specified binding is available in the <see cref="IBindingManager" />.
        /// </summary>
        /// <param name="binding">The <see cref="IDataBinding" /> to test for the registration of.</param>
        /// <returns>
        ///     True if the binding is registered.
        /// </returns>
        bool IsRegistered([NotNull] IDataBinding binding);

        /// <summary>
        ///     Retrieves the <see cref="IDataBinding" /> objects.
        /// </summary>
        /// <param name="target">The object to get bindings.</param>
        /// <param name="context">The specified <see cref="IDataContext"/>, if any.</param>
        IEnumerable<IDataBinding> GetBindings([NotNull] object target, IDataContext context = null);

        /// <summary>
        ///     Retrieves the <see cref="IDataBinding" /> objects that is set on the specified property.
        /// </summary>
        /// <param name="target">The object where <paramref name="path" /> is.</param>
        /// <param name="path">The binding target property from which to retrieve the binding.</param>
        /// <param name="context">The specified <see cref="IDataContext"/>, if any.</param>
        IEnumerable<IDataBinding> GetBindings([NotNull] object target, [NotNull] string path, IDataContext context = null);

        /// <summary>
        ///     Unregisters the specified <see cref="IDataBinding"/>.
        /// </summary>        
        void Unregister(IDataBinding binding);

        /// <summary>
        ///     Removes all bindings from the specified target.
        /// </summary>
        /// <param name="target">The object from which to remove bindings.</param>
        /// <param name="context">The specified <see cref="IDataContext"/>, if any.</param>
        void ClearBindings([NotNull] object target, IDataContext context = null);

        /// <summary>
        ///     Removes the bindings from a property if there is one.
        /// </summary>
        /// <param name="target">The object from which to remove the bindings.</param>
        /// <param name="path">The property path from which to remove the bindings.</param>
        /// <param name="context">The specified <see cref="IDataContext"/>, if any.</param>
        void ClearBindings([NotNull] object target, [NotNull] string path, IDataContext context = null);
    }
}