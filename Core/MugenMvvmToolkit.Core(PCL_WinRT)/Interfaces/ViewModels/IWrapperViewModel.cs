#region Copyright

// ****************************************************************************
// <copyright file="IWrapperViewModel.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    public interface IWrapperViewModel : IViewModel
    {
        IViewModel ViewModel { get; }

        void Wrap([NotNull] IViewModel viewModel, [CanBeNull] IDataContext context);
    }
}
