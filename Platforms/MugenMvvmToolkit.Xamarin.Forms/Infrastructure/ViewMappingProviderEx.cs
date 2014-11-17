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