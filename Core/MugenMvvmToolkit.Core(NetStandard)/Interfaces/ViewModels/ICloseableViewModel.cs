#region Copyright

// ****************************************************************************
// <copyright file="ICloseableViewModel.cs">
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

using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    public interface ICloseableViewModel : IViewModel
    {
        ICommand CloseCommand { get; set; }

        [CanBeNull]
        Task<bool> OnClosingAsync([CanBeNull]IDataContext context);

        void OnClosed([CanBeNull]IDataContext context);
    }
}
