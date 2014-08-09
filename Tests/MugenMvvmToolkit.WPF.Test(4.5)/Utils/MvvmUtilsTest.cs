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
            ApplicationSettings.SynchronizedCollectionExecutionMode.ShouldEqual(ExecutionMode.AsynchronousOnUiThread);

            ApplicationSettings.ViewModelObservationMode.ShouldEqual(ObservationMode.ParentObserveChild);
            ApplicationSettings.PropertyChangeExecutionMode.ShouldEqual(ExecutionMode.AsynchronousOnUiThread);
            ApplicationSettings.NotificationCollectionMode.ShouldEqual(
                NotificationCollectionMode.CollectionIntefaceUseNotificationValue |
                NotificationCollectionMode.OnlyOnUiThread);
        }
    }
}