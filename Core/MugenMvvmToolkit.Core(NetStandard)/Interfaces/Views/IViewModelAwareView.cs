#region Copyright

// ****************************************************************************
// <copyright file="IViewModelAwareView.cs">
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

using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces.Views
{
    public interface IViewModelAwareView<TViewModel> : IView where TViewModel : IViewModel
    {
        [Preserve(Conditional = true)]
        TViewModel ViewModel { get; set; }
    }
}
