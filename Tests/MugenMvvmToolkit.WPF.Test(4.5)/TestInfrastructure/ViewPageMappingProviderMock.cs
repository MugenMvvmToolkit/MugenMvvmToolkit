#region Copyright

// ****************************************************************************
// <copyright file="ViewPageMappingProviderMock.cs">
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
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class ViewPageMappingProviderMock : IViewMappingProvider
    {
        #region Properties

        public Func<Type, bool, IViewMappingItem> FindMappingForView { get; set; }
        public Func<Type, string, bool, IViewMappingItem> FindMappingForViewModel { get; set; }

        #endregion

        #region Implementation of IViewPageMappingProvider

        public IEnumerable<IViewMappingItem> ViewMappings { get; set; }

        IList<IViewMappingItem> IViewMappingProvider.FindMappingsForView(Type viewType, bool throwOnError)
        {
            return new[] {FindMappingForView(viewType, throwOnError)};
        }

        IViewMappingItem IViewMappingProvider.FindMappingForViewModel(Type viewModelType,
            string viewName,
            bool throwOnError)
        {
            return FindMappingForViewModel(viewModelType, viewName, throwOnError);
        }

        #endregion
    }
}
