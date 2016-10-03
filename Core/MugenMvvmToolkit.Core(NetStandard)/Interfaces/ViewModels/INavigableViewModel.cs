#region Copyright

// ****************************************************************************
// <copyright file="INavigableViewModel.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Navigation;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    public interface INavigableViewModel : IViewModel
    {
        void OnNavigatedTo([NotNull] INavigationContext context);

        Task<bool> OnNavigatingFrom([NotNull] INavigationContext context);

        void OnNavigatedFrom([NotNull] INavigationContext context);
    }
}
