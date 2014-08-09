using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Test.Infrastructure
{
    [TestClass]
    public class AttachedValueProviderWindowsPhoneTest : AttachedValueProviderTestBase
    {
        #region Overrides of AttachedValueProviderTestBase

        protected override IAttachedValueProvider Create()
        {
            return new WeakReferenceAttachedValueProvider();
        }

        #endregion
    }
}