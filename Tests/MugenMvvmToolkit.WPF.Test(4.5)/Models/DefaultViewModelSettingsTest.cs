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
