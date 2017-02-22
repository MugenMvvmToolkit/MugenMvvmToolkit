#region Copyright

// ****************************************************************************
// <copyright file="NavigationOperation.cs">
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
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    public class NavigationOperation : AsyncOperation<bool>, INavigationOperation//todo fix type
    {
        #region Fields

        private Task _task;

        #endregion

        #region Constructors

        public NavigationOperation()
            : this(Empty.Task)
        {
        }

        public NavigationOperation(Task task)
        {
            Should.NotBeNull(task, nameof(task));
            _task = task;
        }

        #endregion

        #region Implementation of INavigationOperation

        public Task NavigationCompletedTask => _task;

        #endregion

        #region Methods

        public void SetNavigationCompletedTask(Task task)
        {
            _task = task;
        }

        #endregion
    }
}
