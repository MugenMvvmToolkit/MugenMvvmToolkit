#region Copyright

// ****************************************************************************
// <copyright file="ViewClearedEventArgs.cs">
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
    public class ViewClearedEventArgs : ViewInitializedEventArgs
    {
        #region Constructors

        public ViewClearedEventArgs([NotNull] object view, [CanBeNull] IViewModel viewModel, [CanBeNull] IDataContext context) : base(view, viewModel, context)
        {
        }

        protected ViewClearedEventArgs()
        {
        }

        #endregion
    }
}