#region Copyright

// ****************************************************************************
// <copyright file="ViewModelClosingEventArgs.cs">
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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ViewModelClosingEventArgs : ViewModelClosedEventArgs
    {
        #region Fields

        private List<Task<bool>> _deferrals;

        #endregion

        #region Constructors

        public ViewModelClosingEventArgs([NotNull]IViewModel viewModel, [CanBeNull]IDataContext context)
            : base(viewModel, context)
        {
        }

        #endregion

        #region Properties

        public bool Cancel { get; set; }

        #endregion

        #region Methods

        public void AddCancelTask(Task<bool> cancelTask)
        {
            if (cancelTask == null)
                return;
            if (_deferrals == null)
                Interlocked.CompareExchange(ref _deferrals, new List<Task<bool>>(), null);
            lock (_deferrals)
                _deferrals.Add(cancelTask);
        }

        public Task<bool> GetCanCloseAsync()
        {
            if (Cancel)
                return MugenMvvmToolkit.Empty.FalseTask;
            if (_deferrals == null)
                return MugenMvvmToolkit.Empty.TrueTask;
            Task<bool>[] tasks;
            lock (_deferrals)
                tasks = _deferrals.ToArray();
            return ToolkitExtensions.WhenAll(tasks).ContinueWith(task =>
            {
                return tasks.All(t => !t.Result);
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        #endregion
    }
}
