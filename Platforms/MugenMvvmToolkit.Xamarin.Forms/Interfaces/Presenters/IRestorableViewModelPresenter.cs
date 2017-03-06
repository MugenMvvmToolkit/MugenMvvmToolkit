#region Copyright

// ****************************************************************************
// <copyright file="IRestorableViewModelPresenter.cs">
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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;

namespace MugenMvvmToolkit.Xamarin.Forms.Interfaces.Presenters
{
    public interface IRestorableViewModelPresenter : IViewModelPresenter
    {
        Func<IDictionary<string, object>> GetStateDictionary { get; set; }

        void SaveState(IDataContext context = null);

        void ClearState(IDataContext context = null);

        bool TryRestore(IDataContext context = null);
    }
}