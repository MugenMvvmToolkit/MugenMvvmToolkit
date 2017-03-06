#region Copyright

// ****************************************************************************
// <copyright file="IViewAwareViewModel.cs">
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

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    public interface IViewAwareViewModel<TView> : IViewModel where TView : class
    {
        [Preserve(Conditional = true)]
        TView View { get; set; }
    }
}
