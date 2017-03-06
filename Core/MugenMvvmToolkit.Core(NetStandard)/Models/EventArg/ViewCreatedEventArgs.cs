#region Copyright

// ****************************************************************************
// <copyright file="ViewCreatedEventArgs.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ViewCreatedEventArgs : ViewInitializedEventArgs
    {
        #region Constructors

        public ViewCreatedEventArgs([NotNull] object view, [CanBeNull] IViewModel viewModel, [NotNull] IViewMappingItem viewMappingItem, [CanBeNull] IDataContext context)
            : base(view, viewModel, context)
        {
            Should.NotBeNull(viewMappingItem, nameof(viewMappingItem));
            ViewMappingItem = viewMappingItem;
        }

        protected ViewCreatedEventArgs()
        {
        }

        #endregion

        #region Properties

        [NotNull]
        public IViewMappingItem ViewMappingItem { get; protected set; }

        #endregion
    }
}