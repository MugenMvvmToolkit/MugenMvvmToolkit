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
}