#region Copyright

// ****************************************************************************
// <copyright file="RelayCommandGenericTest.cs">
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
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.Models
{
    [TestClass]
    public class RelayCommandGenericTest : RelayCommandTest
    {
        #region Overrides of RelayCommandTest

        protected override RelayCommandBase CreateCommand(Action<object> execute, Func<object, bool> canExecute = null,
            params object[] items)
        {
            return new RelayCommand<object>(execute, canExecute, items);
        }

        #endregion
    }

    [TestClass]
    public class RelayCommandGenericTaskTest : RelayCommandTest
    {
        #region Overrides of RelayCommandTest

        protected override RelayCommandBase CreateCommand(Action<object> execute, Func<object, bool> canExecute = null,
            params object[] items)
        {
            return (RelayCommandBase)RelayCommandBase.FromAsyncHandler(o =>
            {
                execute(o);
                return Empty.Task;
            }, canExecute, true, items);
        }

        #endregion
    }
}
