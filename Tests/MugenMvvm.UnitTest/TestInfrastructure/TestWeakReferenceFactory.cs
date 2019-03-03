using System;
using MugenMvvm.Interfaces;

namespace MugenMvvm.UnitTest.TestInfrastructure
{
    public class TestWeakReferenceFactory : IWeakReferenceFactory
    {
        #region Implementation of interfaces

        public WeakReference GetWeakReference(object item)
        {
            return new WeakReference(item, false);
        }

        #endregion
    }
}