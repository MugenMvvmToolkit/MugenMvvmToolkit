#region Copyright

// ****************************************************************************
// <copyright file="WpfRootDynamicViewModelPresenter.cs">
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
using System.Threading.Tasks;
using System.Windows;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;

namespace MugenMvvmToolkit.WPF.Infrastructure.Presenters
{
    public class WpfRootDynamicViewModelPresenter : IDynamicViewModelPresenter
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public WpfRootDynamicViewModelPresenter()
        {
        }

        #endregion

        #region Properties

        public int Priority => int.MaxValue;

        public bool ShutdownOnMainViewModelClose { get; set; }

        #endregion

        #region Implementation of interfaces

        public virtual IAsyncOperation TryShowAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            parentPresenter.DynamicPresenters.Remove(this);
            var operation = parentPresenter.ShowAsync(context);
            if (ShutdownOnMainViewModelClose)
            {
                operation.ContinueWith(result =>
                {
                    var application = Application.Current;
                    if (application != null)
                    {
                        Action action = application.Shutdown;
                        application.Dispatcher.BeginInvoke(action);
                    }
                });
            }
            return operation;
        }

        public virtual Task<bool> TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            return null;
        }

        #endregion
    }
}