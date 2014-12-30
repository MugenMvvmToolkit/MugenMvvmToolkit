#region Copyright

// ****************************************************************************
// <copyright file="ViewMappingProviderEx.cs">
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
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the implementation of <see cref="IViewMappingProvider" /> to provide view mappings.
    /// </summary>
    public class ViewMappingProviderEx : ViewMappingProvider
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewMappingProvider" /> class.
        /// </summary>
        public ViewMappingProviderEx([NotNull] IEnumerable<Assembly> assemblies)
            : base(assemblies)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewMappingProvider" /> class.
        /// </summary>
        public ViewMappingProviderEx([NotNull] IEnumerable<Assembly> assemblies, IList<string> viewPostfix,
            IList<string> viewModelPostfix)
            : base(assemblies, viewPostfix, viewModelPostfix)
        {
        }

        #endregion

        #region Overrides of ViewMappingProvider

        /// <summary>
        ///     Defines the method that determines whether the type is view type.
        /// </summary>
        protected override bool IsViewType(Type type)
        {
            return typeof(Element).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
        }

        #endregion
    }
}