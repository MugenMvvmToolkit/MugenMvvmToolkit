#region Copyright

// ****************************************************************************
// <copyright file="ViewMappingProviderEx.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Infrastructure;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure
{
    public class ViewMappingProviderEx : ViewMappingProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewMappingProviderEx([NotNull] IEnumerable<Assembly> assemblies)
            : base(assemblies)
        {
        }

        [Preserve(Conditional = true)]
        public ViewMappingProviderEx([NotNull] IEnumerable<Assembly> assemblies, IList<string> viewPostfix,
            IList<string> viewModelPostfix)
            : base(assemblies, viewPostfix, viewModelPostfix)
        {
        }

        #endregion

        #region Overrides of ViewMappingProvider

        protected override bool IsViewType(Type type)
        {
            return typeof(Element).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
        }

        #endregion
    }
}
