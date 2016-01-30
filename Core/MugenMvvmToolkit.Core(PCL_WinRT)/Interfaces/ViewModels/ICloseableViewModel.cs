#region Copyright

// ****************************************************************************
// <copyright file="ICloseableViewModel.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    public interface ICloseableViewModel : IViewModel
    {
        ICommand CloseCommand { get; set; }

        Task<bool> CloseAsync(object parameter = null);

        event EventHandler<ICloseableViewModel, ViewModelClosingEventArgs> Closing;

        event EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> Closed;
    }
}
