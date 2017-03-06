#region Copyright

// ****************************************************************************
// <copyright file="MvvmUtilsTest.cs">
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

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Models;
using Should;

namespace MugenMvvmToolkit.Test.Utils
{
    [TestClass]
    public class MvvmUtilsTest
    {
        [TestMethod]
        public void DefaultSettingValueTest()
        {
            ApplicationSettings.SetDefaultValues();
            
            ApplicationSettings.ViewModelObservationMode.ShouldEqual(ObservationMode.ParentObserveChild);
            ApplicationSettings.PropertyChangeExecutionMode.ShouldEqual(ExecutionMode.AsynchronousOnUiThread);            
        }
    }
}
