#region Copyright

// ****************************************************************************
// <copyright file="IBindingResourceResolver.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the dynamic resource resolver.
    /// </summary>
    public interface IBindingResourceResolver
    {
        /// <summary>
        ///     Gets or sets the name of self element resource default is <c>self</c>.
        /// </summary>
        [NotNull]
        string SelfResourceName { get; set; }

        /// <summary>
        ///     Gets or sets the name of root element resource default is <c>root</c>.
        /// </summary>
        [NotNull]
        string RootElementResourceName { get; set; }

        /// <summary>
        ///     Gets or sets the name of binding source resource default is <c>src</c>.
        /// </summary>
        [NotNull]
        string BindingSourceResourceName { get; set; }

        /// <summary>
        ///     Gets or sets the name of data context resource default is <c>context</c>.
        /// </summary>
        [NotNull]
        string DataContextResourceName { get; set; }

        /// <summary>
        ///     Gets a collection of known types.
        /// </summary>
        [NotNull]
        IList<Type> GetKnownTypes();

        /// <summary>
        ///     Gets an instance of <see cref="IBindingValueConverter" /> by the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <param name="context">The specified data context, if any.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the converter cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="IBindingValueConverter" />.</returns>
        IBindingValueConverter ResolveConverter([NotNull] string name, IDataContext context, bool throwOnError);

        /// <summary>
        ///     Gets an instance of <see cref="Type" /> by the specified name.
        /// </summary>
        /// <param name="typeName">The specified name.</param>
        /// <param name="context">The specified data context, if any.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="Type" />.</returns>
        Type ResolveType([NotNull] string typeName, IDataContext context, bool throwOnError);

        /// <summary>
        ///     Gets an instance of <see cref="IBindingResourceMethod" /> by the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <param name="context">The specified data context, if any.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the method cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="IBindingResourceMethod" />.</returns>
        IBindingResourceMethod ResolveMethod([NotNull] string name, IDataContext context, bool throwOnError);

        /// <summary>
        ///     Gets an instance of <see cref="ISourceValue" /> by the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <param name="context">The specified data context, if any.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the object cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="ISourceValue" />.</returns>
        ISourceValue ResolveObject([NotNull] string name, IDataContext context, bool throwOnError);

        /// <summary>
        ///     Gets an instance of <see cref="IBindingBehavior" /> by the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <param name="context">The specified data context, if any.</param>
        /// <param name="args">The specified args to create behavior.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the object cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="IBindingBehavior" />.</returns>
        IBindingBehavior ResolveBehavior([NotNull] string name, IDataContext context, IList<object> args, bool throwOnError);

        /// <summary>
        ///     Adds the specified behavior.
        /// </summary>
        void AddBehavior([NotNull] string name, [NotNull] Func<IDataContext, IList<object>, IBindingBehavior> getBehavior, bool rewrite = true);

        /// <summary>
        ///     Adds the specified converter.
        /// </summary>
        void AddConverter([NotNull] string name, [NotNull] IBindingValueConverter converter, bool rewrite = true);

        /// <summary>
        ///     Adds the specified type.
        /// </summary>
        void AddType([NotNull] string name, [NotNull] Type type, bool rewrite = true);

        /// <summary>
        ///     Adds the specified method.
        /// </summary>
        void AddMethod([NotNull] string name, [NotNull] IBindingResourceMethod method, bool rewrite = true);

        /// <summary>
        ///     Adds the specified object.
        /// </summary>
        void AddObject([NotNull] string name, [NotNull] ISourceValue obj, bool rewrite = true);

        /// <summary>
        ///     Removes the specified behavior using name.
        /// </summary>
        bool RemoveBehavior(string name);

        /// <summary>
        ///     Removes the specified converter using name.
        /// </summary>
        bool RemoveConverter(string name);

        /// <summary>
        ///     Removes the specified type using name.
        /// </summary>
        bool RemoveType(string name);

        /// <summary>
        ///     Removes the specified method using name.
        /// </summary>
        bool RemoveMethod(string name);

        /// <summary>
        ///     Removes the specified object using name.
        /// </summary>
        bool RemoveObject(string name);
    }
}