using System;

namespace MugenMvvm.Interfaces
{
    public interface IWeakReferenceFactory
    {
        WeakReference CreateWeakReference(object item);//todo change type to WeakReferenceDescriptor
    }
}