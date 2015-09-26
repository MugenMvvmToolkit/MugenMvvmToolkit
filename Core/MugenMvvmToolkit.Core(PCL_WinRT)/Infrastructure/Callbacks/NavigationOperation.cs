#region Copyright

// ****************************************************************************
// <copyright file="NavigationOperation.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
    public class NavigationOperation : AsyncOperation<bool>, INavigationOperation
    {
        #region Fields

        private readonly Task _task;

        #endregion

        #region Constructors

        public NavigationOperation()
            : this(Empty.Task)
        {
        }

        public NavigationOperation(Task task)
        {
            Should.NotBeNull(task, "task");
            _task = task;
        }

        #endregion

        #region Implementation of INavigationOperation

        public Task NavigationCompletedTask
        {
            get { return _task; }
        }

        #endregion
    }
}
