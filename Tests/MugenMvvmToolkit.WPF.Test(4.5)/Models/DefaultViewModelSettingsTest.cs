#region Copyright

// ****************************************************************************
// <copyright file="DefaultViewModelSettingsTest.cs">
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

namespace MugenMvvmToolkit.Test.Models
{
    [TestClass]
    public class DefaultViewModelSettingsTest
    {
        [TestMethod]
        public void DefaultValueTest()
        {
            var settings = new DefaultViewModelSettings();
            settings.DefaultBusyMessage.ShouldEqual(string.Empty);

            settings.DisposeCommands.ShouldBeTrue();
            settings.HandleBusyMessageMode.ShouldEqual(HandleMode.Handle);
            settings.Metadata.ShouldNotBeNull();
            settings.State.ShouldNotBeNull();
        }
    }
}
