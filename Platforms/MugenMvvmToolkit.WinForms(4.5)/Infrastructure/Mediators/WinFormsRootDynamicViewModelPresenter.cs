#region Copyright

// ****************************************************************************
// <copyright file="WinFormsRootDynamicViewModelPresenter.cs">
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
using System.Windows.Forms;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;

namespace MugenMvvmToolkit.WinForms.Infrastructure.Mediators
{
    public class WinFormsRootDynamicViewModelPresenter : IDynamicViewModelPresenter
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public WinFormsRootDynamicViewModelPresenter()
        {
        }

        #endregion

        #region Properties

        public bool AutoRunApplication { get; set; }

        public bool ShutdownOnMainViewModelClose { get; set; }

        public int Priority => int.MaxValue - 1;

        #endregion

        #region Implementation of interfaces

        public virtual IAsyncOperation TryShowAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            parentPresenter.DynamicPresenters.Remove(this);
            var operation = parentPresenter.ShowAsync(context);
            if (ShutdownOnMainViewModelClose)
                operation.ContinueWith(result => Application.Exit());
            if (AutoRunApplication)
                Application.Run();
            return operation;
        }

        public virtual Task<bool> TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            return null;
        }

        #endregion
    }
}