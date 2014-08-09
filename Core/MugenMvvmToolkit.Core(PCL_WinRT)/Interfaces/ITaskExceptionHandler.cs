#region Copyright
// ****************************************************************************
// <copyright file="ITaskExceptionHandler.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents an interface that is responsible for handling exceptions in the task.
    /// </summary>
    public interface ITaskExceptionHandler
    {
        /// <summary>
        ///     Handles an exception.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="task">The task that throws an exception.</param>
        void Handle(object sender, Task task);
    }
}